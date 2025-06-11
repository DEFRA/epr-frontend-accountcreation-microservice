using System.Linq.Expressions;
using System.Reflection;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;

/// <summary>
/// Helper class for Lazy ViewModel accessor initialization
/// </summary>
/// <typeparam name="TViewModelLocal"></typeparam>
/// <typeparam name="TProperty"></typeparam>
internal class ViewModelPropertyAccessors<TViewModelLocal, TProperty>
    where TViewModelLocal : class
{
    public string PropertyName { get; }
    public Func<TViewModelLocal, TProperty> Getter { get; }
    public Action<TViewModelLocal, TProperty> Setter { get; }

    public ViewModelPropertyAccessors(
        Expression<Func<TViewModelLocal, TProperty>> propertyExpression)
    {
        if (!(propertyExpression.Body is MemberExpression memberExpression))
        {
            throw new ArgumentException($"Expected {nameof(propertyExpression)} to be a MemberExpression (e.g., vm => vm.Property), but it was {propertyExpression.Body.NodeType}", nameof(propertyExpression));
        }

        if (!(memberExpression.Member is PropertyInfo propertyInfo))
        {
            throw new ArgumentException($"The member in {nameof(propertyExpression)} must be a Property.", nameof(propertyExpression));
        }

        PropertyName = propertyInfo.Name;
        Getter = propertyExpression.Compile();

        if (!propertyInfo.CanWrite)
        {
            throw new InvalidOperationException($"Property '{propertyInfo.Name}' on type '{typeof(TViewModelLocal).FullName}' must be writable.");
        }

        var viewModelParam = propertyExpression.Parameters[0];
        var valueParam = Expression.Parameter(typeof(TProperty), "value");

        if (propertyInfo.PropertyType != typeof(TProperty))
        {
            // This check is crucial if TProperty could be different from the actual property type.
            // For YesNoAnswer?, it should match.
            throw new InvalidOperationException(
               $"Property '{propertyInfo.Name}' on type '{typeof(TViewModelLocal).FullName}' is of type '{propertyInfo.PropertyType.Name}' " +
               $"but expected '{typeof(TProperty).Name}'.");
        }

        var assignExpression = Expression.Assign(
            Expression.Property(viewModelParam, propertyInfo),
            valueParam
        );

        Setter = Expression.Lambda<Action<TViewModelLocal, TProperty>>(
            assignExpression,
            viewModelParam,
            valueParam
        ).Compile();
    }
}
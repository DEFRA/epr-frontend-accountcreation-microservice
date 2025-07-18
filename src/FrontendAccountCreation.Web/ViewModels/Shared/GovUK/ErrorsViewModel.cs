using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace FrontendAccountCreation.Web.ViewModels.Shared.GovUK;

//to-do: shouldn't be excluded
[ExcludeFromCodeCoverage]
public class ErrorsViewModel
{
    private ErrorsViewModel(List<(string Key, List<ErrorViewModel> Errors)> errors, Func<string, string> localiseFunc, params string[]? fieldOrder)
    {
        Errors = GetOrderedErrors(errors, localiseFunc, fieldOrder);
    }

    private ErrorsViewModel(List<(string Key, List<ErrorViewModel> Errors)> errors, Func<string, string> localiseFunc, Dictionary<string, string[]> fieldOrder)
    {
        Errors = GetNestedOrderedErrors(errors, localiseFunc, fieldOrder);
    }

    private static List<(string Key, List<ErrorViewModel> Errors)> GetNestedOrderedErrors(
        List<(string Key, List<ErrorViewModel> Errors)> errors, Func<string, string> localiseFunc, Dictionary<string, string[]> fieldOrder)
    {
        // Extracted helper method to reduce complexity
        static (int baseIdx, int? index, int subIdx, string key) GetOrderParts(string key, Dictionary<string, string[]> fieldOrder)
        {
            var (baseKey, idx) = ExtractBaseKeyAndIndex(key);
            var baseIdx = GetBaseIndex(baseKey, fieldOrder);
            var subIdx = GetSubIndex(key, baseKey, fieldOrder);

            return (baseIdx, idx, subIdx, key);
        }

        static (string baseKey, int? index) ExtractBaseKeyAndIndex(string key)
        {
            var dotIdx = key.IndexOf('.');
            string main = dotIdx >= 0 ? key.Substring(0, dotIdx) : key;

            int bracketStart = main.IndexOf('[');
            int? idx = null;
            string baseKey = main;
            if (bracketStart >= 0)
            {
                int bracketEnd = main.IndexOf(']', bracketStart);
                if (bracketEnd > bracketStart)
                {
                    if (int.TryParse(main.Substring(bracketStart + 1, bracketEnd - bracketStart - 1), out int parsedIdx))
                        idx = parsedIdx;
                    baseKey = main.Substring(0, bracketStart);
                }
            }

            return (baseKey, idx);
        }

        static int GetBaseIndex(string baseKey, Dictionary<string, string[]> fieldOrder)
        {
            var baseKeys = fieldOrder.Keys.ToList();
            int baseIdx = baseKeys.IndexOf(baseKey);
            return baseIdx == -1 ? int.MaxValue : baseIdx;
        }

        static int GetSubIndex(string key, string baseKey, Dictionary<string, string[]> fieldOrder)
        {
            var dotIdx = key.IndexOf('.');
            string? sub = dotIdx >= 0 ? key.Substring(dotIdx + 1) : null;

            int subIdx = int.MaxValue;
            if (sub != null && fieldOrder.TryGetValue(baseKey, out var subProps) && subProps.Length > 0)
            {
                var subPart = sub.Split('.')[0];
                int idx2 = Array.IndexOf(subProps, subPart);
                if (idx2 >= 0) subIdx = idx2;
            }

            return subIdx;
        }

        var ordered = errors
            .OrderBy(e => GetOrderParts(e.Key, fieldOrder).baseIdx)
            .ThenBy(e => GetOrderParts(e.Key, fieldOrder).index ?? -1)
            .ThenBy(e => GetOrderParts(e.Key, fieldOrder).subIdx)
            .ThenBy(e => e.Key);

        var result = new List<(string Key, List<ErrorViewModel> Errors)>();
        foreach (var kvp in ordered)
        {
            kvp.Errors.ForEach(x => x.Message = localiseFunc(x.Message));
            result.Add((kvp.Key, kvp.Errors));
        }
        return result;
    }

    private static List<(string Key, List<ErrorViewModel> Errors)> GetOrderedErrors(
       List<(string Key, List<ErrorViewModel> Errors)> errors, Func<string, string> localiseFunc, string[]? fieldOrder)
    {
        fieldOrder ??= Array.Empty<string>();
        var orderedErrorsKvp = errors.OrderBy(x => fieldOrder.Contains(x.Item1) ? Array.IndexOf(fieldOrder, x.Item1) : int.MaxValue);
        var orderedErrors = new List<(string Key, List<ErrorViewModel> Errors)>();
        foreach (var kvp in orderedErrorsKvp)
        {
            kvp.Item2.ForEach(x => x.Message = localiseFunc(x.Message));
            orderedErrors.Add((kvp.Key, kvp.Errors));
        }
        
        return orderedErrors;
    }

    public ErrorsViewModel(List<(string Key, List<ErrorViewModel> Errors)> errors,
        IStringLocalizer<SharedResources> localizer)
        : this(errors, (x) => localizer[x].Value)
    {
    }

    public ErrorsViewModel(List<(string Key, List<ErrorViewModel> Errors)> errors, IViewLocalizer localizer,
        params string[] fieldOrder)
        : this(errors, (x) => localizer[x].Value, fieldOrder)
    {
    }

    public ErrorsViewModel(List<(string Key, List<ErrorViewModel> Errors)> errors, IViewLocalizer localizer,
    Dictionary<string, string[]> fieldOrder)
    : this(errors, (x) => localizer[x].Value, fieldOrder)
    {
    }

    public List<(string Key, List<ErrorViewModel> Errors)> Errors { get; }

    public List<ErrorViewModel>? this[string key] => Errors.Find(e => e.Key == key).Errors;

    public bool HasErrorKey(string key) => Errors.Exists(e => e.Item1 == key);
}
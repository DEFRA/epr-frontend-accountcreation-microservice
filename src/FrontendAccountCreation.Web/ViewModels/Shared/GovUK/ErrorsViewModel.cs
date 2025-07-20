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
        static (string baseKey, int? index) ExtractBaseKeyAndIndex(string key)
        {
            int dotIdx = key.IndexOf('.');
            string main = dotIdx >= 0 ? key[..dotIdx] : key;

            int bracketStart = main.IndexOf('[');
            int? idx = null;
            string baseKey = main;
            if (bracketStart >= 0)
            {
                int bracketEnd = main.IndexOf(']', bracketStart);
                if (bracketEnd > bracketStart)
                {
                    if (int.TryParse(main[(bracketStart + 1)..bracketEnd], out int parsedIdx))
                        idx = parsedIdx;
                    baseKey = main[..bracketStart];
                }
            }
            return (baseKey, idx);
        }

        static int GetBaseIndex(string baseKey, Dictionary<string, string[]> fieldOrder)
        {
            int baseIdx = fieldOrder.Keys.ToList().IndexOf(baseKey);
            return baseIdx == -1 ? int.MaxValue : baseIdx;
        }

        static int GetSubIndex(string key, string baseKey, Dictionary<string, string[]> fieldOrder)
        {
            int dotIdx = key.IndexOf('.');
            if (dotIdx < 0) return int.MaxValue;

            string sub = key[(dotIdx + 1)..];
            if (fieldOrder.TryGetValue(baseKey, out var subProps) && subProps.Length > 0)
            {
                string subPart = sub.Split('.')[0];
                int idx2 = Array.IndexOf(subProps, subPart);
                if (idx2 >= 0) return idx2;
            }
            return int.MaxValue;
        }

        static (int baseIdx, int? index, int subIdx) GetOrderParts(string key, Dictionary<string, string[]> fieldOrder)
        {
            var (baseKey, idx) = ExtractBaseKeyAndIndex(key);
            int baseIdx = GetBaseIndex(baseKey, fieldOrder);
            int subIdx = GetSubIndex(key, baseKey, fieldOrder);
            return (baseIdx, idx, subIdx);
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
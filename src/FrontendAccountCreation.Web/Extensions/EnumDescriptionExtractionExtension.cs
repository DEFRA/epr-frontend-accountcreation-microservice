using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace FrontendAccountCreation.Web.Extensions
{
    public static class EnumDescriptionExtractionExtension
    {
        private static readonly ConcurrentDictionary<string, string> DisplayNameCache = new();

        /// <summary>
        /// Gets enum description value as string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescrption(this Enum value)
        {
            var key = $"{value.GetType().FullName}.{value}";

            var displayName = DisplayNameCache.GetOrAdd(key, x =>
            {
                var name = (DescriptionAttribute[])value
                    .GetType()
                    .GetTypeInfo()
                    .GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false);

                return name.Length > 0 ? name[0].Description : value.ToString();
            });

            return displayName;
        }
    }
}

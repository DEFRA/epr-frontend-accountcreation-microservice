using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontendAccountCreation.IntegrationTests.Resources
{
    public class TestRequestCookieCollection : Dictionary<string, string>, IRequestCookieCollection
    {
        public new ICollection<string> Keys => base.Keys;
        public new string this[string key]
        {
            get
            {
                TryGetValue(key, out var value);
                return value;
            }
            set
            {
                base[key] = value;
            }
        }
    }
}

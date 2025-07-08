using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontendAccountCreation.Core.Sessions
{
    public enum PartnerType
    {
        [Description("Individual Partner")]
        IndividualPartner = 1,

        [Description("Corporate Partner")]
        CorporatePartner = 2,
    }
}

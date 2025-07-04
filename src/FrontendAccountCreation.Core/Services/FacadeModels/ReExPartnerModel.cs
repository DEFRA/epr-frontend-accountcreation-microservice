using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontendAccountCreation.Core.Services.FacadeModels
{
    [ExcludeFromCodeCoverage(Justification = "Model no business logic")]
    public class ReExPartnerModel
    {
        public string Name { get; set; }

        public string PartnerRole { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace TitanTracker.Extensions
{
    public static class IdentityExtensions
    {
        public static int? GetCompanyId(this IIdentity identity)
        {
            Claim claim = ((ClaimsIdentity)identity).FindFirst("CompanyId");
            return (claim != null) ? int.Parse(claim.Value) : null;

            //Above Ternary opetator explanation

            //int result;
            //if (claim != null)
            //{
            // result = int.Parse(claim.Value);
            //}
            //result = 0
            //}
            //return result;
        }
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ProbSolv.Models;
using System.Security.Claims;

namespace ProbSolv.Services.Factories
{
    public class PSUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<PSUser, IdentityRole>
    {
        public PSUserClaimsPrincipalFactory(UserManager<PSUser> userManager, 
                                            RoleManager<IdentityRole> roleManager, 
                                            IOptions<IdentityOptions> options) 
        : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(PSUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));

            return identity;
        }

    }
}

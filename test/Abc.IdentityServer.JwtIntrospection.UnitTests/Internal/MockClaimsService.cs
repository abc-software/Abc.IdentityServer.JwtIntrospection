using System.Security.Claims;

#if DUENDE
namespace Duende.IdentityServer.Services
#else
namespace IdentityServer4.Services
#endif
{
    internal class MockClaimsService : IClaimsService
    {
        public List<Claim> IdentityTokenClaims { get; set; } = new List<Claim>();
        public List<Claim> AccessTokenClaims { get; set; } = new List<Claim>();

        public Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, ResourceValidationResult resources, bool includeAllIdentityClaims, ValidatedRequest request)
        {
            return Task.FromResult(IdentityTokenClaims.AsEnumerable());
        }

        public Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, ResourceValidationResult resources, ValidatedRequest request)
        {
            return Task.FromResult(AccessTokenClaims.AsEnumerable());
        }
    }
}

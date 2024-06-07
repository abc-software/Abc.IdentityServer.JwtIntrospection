using Microsoft.AspNetCore.Http;

namespace Abc.IdentityServer.Endpoints
{
    internal class StubClientSecretValidator : IClientSecretValidator
    {
        public ClientSecretValidationResult Result { get; set; }

        public Task<ClientSecretValidationResult> ValidateAsync(HttpContext context)
        {
            return Task.FromResult(Result);
        }
    }
}

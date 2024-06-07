using Microsoft.AspNetCore.Http;

namespace Abc.IdentityServer.Endpoints
{
    internal class StubApiSecretValidator : IApiSecretValidator
    {
        public ApiSecretValidationResult Result { get; set; }

        public Task<ApiSecretValidationResult> ValidateAsync(HttpContext context)
        {
            return Task.FromResult(Result);
        }
    }
}

#if DUENDE
namespace Duende.IdentityServer.Services
#else
namespace IdentityServer4.Services
#endif
{
    internal class MockTokenCreationService : ITokenCreationService
    {
        public string TokenResult { get; set; }
        public Token Token { get; set; }

        public Task<string> CreateTokenAsync(Token token)
        {
            Token = token;
            return Task.FromResult(TokenResult);
        }
    }
}

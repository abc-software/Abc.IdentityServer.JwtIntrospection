#if DUENDE

namespace Duende.IdentityServer.Services;

internal class TestIssuerNameService : IIssuerNameService
{
    private readonly string _value;

    public TestIssuerNameService(string? value = null)
    {
        _value = value ?? "https://identityserver";
    }

    public Task<string> GetCurrentAsync()
    {
        return Task.FromResult(_value);
    }
}

#endif
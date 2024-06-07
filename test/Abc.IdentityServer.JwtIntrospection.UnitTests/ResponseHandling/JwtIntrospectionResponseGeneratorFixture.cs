using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Abc.IdentityServer.ResponseHandling.UnitTests
{
    public class JwtIntrospectionResponseGeneratorFixture
    {
        private ILogger<IntrospectionResponseGenerator> _logger = TestLogger.Create<IntrospectionResponseGenerator>();
        private TestEventService _events = new TestEventService();
        private JwtIntrospectionResponseGenerator _target;
        private MockTokenCreationService _tokenCreation = new MockTokenCreationService();

#if DUENDE
        private TestIssuerNameService _issuerNameService = new TestIssuerNameService();
#else
        private MockHttpContextAccessor _contextAncessor = new MockHttpContextAccessor();
#endif

        public JwtIntrospectionResponseGeneratorFixture()
        {
#if DUENDE
             _target = new JwtIntrospectionResponseGenerator(
                _issuerNameService,
                _events,
                _tokenCreation,
                _logger
                );
#else
            _target = new JwtIntrospectionResponseGenerator(
                _contextAncessor,
                _events,
                _tokenCreation,
                _logger
                );
#endif
        }

        [Fact()]
        public async Task introspection_not_active()
        {
            // Arrange
            var validationResult = new IntrospectionRequestValidationResult()
            {
                IsActive = false,
                Parameters = new System.Collections.Specialized.NameValueCollection(),
                Api = new ApiResource("api"),
            };

            // Act
            var result = await _target.ProcessAsync(validationResult);

            // Assert
            var expected = new Dictionary<string, object>()
            {
                { "active", false }
            };

            result.Should().BeEquivalentTo(expected); 
        }

        [Fact()]
        public async Task introspection_active()
        {
            // Arrange
            var api = new ApiResource("api");
            api.Scopes.Add("sc1");

            var validationResult = new IntrospectionRequestValidationResult()
            {
                IsActive = true,
                Parameters = new System.Collections.Specialized.NameValueCollection(),
                Api = api,
                Claims = new Claim[]
                {
                    new Claim("scope", "sc1")
                },
            };

            // Act
            var result = await _target.ProcessAsync(validationResult);

            // Assert
            var expected = new Dictionary<string, object>()
            {
                { "scope", "sc1" },
                { "active", true }
            };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact()]
        public async Task jwt_introspection_active()
        {
            // Arrange
            var api = new ApiResource("api");
            api.Scopes.Add("sc1");

            var validationResult = new IntrospectionRequestValidationResult()
            {
                IsActive = true,
                Parameters = new System.Collections.Specialized.NameValueCollection() { { "ContentType", "application/token-introspection+jwt" } },
                Api = api,
                Claims = new Claim[]
                {
                    new Claim("scope", "sc1")
                },
            };

            _tokenCreation.TokenResult = "some_token";

            // Act
            var result = await _target.ProcessAsync(validationResult);

            // Assert
            var expected = new Dictionary<string, object>()
            {
                {"introspection_token", "some_token" },
            };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact()]
        public async Task jwt_introspection_v7_active()
        {
            // Arrange
            var api = new ApiResource("api");
            api.Scopes.Add("sc1");

            var validationResult = new IntrospectionRequestValidationResult()
            {
                IsActive = true,
                Parameters = new System.Collections.Specialized.NameValueCollection() { { "ContentType", "application/jwt" } },
                Api = api,
                Claims = new Claim[]
                {
                    new Claim("scope", "sc1")
                },
            };

            _tokenCreation.TokenResult = "some_token_v7";

            // Act
            var result = await _target.ProcessAsync(validationResult);

            // Assert
            var expected = new Dictionary<string, object>()
            {
                {"introspection_token_v7", "some_token_v7" },
            };

            result.Should().BeEquivalentTo(expected);
        }
    }
}
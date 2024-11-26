#nullable disable

using Abc.IdentityServer.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityModel;
using System.Text.Json;
using System.Linq;

namespace Abc.IdentityServer.Services.UnitTests
{
    public class AbcTokenCreationServiceFixture
    {
#if DUENDE && NET8_0_OR_GREATER
        private Duende.IdentityServer.StubClock _stubClock = new();
#else
        private Microsoft.AspNetCore.Authentication.StubSystemClock _stubClock = new();
#endif
        private MockKeyMaterialService _keys = new MockKeyMaterialService();
        private AbcIdentityServerOptions _options = new AbcIdentityServerOptions();
        
        private AbcTokenCreationService _target;

        public AbcTokenCreationServiceFixture()
        {
            _options.IssuerUri = "https://test.identityserver.io";
            _options.AccessTokenJwtType = "http://openid.net/specs/jwt/1.0";

            var svcs = new ServiceCollection();
            svcs.AddSingleton(_options);

            var signingKey = new SigningCredentials(CryptoHelper.CreateRsaSecurityKey(), SecurityAlgorithms.RsaSha256);
            _keys.SigningCredentials.Add(signingKey);

            _target = new AbcTokenCreationService(
                _stubClock,
                _keys,
                _options,
                TestLogger.Create<DefaultTokenCreationService>());
        }

        [Fact()]
        public async Task token()
        {
            // Arrange
            var currentTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _stubClock.UtcNowFunc = () => currentTime;

            var token = new Token()
            {
                Lifetime = 60 * 60 * 2, // 2h
                // TODO: CreationTime = currentTime.AddSeconds(1),
            };

            // Act
            var result = await _target.CreateTokenAsync(token);
            result.Should().NotBeNull();
            var jwt = new JwtSecurityToken(result);

            // Assert
            jwt.Header["alg"].Should().Be("RS256");
            jwt.Header["typ"].Should().Be("http://openid.net/specs/jwt/1.0");

            jwt.SignatureAlgorithm.Should().Be("RS256");
            jwt.ValidFrom.Should().Be(currentTime);
            jwt.ValidTo.Should().Be(currentTime.AddHours(2));
            jwt.IssuedAt.Should().Be(currentTime);

            jwt.Claims.Should().HaveCount(3);
        }

        [Fact()]
        public async Task token_with_claims_use_lifetime_ignore_claims()
        {
            // Arrange
            var currentTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _stubClock.UtcNowFunc = () => currentTime;

            var token = new Token()
            {
                Lifetime = 60 * 60 * 2, // 2h
                Claims = new List<Claim>()
                {
                    new Claim(JwtClaimTypes.NotBefore, currentTime.AddMinutes(-1).ToEpochTime().ToString(), ClaimValueTypes.Integer),
                    new Claim(JwtClaimTypes.Expiration, currentTime.AddHours(2).AddMinutes(-1).ToEpochTime().ToString(), ClaimValueTypes.Integer),
                },
            };

            // Act
            var result = await _target.CreateTokenAsync(token);
            result.Should().NotBeNull();
            var jwt = new JwtSecurityToken(result);

            // Assert
            jwt.Header["alg"].Should().Be("RS256");
            jwt.Header["typ"].Should().Be("http://openid.net/specs/jwt/1.0");

            jwt.SignatureAlgorithm.Should().Be("RS256");
            jwt.ValidFrom.Should().Be(currentTime);
            jwt.ValidTo.Should().Be(currentTime.AddHours(2));
            jwt.IssuedAt.Should().Be(currentTime);

            jwt.Claims.Should().HaveCount(3);
        }

        [Fact()]
        public async Task token_with_issuer()
        {
            // Arrange
            var currentTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _stubClock.UtcNowFunc = () => currentTime;

            var token = new Token()
            {
                Issuer = _options.IssuerUri,
                Lifetime = 60 * 60 * 2, // 2h
            };

            // Act
            var result = await _target.CreateTokenAsync(token);
            result.Should().NotBeNull();
            var jwt = new JwtSecurityToken(result);

            // Assert
            jwt.Header["alg"].Should().Be("RS256");
            jwt.Header["typ"].Should().Be("http://openid.net/specs/jwt/1.0");

            jwt.SignatureAlgorithm.Should().Be("RS256");
            jwt.ValidFrom.Should().Be(currentTime);
            jwt.ValidTo.Should().Be(currentTime.AddHours(2));
            jwt.IssuedAt.Should().Be(currentTime);
            jwt.Issuer.Should().Be("https://test.identityserver.io");

            jwt.Claims.Should().HaveCount(4);
        }

        [Fact()]
        public async Task introspection_token_v7_with_issuer()
        {
            // Arrange
            var currentTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _stubClock.UtcNowFunc = () => currentTime;

            var token = new Token()
            {
                Type = "introspection_token_v7",
                Issuer = _options.IssuerUri,
                Lifetime = 60 * 60 * 2, // 2h
            };

            // Act
            var result = await _target.CreateTokenAsync(token);
            result.Should().NotBeNull();
            var jwt = new JwtSecurityToken(result);

            // Assert
            jwt.Header["alg"].Should().Be("RS256");
            jwt.Header["typ"].Should().Be("token-introspection+jwt");

            jwt.SignatureAlgorithm.Should().Be("RS256");
            jwt.ValidFrom.Should().Be(currentTime);
            jwt.ValidTo.Should().Be(currentTime.AddHours(2));
            jwt.IssuedAt.Should().Be(currentTime);
            jwt.Issuer.Should().Be("https://test.identityserver.io");

            jwt.Claims.Should().HaveCount(4);
        }

        [Fact()]
        public async Task introspection_token_v7_with_claims_ignore_lifetime()
        {
            // Arrange
            var currentTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _stubClock.UtcNowFunc = () => currentTime;

            var token = new Token()
            {
                Type = "introspection_token_v7",
                Lifetime = 1, // 1s
                Claims = new List<Claim>()
                {
                    new Claim(JwtClaimTypes.NotBefore, currentTime.AddMinutes(-1).ToEpochTime().ToString(), ClaimValueTypes.Integer),
                    new Claim(JwtClaimTypes.Expiration, currentTime.AddHours(2).AddMinutes(-1).ToEpochTime().ToString(), ClaimValueTypes.Integer),
                    new Claim("active", "true", ClaimValueTypes.Boolean),
                },
            };

            // Act
            var result = await _target.CreateTokenAsync(token);
            result.Should().NotBeNull();
            var jwt = new JwtSecurityToken(result);

            // Assert
            jwt.Header["alg"].Should().Be("RS256");
            jwt.Header["typ"].Should().Be("token-introspection+jwt");

            jwt.SignatureAlgorithm.Should().Be("RS256");
            jwt.ValidFrom.Should().Be(currentTime.AddMinutes(-1));
            jwt.ValidTo.Should().Be(currentTime.AddHours(2).AddMinutes(-1));
            jwt.IssuedAt.Should().Be(currentTime);
            jwt.Issuer.Should().BeNull();

            jwt.Claims.Should().HaveCount(4);

#if DUENDE && NET8_0_OR_GREATER
            var claimType = ClaimValueTypes.Integer64;
#else
            var claimType = ClaimValueTypes.Integer;
#endif

            var expectedClaims = new Claim[] {
                new Claim(JwtClaimTypes.NotBefore, currentTime.AddMinutes(-1).ToEpochTime().ToString(), claimType),
                new Claim(JwtClaimTypes.IssuedAt, currentTime.ToEpochTime().ToString(), claimType),
                new Claim(JwtClaimTypes.Expiration, currentTime.AddHours(2).AddMinutes(-1).ToEpochTime().ToString(), claimType),
                new Claim("active", "true", ClaimValueTypes.Boolean),
            };

            jwt.Claims.Should().BeEquivalentTo(expectedClaims);
        }

        [Fact()]
        public async Task introspection_token_with_issuer()
        {
            // Arrange
            var currentTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _stubClock.UtcNowFunc = () => currentTime;

            var token = new Token()
            {
                Type = "introspection_token",
                Issuer = _options.IssuerUri,
                Lifetime = 60 * 60 * 2, // 2h
            };

            // Act
            var result = await _target.CreateTokenAsync(token);
            result.Should().NotBeNull();
            var jwt = new JwtSecurityToken(result);

            // Assert
            jwt.Header["alg"].Should().Be("RS256");
            jwt.Header["typ"].Should().Be("token-introspection+jwt");

            jwt.SignatureAlgorithm.Should().Be("RS256");
            jwt.IssuedAt.Should().Be(currentTime);
            jwt.Issuer.Should().Be("https://test.identityserver.io");
            jwt.Claims.Should().HaveCount(3);

            var introspection = ToDictionary(jwt.Payload["token_introspection"]);
            introspection.Should().NotBeNull();
            introspection.Values.Should().HaveCount(4);

            ((string)introspection["iss"]).Should().Be("https://test.identityserver.io");
            ((long)introspection["nbf"]).Should().Be(currentTime.ToEpochTime());
            ((long)introspection["exp"]).Should().Be(currentTime.AddHours(2).ToEpochTime());
            ((long)introspection["iat"]).Should().Be(currentTime.ToEpochTime());
        }

        [Fact()]
        public async Task introspection_token_with_claims_ignore_lifetime()
        {
            // Arrange
            var currentTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _stubClock.UtcNowFunc = () => currentTime;

            var token = new Token()
            {
                Type = "introspection_token",
                Lifetime = 1, // 1s
                Claims = new List<Claim>()
                {
                    new Claim(JwtClaimTypes.NotBefore, currentTime.AddMinutes(-1).ToEpochTime().ToString(), ClaimValueTypes.Integer),
                    new Claim(JwtClaimTypes.Expiration, currentTime.AddHours(2).AddMinutes(-1).ToEpochTime().ToString(), ClaimValueTypes.Integer),
                    new Claim("active", "true", ClaimValueTypes.Boolean),
                },
            };

            // Act
            var result = await _target.CreateTokenAsync(token);
            result.Should().NotBeNull();
            var jwt = new JwtSecurityToken(result);

            // Assert
            jwt.Header["alg"].Should().Be("RS256");
            jwt.Header["typ"].Should().Be("token-introspection+jwt");

            jwt.SignatureAlgorithm.Should().Be("RS256");
            jwt.IssuedAt.Should().Be(currentTime);
            jwt.Issuer.Should().BeNull();
            jwt.Claims.Should().HaveCount(2);

            var introspection = ToDictionary(jwt.Payload["token_introspection"]);

            introspection.Should().NotBeNull();
            introspection.Values.Should().HaveCount(4);

            ((long)introspection["nbf"]).Should().Be(currentTime.AddMinutes(-1).ToEpochTime());
            ((long)introspection["exp"]).Should().Be(currentTime.AddHours(2).AddMinutes(-1).ToEpochTime());
            ((long)introspection["iat"]).Should().Be(currentTime.ToEpochTime());
            ((bool)introspection["active"]).Should().BeTrue();
        }

        private static Dictionary<string, object> ToDictionary(object value)
        {
#if DUENDE
            var str = value.ToString();
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(str);
            var a = new Dictionary<string, object>();
            foreach (var kvp in dict)
            {
                object val;
                switch(kvp.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        val = kvp.Value.GetString();
                        break;
                    case JsonValueKind.Number:
                        val = kvp.Value.GetInt64();
                        break;
                    case JsonValueKind.True:
                        val = true;
                        break;
                    case JsonValueKind.False:
                        val = false;
                        break;
                    default:
                        continue;
                }

                a.Add(kvp.Key, val);
            }

            return a;
#else
            var jObject = value as Newtonsoft.Json.Linq.JObject;
            return jObject.ToObject<Dictionary<string, object>>();
#endif
        }

    }
}
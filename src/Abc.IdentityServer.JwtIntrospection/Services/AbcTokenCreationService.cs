// ----------------------------------------------------------------------------
// <copyright file="AbcTokenCreationService.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityServer.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Abc.IdentityServer.Services
{
    /// <summary>
    /// Default token creation service.
    /// </summary>
    public class AbcTokenCreationService : DefaultTokenCreationService
    {
        /// <summary>
        /// The options.
        /// </summary>
        protected new readonly AbcIdentityServerOptions Options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbcTokenCreationService"/> class.
        /// </summary>
        /// <param name="clock">The clock.</param>
        /// <param name="keys">The keys.</param>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public AbcTokenCreationService(
#if DUENDE && NET8_0_OR_GREATER
            Ids.IClock clock,
#else
            Microsoft.AspNetCore.Authentication.ISystemClock clock,
#endif
            IKeyMaterialService keys,
            AbcIdentityServerOptions options,
            ILogger<DefaultTokenCreationService> logger)
            : base(clock, keys, options, logger)
        {
            Options = options;
        }

#if DUENDE
        /// <inheritdoc/>
        protected override async Task<Dictionary<string, object>> CreateHeaderElementsAsync(Token token)
        {
            var headerElements = await base.CreateHeaderElementsAsync(token);
            if (token.Type == "introspection_token_v7" || token.Type == "introspection_token")
            {
                headerElements[JwtRegisteredClaimNames.Typ] = Options.IntrospectTokenJwtType.IsPresent()
                    ? Options.IntrospectTokenJwtType
                    : "token-introspection+jwt";
            }

            return headerElements;
        }

        /// <inheritdoc/>
        protected override Task<string> CreatePayloadAsync(Token token)
        {
            // remove "nbf" and "exp" claims
            var notBeforeClaim = token.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.NotBefore);
            var expiresClaim = token.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Expiration);

            if (notBeforeClaim != null)
            {
                token.Claims.Remove(notBeforeClaim);
            }

            if (expiresClaim != null)
            {
                token.Claims.Remove(expiresClaim);
            }

            var payload = token.CreateJwtPayloadDictionary(Options, Clock, Logger);

            // Duende identity server add 'iss' with null value
            if (token.Issuer == null)
            {
                payload.Remove(JwtClaimTypes.Issuer);
            }

            if (token.Type == "introspection_token" || token.Type == "introspection_token_v7")
            {
                payload[JwtClaimTypes.IssuedAt] = EpochTime.GetIntDate(Clock.UtcNow.UtcDateTime);
                if (notBeforeClaim != null)
                {
                    payload[JwtClaimTypes.NotBefore] = long.Parse(notBeforeClaim.Value);
                }

                if (expiresClaim != null)
                {
                    payload[JwtClaimTypes.Expiration] = long.Parse(expiresClaim.Value);
                }
            }

            if (token.Type == "introspection_token")
            {
                var payload2 = new Dictionary<string, object>
                {
                    { JwtClaimTypes.IssuedAt, EpochTime.GetIntDate(Clock.UtcNow.UtcDateTime) },
                    { "token_introspection", payload },
                };

                if (token.Issuer != null)
                {
                    payload2.Add(JwtClaimTypes.Issuer, token.Issuer);
                }

                return Task.FromResult(JsonSerializer.Serialize(payload2));
            }

            return Task.FromResult(JsonSerializer.Serialize(payload));
        }
#else
        /// <inheritdoc/>
        protected override async Task<JwtHeader> CreateHeaderAsync(Token token)
        {
            var header = await base.CreateHeaderAsync(token);
            if (token.Type == "introspection_token_v7" || token.Type == "introspection_token")
            {
                header[JwtRegisteredClaimNames.Typ] = Options.IntrospectTokenJwtType.IsPresent()
                    ? Options.IntrospectTokenJwtType
                    : "token-introspection+jwt";
            }

            return header;
        }

        /// <inheritdoc/>
        protected override async Task<JwtPayload> CreatePayloadAsync(Token token)
        {
            // remove 'nbf', 'exp' and 'iss' claims
            // values ​​are obtained from the `token` argument
            var notBeforeClaim = token.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.NotBefore);
            var expiresClaim = token.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Expiration);
            var issuerClaim = token.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Issuer);

            if (notBeforeClaim != null)
            {
                token.Claims.Remove(notBeforeClaim);
            }

            if (expiresClaim != null)
            {
                token.Claims.Remove(expiresClaim);
            }

            if (issuerClaim != null)
            {
                token.Claims.Remove(issuerClaim);
            }

            var payload = token.CreateJwtPayload(Clock, Options, Logger);

            // IdentityServer4 does not set 'iat' claim
            payload[JwtClaimTypes.IssuedAt] = EpochTime.GetIntDate(Clock.UtcNow.UtcDateTime);

            if (token.Type == "introspection_token" || token.Type == "introspection_token_v7")
            {
                // Replace 'nbf' and 'exp' calims
                if (notBeforeClaim != null)
                {
                    payload[JwtClaimTypes.NotBefore] = long.Parse(notBeforeClaim.Value);
                }

                if (expiresClaim != null)
                {
                    payload[JwtClaimTypes.Expiration] = long.Parse(expiresClaim.Value);
                }
            }

            if (token.Type == "introspection_token")
            {
                var payload2 = new JwtPayload(
                    token.Issuer,
                    null,
                    null,
                    null,
                    null,
                    Clock.UtcNow.UtcDateTime);

                payload2.Add("token_introspection", payload);
                return payload2;
            }

            return payload;
        }
#endif
    }
}
// ----------------------------------------------------------------------------
// <copyright file="JwtIntrospectionResponseGenerator.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abc.IdentityServer.ResponseHandling
{
    /// <summary>
    /// The introspection response generator.
    /// </summary>
    /// <seealso cref="Ids.ResponseHandling.IIntrospectionResponseGenerator" />
    public class JwtIntrospectionResponseGenerator : IntrospectionResponseGenerator
    {
        private const int DefaultIntrospectionTokenLifetime = 60 * 60 * 2; // 2h

        private readonly ITokenCreationService _tokenCreation;
#if DUENDE
        private readonly IIssuerNameService _issuerNameService;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtIntrospectionResponseGenerator" /> class.
        /// </summary>
        /// <param name="issuerNameService">The issuer name service.</param>
        /// <param name="events">The events.</param>
        /// <param name="tokenCreation">The token creation service.</param>
        /// <param name="logger">The logger.</param>
        public JwtIntrospectionResponseGenerator(
            IIssuerNameService issuerNameService,
            IEventService events,
            ITokenCreationService tokenCreation,
            ILogger<IntrospectionResponseGenerator> logger)
            : base(events, logger)
        {
            _issuerNameService = issuerNameService;
            _tokenCreation = tokenCreation;
        }
#else
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtIntrospectionResponseGenerator" /> class.
        /// </summary>
        /// <param name="contextAccessor">The context anccessor.</param>
        /// <param name="events">The events.</param>
        /// <param name="tokenCreation">The token creation service.</param>
        /// <param name="logger">The logger.</param>
        public JwtIntrospectionResponseGenerator(
            IHttpContextAccessor contextAccessor,
            IEventService events,
            ITokenCreationService tokenCreation,
            ILogger<IntrospectionResponseGenerator> logger)
            : base(events, logger)
        {
            _contextAccessor = contextAccessor;
            _tokenCreation = tokenCreation;
        }

#endif

        /// <inheritdoc/>
        public override async Task<Dictionary<string, object>> ProcessAsync(IntrospectionRequestValidationResult validationResult)
        {
            var tokenType = validationResult.Parameters["ContentType"] switch
            {
                "application/jwt" => "introspection_token_v7",
                "application/token-introspection+jwt" => "introspection_token",
                _ => null,
            };

            if (tokenType != null)
            {
                ICollection<Claim> claims;

                if (!validationResult.IsActive)
                {
                    Logger.LogDebug("Creating introspection response for inactive token.");
                    await Events.RaiseAsync(new TokenIntrospectionSuccessEvent(validationResult));
                    claims = new Claim[] { new Claim("active", "false", ClaimValueTypes.Boolean) };
                }
                else
                {
                    claims = new HashSet<Claim>(validationResult.Claims, new ClaimComparer());
                    claims.Add(new Claim("active", "true", ClaimValueTypes.Boolean));
                }

#if DUENDE
                var issuer = await _issuerNameService.GetCurrentAsync();
#else
                var issuer = _contextAccessor.HttpContext.GetIdentityServerIssuerUri();
#endif

                var token = new Token
                {
                    Issuer = issuer,
                    Type = tokenType,
                    AccessTokenType = AccessTokenType.Jwt,
                    Claims = claims,
                };

                // Identityserver4 IDX12401 error thrown if Lifetime equals 0
                // Lifetime value does not matter if `nbf` and `exp` claims presents, see AbcTokenCreationService
                // Set default value in case when claims absent
#if DUENDE && NET8_0_OR_GREATER
                token.Lifetime = validationResult.Client?.AccessTokenLifetime ?? DefaultIntrospectionTokenLifetime;
#else
                token.Lifetime = DefaultIntrospectionTokenLifetime;
#endif

                var introspectionToken = await _tokenCreation.CreateTokenAsync(token);

                var response = new Dictionary<string, object> 
                {
                    {
                        tokenType,
                        introspectionToken
                    },
                };

                return response;
            }

            return await base.ProcessAsync(validationResult);
        }
    }
}
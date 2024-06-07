// ----------------------------------------------------------------------------
// <copyright file="JwtUserInfoResponseGenerator.cs" company="ABC Software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See License.txt in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abc.IdentityServer.ResponseHandling
{
    public class JwtUserInfoResponseGenerator : UserInfoResponseGenerator {
        private readonly ITokenCreationService tokenCreation;
        private readonly ISystemClock clock;

        public JwtUserInfoResponseGenerator(
            IProfileService profileService,
            IResourceStore resourceStore,
            ITokenCreationService tokenCreation,
            ISystemClock clock,
            ILogger<UserInfoResponseGenerator> logger)
            : base(profileService, resourceStore, logger) {
            this.tokenCreation = tokenCreation;
            this.clock = clock;
        }

        public override async Task<Dictionary<string, object>> ProcessAsync(UserInfoRequestValidationResult validationResult) {
            if (validationResult.Parameters["ContentType"] == "application/jwt") {
                ICollection<Claim> claims;

                if (!validationResult.IsActive) {
                    Logger.LogDebug("Creating introspection response for inactive token.");
                    await Events.RaiseAsync(new TokenIntrospectionSuccessEvent(validationResult));
                }
                else {
                    claims = new HashSet<Claim>(validationResult.Subject.Claims, new ClaimComparer());
                }

                var token = new Token {
                    Type = "introspection_token",
                    Lifetime = 300, // Not used
                    AccessTokenType = AccessTokenType.Jwt,
                    Claims = claims,
                };

                var userinfoToken = await this.tokenCreation.CreateTokenAsync(token);

                var response = new Dictionary<string, object> {
                    {
                        "userinfoToken",
                        userinfoToken
                    },
                };

                return response;
            }

            return await base.ProcessAsync(validationResult);
        }
    }
}


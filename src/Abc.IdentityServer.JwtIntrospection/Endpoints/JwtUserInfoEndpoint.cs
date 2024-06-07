// ----------------------------------------------------------------------------
// <copyright file="JwtUserInfoEndpoint.cs" company="ABC Software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See License.txt in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityServer4.Endpoints.Results;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Events;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Threading.Tasks;

namespace Abc.IdentityServer4.Endpoints
{
    /// <summary>
    /// The JWT user info endpoint.
    /// </summary>
    /// <seealso cref="IEndpointHandler" />
    public class JwtUserInfoEndpoint : IEndpointHandler {
        private readonly IApiSecretValidator apiSecretValidator;
        private readonly IIntrospectionRequestValidator requestValidator;
        private readonly IIntrospectionResponseGenerator responseGenerator;
        private readonly IEventService events;
        private readonly ILogger<JwtIntrospectionEndpoint> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtUserInfoEndpoint" /> class.
        /// </summary>
        /// <param name="tokenUsageValidator">The token usage validator.</param>
        /// <param name="requestValidator">The request validator.</param>
        /// <param name="responseGenerator">The response generator.</param>
        /// <param name="logger">The logger.</param>
        public JwtUserInfoEndpoint(
            BearerTokenUsageValidator tokenUsageValidator,
            IUserInfoRequestValidator requestValidator,
            IUserInfoResponseGenerator responseGenerator,
            ILogger<JwtUserInfoEndpoint> logger) {
            this.apiSecretValidator = apiSecretValidator;
            this.requestValidator = requestValidator;
            this.responseGenerator = responseGenerator;
            this.events = events;
            this.logger = logger;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context) {
            this.logger.LogTrace("Processing JWT userinfo request.");

            // validate HTTP
            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsPost(context.Request.Method)) {
                this.logger.LogWarning("Invalid HTTP method for userinfo endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            return await ProcessUserInfoRequestAsync(context);
        }

        private async Task<IEndpointResult> ProcessUserInfoRequestAsync(HttpContext context) {
            this.logger.LogDebug("Starting userinfo request.");

            // generate response
            this.logger.LogTrace("Calling into userinfo response generator: {type}", _responseGenerator.GetType().FullName);
            var response = await _responseGenerator.ProcessAsync(validationResult);

            // render result
            this.logger.LogDebug("End userinfo request");
            if (response.ContainsKey("userinfoToken")) {
                return new JwtUserInfoResult(response["userinfoToken"] as string);
            }

            return new UserInfoResult(response);
        }

        private IEndpointResult Error(string error, string description = null) {
            return new ProtectedResourceErrorResult(error, description);
        }
    }
}


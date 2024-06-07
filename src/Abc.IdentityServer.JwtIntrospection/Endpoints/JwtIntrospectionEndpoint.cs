// ----------------------------------------------------------------------------
// <copyright file="JwtIntrospectionEndpoint.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityServer.Endpoints.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Abc.IdentityServer.Endpoints
{
    /// <summary>
    /// Introspection endpoint.
    /// </summary>
    /// <seealso cref="Ids.Hosting.IEndpointHandler" />
    public class JwtIntrospectionEndpoint : IEndpointHandler
    {
        private readonly IApiSecretValidator _apiSecretValidator;
        private readonly IClientSecretValidator _clientValidator;
        private readonly IIntrospectionRequestValidator _requestValidator;
        private readonly IIntrospectionResponseGenerator _responseGenerator;
        private readonly IEventService _events;
        private readonly ILogger<JwtIntrospectionEndpoint> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtIntrospectionEndpoint" /> class.
        /// </summary>
        /// <param name="apiSecretValidator">The API secret validator.</param>
        /// <param name="clientValidator">The client secret validator.</param>
        /// <param name="requestValidator">The request validator.</param>
        /// <param name="responseGenerator">The generator.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        public JwtIntrospectionEndpoint(
            IApiSecretValidator apiSecretValidator,
            IClientSecretValidator clientValidator,
            IIntrospectionRequestValidator requestValidator,
            IIntrospectionResponseGenerator responseGenerator,
            IEventService events,
            ILogger<JwtIntrospectionEndpoint> logger)
        {
            _apiSecretValidator = apiSecretValidator;
            _clientValidator = clientValidator;
            _requestValidator = requestValidator;
            _responseGenerator = responseGenerator;
            _events = events;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogTrace("Processing JWT introspection request.");

            // validate HTTP
            if (!HttpMethods.IsPost(context.Request.Method))
            {
                _logger.LogWarning("Introspection endpoint only supports POST requests");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            if (!context.Request.HasFormContentType)
            {
                _logger.LogWarning("Invalid media type for introspection endpoint");
                return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType);
            }

            try
            {
                return await ProcessIntrospectionRequestAsync(context);
            }
            catch (InvalidDataException ex)
            {
                _logger.LogWarning(ex, "Invalid HTTP request for introspection endpoint");
                return new StatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        private async Task<IEndpointResult> ProcessIntrospectionRequestAsync(HttpContext context)
        {
            _logger.LogDebug("Starting introspection request.");

            // caller validation
            ApiResource api = null;
            string callerName;

            var apiResult = await _apiSecretValidator.ValidateAsync(context);
#if DUENDE && NET8_0_OR_GREATER
            Client client = null;

            if (apiResult.IsError)
            {
                var clientResult = await _clientValidator.ValidateAsync(context);
                if (clientResult.IsError)
                {
                    _logger.LogError("Unauthorized call introspection endpoint. aborting.");
                    return new StatusCodeResult(HttpStatusCode.Unauthorized);
                }
                else
                {
                    client = clientResult.Client;
                    callerName = client.ClientId;
                    _logger.LogDebug("Client making introspection request: {clientId}", callerName);
                }
            }
            else
            {
                api = apiResult.Resource;
                callerName = api.Name;
                _logger.LogDebug("ApiResource making introspection request: {apiId}", callerName);
            }
#else
            if (apiResult.Resource == null)
            {
                _logger.LogError("API unauthorized to call introspection endpoint. aborting.");
                return new StatusCodeResult(HttpStatusCode.Unauthorized);
            }

            api = apiResult.Resource;
            callerName = api.Name;
#endif

            var body = await context.Request.ReadFormAsync();
            if (body == null)
            {
                _logger.LogError("Malformed request body. aborting.");
                await _events.RaiseAsync(new TokenIntrospectionFailureEvent(callerName, "Malformed request body"));

                return new StatusCodeResult(HttpStatusCode.BadRequest);
            }

            var parameters = body.AsNameValueCollection();
            var acceptHeaderValue = context.Request.Headers[HeaderNames.Accept].ToString();  // TODO: improve header parsing
            if (acceptHeaderValue == "application/jwt" || acceptHeaderValue == "application/token-introspection+jwt")
            {
                parameters.Add("ContentType", acceptHeaderValue);
            }

            // request validation
            _logger.LogTrace("Calling into introspection request validator: {type}", _requestValidator.GetType().FullName);
#if DUENDE && NET8_0_OR_GREATER
            var validationRequest = new IntrospectionRequestValidationContext
            {
                Parameters = parameters,
                Api = apiResult.Resource,
                Client = client,
            };

            var validationResult = await _requestValidator.ValidateAsync(validationRequest);
#else
            var validationResult = await _requestValidator.ValidateAsync(parameters, apiResult.Resource);
#endif
            if (validationResult.IsError)
            {
                LogFailure(validationResult.Error, callerName);
                await _events.RaiseAsync(new TokenIntrospectionFailureEvent(callerName, validationResult.Error));

                return new Results.BadRequestResult(validationResult.Error);
            }

            // response generation
            _logger.LogTrace("Calling into introspection response generator: {type}", _responseGenerator.GetType().FullName);
            var response = await _responseGenerator.ProcessAsync(validationResult);

            // render result
            LogSuccess(validationResult.IsActive, validationResult.Api.Name);
            if (response.ContainsKey("introspection_token_v7"))
            {
                return new JwtIntrospectionResult07(response["introspection_token_v7"] as string);
            }

            if (response.ContainsKey("introspection_token"))
            {
                return new JwtIntrospectionResult(response["introspection_token"] as string);
            }

            return new IntrospectionResult(response);
        }

        private void LogSuccess(bool tokenActive, string apiName)
        {
            _logger.LogInformation("Success token introspection. Token active: {tokenActive}, for API name: {apiName}", tokenActive, apiName);
        }

        private void LogFailure(string error, string apiName)
        {
            _logger.LogError("Failed token introspection: {error}, for API name: {apiName}", error, apiName);
        }
    }
}
// ----------------------------------------------------------------------------
// <copyright file="PostConfigureInternalCookieOptions.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Abc.IdentityServer.Configuration
{
    internal class PostConfigureInternalCookieOptions : IPostConfigureOptions<CookieAuthenticationOptions>
    {
        private readonly IdentityServerOptions _options;

        private readonly IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> _authOptions;

        private readonly ILogger _logger;

        public PostConfigureInternalCookieOptions(AbcIdentityServerOptions options, IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> authOptions, ILoggerFactory loggerFactory)
        {
            _options = options;
            _authOptions = authOptions;
#if DUENDE
            _logger = loggerFactory.CreateLogger("Duende.IdentityServer.Startup");
#else
            _logger = loggerFactory.CreateLogger("IdentityServer4.Startup");
#endif
        }

        public void PostConfigure(string name, CookieAuthenticationOptions options)
        {
            string scheme = _options.Authentication.CookieAuthenticationScheme ?? _authOptions.Value.DefaultAuthenticateScheme ?? _authOptions.Value.DefaultScheme;
            if (name == scheme)
            {
                _options.UserInteraction.LoginUrl = _options.UserInteraction.LoginUrl ?? (string)options.LoginPath;
                _options.UserInteraction.LoginReturnUrlParameter = _options.UserInteraction.LoginReturnUrlParameter ?? options.ReturnUrlParameter;
                _options.UserInteraction.LogoutUrl = _options.UserInteraction.LogoutUrl ?? (string)options.LogoutPath;

                _logger.LogDebug("Login Url: {url}", _options.UserInteraction.LoginUrl);
                _logger.LogDebug("Login Return Url Parameter: {param}", _options.UserInteraction.LoginReturnUrlParameter);
                _logger.LogDebug("Logout Url: {url}", _options.UserInteraction.LogoutUrl);
                _logger.LogDebug("ConsentUrl Url: {url}", _options.UserInteraction.ConsentUrl);
                _logger.LogDebug("Consent Return Url Parameter: {param}", _options.UserInteraction.ConsentReturnUrlParameter);
                _logger.LogDebug("Error Url: {url}", _options.UserInteraction.ErrorUrl);
                _logger.LogDebug("Error Id Parameter: {param}", _options.UserInteraction.ErrorIdParameter);
            }
        }
    }
}
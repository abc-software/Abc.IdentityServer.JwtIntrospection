// ----------------------------------------------------------------------------
// <copyright file="IdentityServerServiceCollectionExtensions.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityServer.Configuration;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// DI extension methods for adding IdentityServer.
    /// </summary>
    public static class IdentityServerServiceCollectionExtensions
    {
        /// <summary>
        /// Adds IdentityServer with Jwt response.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>The IdentityServer builder.</returns>
        public static IIdentityServerBuilder AddIdentityServerWithJwtResponse(this IServiceCollection services)
        {
            var builder = services.AddIdentityServerBuilder();

            builder
                .AddJwtIntrospection()
                .AddJwtUserInfo()
                .AddJwtRequiredPlatformServices()
                .AddCookieAuthentication()
                .AddCoreServices()
                .AddDefaultEndpoints()
                .AddPluggableServices()
#if DUENDE
		        .AddKeyManagement()
		        .AddDynamicProvidersCore()
		        .AddOidcDynamicProvider()
#endif
                .AddValidators()
                .AddResponseGenerators()
                .AddDefaultSecretParsers()
                .AddDefaultSecretValidators();

            // provide default in-memory implementation, not suitable for most production scenarios
            builder.AddInMemoryPersistedGrants();

            return builder;
        }

        /// <summary>
        /// Adds IdentityServer with Jwt introspection.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="setupAction">The setup action.</param>
        /// <returns>The IdentityServer builder.</returns>
        public static IIdentityServerBuilder AddIdentityServerWithJwtResponse(this IServiceCollection services, Action<AbcIdentityServerOptions> setupAction)
        {
            services.Configure(setupAction);
            return services.AddIdentityServerWithJwtResponse();
        }

        /// <summary>
        /// Adds the IdentityServer with Jwt introspection.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The IdentityServer builder.</returns>
        public static IIdentityServerBuilder AddIdentityServerWithJwtIntrospection(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AbcIdentityServerOptions>(configuration);
            return services.AddIdentityServerWithJwtResponse();
        }
    }
}
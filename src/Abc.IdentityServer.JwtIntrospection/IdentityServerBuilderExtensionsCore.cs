// ----------------------------------------------------------------------------
// <copyright file="IdentityServerBuilderExtensionsCore.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityServer.Configuration;
using Abc.IdentityServer.Endpoints;
using Abc.IdentityServer.ResponseHandling;
using Abc.IdentityServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder extension methods for registering core services.
    /// </summary>
    public static class IdentityServerBuilderExtensionsCore
    {
        /// <summary>
        /// Adds the required platform services.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The IdentityServer builder.</returns>
        public static IIdentityServerBuilder AddJwtRequiredPlatformServices(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddOptions();
            builder.Services.AddSingleton<AbcIdentityServerOptions>(resolver => resolver.GetRequiredService<IOptions<AbcIdentityServerOptions>>().Value);
            builder.Services.AddSingleton<IdentityServerOptions>(resolver => resolver.GetRequiredService<AbcIdentityServerOptions>());
#if DUENDE
            builder.Services.AddTransient(resolver => resolver.GetRequiredService<IOptions<IdentityServerOptions>>().Value.PersistentGrants);
#endif
            builder.Services.AddHttpClient();

            return builder;
        }

        /// <summary>
        /// Adds the JWT response for introspect.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The IdentityServer builder.</returns>
        public static IIdentityServerBuilder AddJwtIntrospection(this IIdentityServerBuilder builder)
        {
            builder.AddEndpoint<JwtIntrospectionEndpoint>("Introspection", "/connect/introspect");
            builder.Services.TryAddTransient<IIntrospectionResponseGenerator, JwtIntrospectionResponseGenerator>();
            builder.Services.TryAddTransient<ITokenCreationService, AbcTokenCreationService>();

            return builder;
        }

        /// <summary>
        /// Adds the JWT response for user info.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The IdentityServer builder.</returns>
        public static IIdentityServerBuilder AddJwtUserInfo(this IIdentityServerBuilder builder)
        {
        /* UNDONE:
            builder.AddEndpoint<JwtUserInfoEndpoint>("UserInfo", "/connect/userinfo");
            builder.Services.TryAddTransient<IUserInfoResponseGenerator, JwtUserInfoResponseGenerator>();
            builder.Services.TryAddTransient<ITokenCreationService, AbcTokenCreationService>();
        */

            return builder;
        }
    }
}

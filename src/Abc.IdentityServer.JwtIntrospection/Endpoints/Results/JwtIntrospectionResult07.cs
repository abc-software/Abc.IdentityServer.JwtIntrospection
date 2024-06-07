// ----------------------------------------------------------------------------
// <copyright file="JwtIntrospectionResult07.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Abc.IdentityServer.Endpoints.Results
{
    /// <summary>
    /// Result for JWT introspection till draft-V7.
    /// </summary>
    /// <seealso cref="Ids.Hosting.IEndpointResult" />
    internal class JwtIntrospectionResult07 : IEndpointResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JwtIntrospectionResult07" /> class.
        /// </summary>
        /// <param name="introspectToken">The introspect token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="introspectToken"/> is <c>null</c>.</exception>
        public JwtIntrospectionResult07(string introspectToken)
        {
            IntrospectToken = introspectToken ?? throw new ArgumentNullException(nameof(introspectToken));
        }

        /// <summary>
        /// Gets the introspect token.
        /// </summary>
        public string IntrospectToken { get; }

        /// <summary>
        /// Executes the result.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.SetNoCache();

            context.Response.ContentType = "application/jwt";
            await context.Response.WriteAsync(IntrospectToken);
            await context.Response.Body.FlushAsync();
        }
    }
}
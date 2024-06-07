// ----------------------------------------------------------------------------
// <copyright file="JwtUserInfoResult.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Abc.IdentityServer.Endpoints.Results
{
    /// <summary>
    /// Result for JWT user info.
    /// </summary>
    /// <seealso cref="T:IdentityServer4.Hosting.IEndpointResult" />
    public class JwtUserInfoResult : IEndpointResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JwtUserInfoResult" /> class.
        /// </summary>
        /// <param name="userinfoToken">The user info token.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="userinfoToken"/> is <c>null</c>.</exception>
        public JwtUserInfoResult(string userinfoToken)
        {
            this.UserInfoToken = userinfoToken ?? throw new ArgumentNullException(nameof(userinfoToken));
        }

        /// <summary>
        /// Gets the user info token.
        /// </summary>
        public string UserInfoToken { get; }

        /// <summary>
        /// Executes the result.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.SetNoCache();

            context.Response.ContentType = "application/jwt";
            await context.Response.WriteAsync(this.UserInfoToken, default(CancellationToken));
            await context.Response.Body.FlushAsync();
        }
    }
}
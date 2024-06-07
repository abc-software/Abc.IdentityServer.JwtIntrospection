// ----------------------------------------------------------------------------
// <copyright file="AbcIdentityServerOptions.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.IdentityServer.Configuration
{
    /// <summary>
    /// Extended identity server options.
    /// </summary>
    public class AbcIdentityServerOptions : IdentityServerOptions
    {
        /// <summary>
        /// Gets or sets the value for the JWT typ header for introspect tokens.
        /// </summary>
        /// <value>
        /// The JWT typ value.
        /// </value>
        public string IntrospectTokenJwtType { get; set; } = "token-introspection+jwt";

        /// <summary>
        /// Gets or sets a value indicating whether the JWT response for introspection endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the JWT response for introspection endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableJwtIntrospectionResponse { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the JWT response for user info endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the JWT response for user info endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableJwtUserInfoResponse { get; set; } = true;
    }
}
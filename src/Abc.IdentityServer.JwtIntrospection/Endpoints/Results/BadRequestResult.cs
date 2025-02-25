// ----------------------------------------------------------------------------
// <copyright file="BadRequestResult.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Abc.IdentityServer.Endpoints.Results
{
    internal class BadRequestResult : IEndpointResult
    {
        public BadRequestResult(string error = null, string errorDescription = null)
        {
            Error = error;
            ErrorDescription = errorDescription;
        }

        public string Error { get; set; }

        public string ErrorDescription { get; set; }

        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.StatusCode = 400;
            context.Response.SetNoCache();
            if (!string.IsNullOrEmpty(Error))
            {
                var dto = new ResultDto
                {
                    error = Error,
                    error_description = ErrorDescription,
                };

                await context.Response.WriteJsonAsync(dto);
            }
        }

        internal class ResultDto
        {
            public string error { get; set; }

            public string error_description { get; set; }
        }
    }
}
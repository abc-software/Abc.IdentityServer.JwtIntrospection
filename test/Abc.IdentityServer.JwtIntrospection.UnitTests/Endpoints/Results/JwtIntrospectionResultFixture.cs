﻿using Abc.IdentityServer.Endpoints.Results;
using Microsoft.AspNetCore.Http;

namespace Abc.IdentityServer.JwtIntrospection.Endpoints.Results.UnitTests
{
    public class JwtIntrospectionResultFixture
    {
        private DefaultHttpContext _context;

        public JwtIntrospectionResultFixture()
        {
            _context = new DefaultHttpContext();
            _context.Response.Body = new MemoryStream();
        }

        [Fact]
        public async Task return_introspection_token()
        {
            var target = new JwtIntrospectionResult("some.introspection.token");
            await target.ExecuteAsync(_context);

            _context.Response.StatusCode.Should().Be(200);
            _context.Response.ContentType.Should().Be("application/token-introspection+jwt");
            
            _context.Response.Body.Seek(0, SeekOrigin.Begin);
            using (var rdr = new StreamReader(_context.Response.Body))
            {
                var json = rdr.ReadToEnd();
                json.Should().Be("some.introspection.token");
            }
        }
    }
}

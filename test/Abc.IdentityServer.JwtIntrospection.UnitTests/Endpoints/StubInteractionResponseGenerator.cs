namespace Abc.IdentityServer.Endpoints
{
    internal class StubInteractionResponseGenerator : IIntrospectionResponseGenerator
    {
        internal Dictionary<string, object> Response { get; set; } = new Dictionary<string, object>();

        public Task<Dictionary<string, object>> ProcessAsync(IntrospectionRequestValidationResult validationResult)
        {
            var tokenType = validationResult.Parameters["ContentType"] switch
            {
                "application/jwt" => "introspection_token_v7",
                "application/token-introspection+jwt" => "introspection_token",
                _ => null,
            };

            if (tokenType != null)
            {
                Response.Add(tokenType, "some token");
            }

            return Task.FromResult(Response);
        }
    }
}

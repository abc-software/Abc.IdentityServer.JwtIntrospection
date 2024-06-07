using System.Collections.Specialized;

namespace Abc.IdentityServer.Endpoints
{
    internal class StubIntrospectionRequestValidator : IIntrospectionRequestValidator
    {
        public IntrospectionRequestValidationResult Result { get; set; } = new IntrospectionRequestValidationResult();

#if DUENDE && NET8_0_OR_GREATER
        public Task<IntrospectionRequestValidationResult> ValidateAsync(IntrospectionRequestValidationContext context)
        {
            Result.Parameters = context.Parameters;
            Result.Api = context.Api;

            return Task.FromResult(Result);
        }
#else
        public Task<IntrospectionRequestValidationResult> ValidateAsync(NameValueCollection parameters, ApiResource api)
        {
            Result.Parameters = parameters;
            Result.Api = api;

            return Task.FromResult(Result);
        }
#endif
    }
}

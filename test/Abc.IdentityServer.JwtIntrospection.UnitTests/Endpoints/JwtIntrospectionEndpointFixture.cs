using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Abc.IdentityServer.Endpoints.UnitTests
{
    public class JwtIntrospectionEndpointFixture
    {
        private const string Category = "JwtIntrospection Endpoint";

        private DefaultHttpContext _context;
        private JwtIntrospectionEndpoint _subject;

        private TestEventService _fakeEventService = new TestEventService();
        private ILogger<JwtIntrospectionEndpoint> _fakeLogger = TestLogger.Create<JwtIntrospectionEndpoint>();
        private StubApiSecretValidator _apiSecretValidator = new StubApiSecretValidator();
        private StubClientSecretValidator _clientValidator = new StubClientSecretValidator();
        private StubIntrospectionRequestValidator _requestValidator = new StubIntrospectionRequestValidator();
        private StubInteractionResponseGenerator _responseGenerator = new StubInteractionResponseGenerator();

        public JwtIntrospectionEndpointFixture()
        {
            _context = new DefaultHttpContext();

            _subject = new JwtIntrospectionEndpoint(
                _apiSecretValidator,
                _clientValidator,
                _requestValidator,
                _responseGenerator,
                _fakeEventService,
                _fakeLogger);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task introspect_get_should_return_405()
        {
            // Arrange
            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/introspect");

            // Act
            var result = await _subject.ProcessAsync(_context);

            // Assert
            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(405);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task introspect_no_forms_should_return_415()
        {
            // Arrange
            _context.Request.Method = "POST";
            _context.Request.Path = new PathString("/introspect");

            // Act
            var result = await _subject.ProcessAsync(_context);

            // Assert
            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(415);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task introspect_no_resource_should_return_401()
        {
            // Arrange
            _context.Request.Method = "POST";
            _context.Request.Path = new PathString("/introspect");
            _context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

            _apiSecretValidator.Result = new ApiSecretValidationResult()
            {
                IsError = true,
            };

            _clientValidator.Result = new ClientSecretValidationResult()
            {
                IsError = true
            };

            // Act
            var result = await _subject.ProcessAsync(_context);

            // Assert
            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(401);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task introspect_invalid_should_return_400()
        {
            // Arrange
            _context.Request.Method = "POST";
            _context.Request.Path = new PathString("/introspect");
            _context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

            _apiSecretValidator.Result = new ApiSecretValidationResult()
            {
                IsError = false,
                Resource = new ApiResource("api"),
            };

            _requestValidator.Result.IsError = true;
            _requestValidator.Result.Error = "invalid request";

            // Act
            var result = await _subject.ProcessAsync(_context);

            // Assert
            var statusCode = result as Results.BadRequestResult;
            statusCode.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task introspect_should_return_ok()
        {
            // Arrange
            _context.Request.Method = "POST";
            _context.Request.Path = new PathString("/introspect");
            _context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

            _apiSecretValidator.Result = new ApiSecretValidationResult() { 
                IsError = false,
                Resource = new ApiResource("api"),
            };

            _requestValidator.Result.IsError = false;

            // Act
            var result = await _subject.ProcessAsync(_context);

            // Assert
            var introspection = result as IntrospectionResult;
            introspection.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task jwtintrospect_should_return_ok()
        {
            // Arrange
            _context.Request.Method = "POST";
            _context.Request.Path = new PathString("/introspect");
            _context.Request.Form = new FormCollection(new Dictionary<string, StringValues>());
            _context.Request.Headers["Accept"] = new StringValues("application/token-introspection+jwt");

            _apiSecretValidator.Result = new ApiSecretValidationResult()
            {
                IsError = false,
                Resource = new ApiResource("api"),
            };

            _requestValidator.Result.IsError = false;

            // Act
            var result = await _subject.ProcessAsync(_context);

            // Assert
            var introspection = result as Results.JwtIntrospectionResult;
            introspection.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task jwtintrospect_v7_should_return_ok()
        {
            // Arrange
            _context.Request.Method = "POST";
            _context.Request.Path = new PathString("/introspect");
            _context.Request.Form = new FormCollection(new Dictionary<string, StringValues>());
            _context.Request.Headers["Accept"] = new StringValues("application/jwt");

            _apiSecretValidator.Result = new ApiSecretValidationResult()
            {
                IsError = false,
                Resource = new ApiResource("api"),
            };

            _requestValidator.Result.IsError = false;

            // Act
            var result = await _subject.ProcessAsync(_context);

            // Assert
            var introspection = result as Results.JwtIntrospectionResult07;
            introspection.Should().NotBeNull();
        }
    }
}
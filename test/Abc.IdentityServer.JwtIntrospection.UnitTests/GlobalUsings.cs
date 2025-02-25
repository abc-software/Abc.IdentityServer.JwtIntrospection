﻿global using Xunit;
global using FluentAssertions;
#if DUENDE
global using Duende.IdentityServer.Hosting;
global using Duende.IdentityServer.Models;
global using Duende.IdentityServer.Stores;
global using Duende.IdentityServer.Configuration;
global using Duende.IdentityServer.Extensions;
global using Duende.IdentityServer.Services;
global using Duende.IdentityServer.Events;
global using Duende.IdentityServer.ResponseHandling;
global using Duende.IdentityServer.Validation;
global using Ids = Duende.IdentityServer;
global using IntrospectionResult = Duende.IdentityServer.Endpoints.Results.IntrospectionResult;
global using StatusCodeResult = Duende.IdentityServer.Endpoints.Results.StatusCodeResult;
#elif IDS8
global using IdentityServer8.Hosting;
global using IdentityServer8.Models;
global using IdentityServer8.Stores;
global using IdentityServer8.Configuration;
global using IdentityServer8.Extensions;
global using IdentityServer8.Services;
global using IdentityServer8.Events;
global using IdentityServer8.ResponseHandling;
global using IdentityServer8.Validation;
global using Ids = IdentityServer8;
global using IntrospectionResult = IdentityServer8.Endpoints.Results.IntrospectionResult;
global using StatusCodeResult = IdentityServer8.Endpoints.Results.StatusCodeResult;
#else
global using IdentityServer4.Hosting;
global using IdentityServer4.Models;
global using IdentityServer4.Stores;
global using IdentityServer4.Configuration;
global using IdentityServer4.Extensions;
global using IdentityServer4.Services;
global using IdentityServer4.Events;
global using IdentityServer4.ResponseHandling;
global using IdentityServer4.Validation;
global using Ids = IdentityServer4;
global using IntrospectionResult = IdentityServer4.Endpoints.Results.IntrospectionResult;
global using StatusCodeResult = IdentityServer4.Endpoints.Results.StatusCodeResult;
#endif

#if NET8_0_OR_GREATER && DUENDE
global using Duende.IdentityModel;
#else
global using IdentityModel;
#endif
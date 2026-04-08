using Application.Attributes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WebApi.OpenApi;

public static class AuthOpenApiTransformers
{
    public const string BearerSchemeName = "Bearer";
    public const string SensorApiKeySchemeName = "SensorApiKey";

    public static Task AddSecuritySchemesAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes[BearerSchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT bearer token. Use: Authorization: Bearer {token}"
        };

        document.Components.SecuritySchemes[SensorApiKeySchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = "X-Sensor-Api-Key",
            Description = "Sensor API key. Use header: X-Sensor-Api-Key"
        };

        return Task.CompletedTask;
    }

    public static Task ApplyOperationSecurityAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = GetEndpointMetadata(context.Description.ActionDescriptor);
        if (metadata.Count == 0)
        {
            return Task.CompletedTask;
        }

        if (metadata.OfType<IAllowAnonymous>().Any())
        {
            return Task.CompletedTask;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();

        if (metadata.OfType<IAuthorizeData>().Any())
        {
            operation.Security.Add(CreateSecurityRequirement(BearerSchemeName, context.Document));
        }

        if (metadata.OfType<RequireSensorApiKeyAttribute>().Any())
        {
            operation.Security.Add(CreateSecurityRequirement(SensorApiKeySchemeName, context.Document));
        }

        return Task.CompletedTask;
    }

    private static IList<object> GetEndpointMetadata(Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor actionDescriptor)
    {
        if (actionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            return controllerActionDescriptor.EndpointMetadata;
        }

        return actionDescriptor.EndpointMetadata ?? [];
    }

    private static OpenApiSecurityRequirement CreateSecurityRequirement(string schemeName, OpenApiDocument document)
    {
        return new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(schemeName, document, null)] = []
        };
    }
}

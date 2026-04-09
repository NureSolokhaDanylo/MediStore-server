using Application.Attributes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
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

        NormalizeOperationResponses(operation, metadata);

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

    private static void NormalizeOperationResponses(OpenApiOperation operation, IList<object> metadata)
    {
        RemoveContentFromBodylessResponses(operation);

        var produces = metadata.OfType<ProducesAttribute>().ToArray();
        if (produces.Length == 0 || operation.Responses is null)
        {
            return;
        }

        var contentTypes = produces
            .SelectMany(x => x.ContentTypes)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (contentTypes.Length == 0)
        {
            return;
        }

        if (contentTypes.Length == 1 && string.Equals(contentTypes[0], "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            ReplaceResponseContentWithBinaryPdf(operation);
            return;
        }

        if (contentTypes.Length == 1 && string.Equals(contentTypes[0], "application/json", StringComparison.OrdinalIgnoreCase))
        {
            KeepOnlyJsonContent(operation);
        }
    }

    private static void RemoveContentFromBodylessResponses(OpenApiOperation operation)
    {
        if (operation.Responses is null || operation.Responses.Count == 0)
        {
            return;
        }

        foreach (var statusCode in new[] { "204", "304" })
        {
            if (operation.Responses.TryGetValue(statusCode, out var response) && response.Content is not null)
            {
                response.Content.Clear();
            }
        }
    }

    private static void KeepOnlyJsonContent(OpenApiOperation operation)
    {
        foreach (var response in operation.Responses.Values)
        {
            if (response.Content is null || response.Content.Count == 0)
            {
                continue;
            }

            if (response.Content.TryGetValue("application/json", out var jsonContent))
            {
                var content = response.Content;
                content.Clear();
                content["application/json"] = jsonContent;
            }
        }
    }

    private static void ReplaceResponseContentWithBinaryPdf(OpenApiOperation operation)
    {
        foreach (var response in operation.Responses.Values)
        {
            var content = response.Content;
            content.Clear();
            content["application/pdf"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Format = "binary"
                }
            };
        }
    }
}

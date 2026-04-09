using Application.Attributes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using WebApi.Controllers;

namespace WebApi.OpenApi;

public static class AuthOpenApiTransformers
{
    public const string BearerSchemeName = "Bearer";
    public const string SensorApiKeySchemeName = "SensorApiKey";
    public const string ApiErrorSchemaName = "ApiError";

    public static Task AddSecuritySchemesAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

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

        document.Components.Schemas[ApiErrorSchemaName] = CreateApiErrorSchema();

        return Task.CompletedTask;
    }

    public static Task ApplyOperationSecurityAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = GetEndpointMetadata(context.Description.ActionDescriptor);
        if (metadata.Count == 0)
        {
            return Task.CompletedTask;
        }

        ApplyDeclaredErrorResponses(operation, metadata, context.Document);
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

    private static void ApplyDeclaredErrorResponses(OpenApiOperation operation, IList<object> metadata, OpenApiDocument? document)
    {
        var declaredErrors = metadata.OfType<ApiErrorsAttribute>().SelectMany(x => x.StatusCodes).Distinct().ToArray();
        if (declaredErrors.Length == 0)
        {
            return;
        }

        var codesByStatus = metadata
            .OfType<ApiErrorCodesAttribute>()
            .GroupBy(x => x.StatusCode)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(x => x.Codes).Distinct(StringComparer.Ordinal).ToArray());

        operation.Responses ??= new OpenApiResponses();

        foreach (var statusCode in declaredErrors)
        {
            var statusKey = statusCode.ToString();
            var errorCodes = codesByStatus.GetValueOrDefault(statusCode, []);

            if (!operation.Responses.TryGetValue(statusKey, out var response))
            {
                response = new OpenApiResponse();
                operation.Responses[statusKey] = response;
            }

            response = EnsureResponseHasContent(operation, statusKey, response);
            response.Description = BuildErrorDescription(statusCode, errorCodes);
            response.Content.Clear();
            response.Content["application/json"] = new OpenApiMediaType
            {
                Schema = GetApiErrorSchema(document)
            };
        }
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
        foreach (var statusCode in operation.Responses.Keys.ToArray())
        {
            if (!IsSuccessStatusCode(statusCode))
            {
                continue;
            }

            var response = EnsureResponseHasContent(operation, statusCode, operation.Responses[statusCode]);
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

    private static OpenApiResponse EnsureResponseHasContent(OpenApiOperation operation, string statusCode, IOpenApiResponse response)
    {
        if (response.Content is not null)
        {
            return (OpenApiResponse)response;
        }

        var mutableResponse = new OpenApiResponse
        {
            Description = response.Description,
            Headers = response.Headers,
            Links = response.Links,
            Extensions = response.Extensions,
            Content = new Dictionary<string, OpenApiMediaType>()
        };

        operation.Responses[statusCode] = mutableResponse;
        return mutableResponse;
    }

    private static IOpenApiSchema GetApiErrorSchema(OpenApiDocument? document)
    {
        if (document is not null)
        {
            return new OpenApiSchemaReference(ApiErrorSchemaName, document, null);
        }

        return CreateApiErrorSchema();
    }

    private static bool IsSuccessStatusCode(string statusCode)
    {
        return statusCode.Length == 3
            && statusCode[0] == '2'
            && char.IsDigit(statusCode[1])
            && char.IsDigit(statusCode[2]);
    }

    private static OpenApiSchema CreateApiErrorSchema()
    {
        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["code"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Description = "Machine-readable internal error code."
                },
                ["message"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                },
                ["status"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Integer,
                    Format = "int32"
                },
                ["traceId"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                },
                ["details"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object
                }
            },
            Required = new HashSet<string>(StringComparer.Ordinal)
            {
                "code",
                "message",
                "status"
            }
        };
    }

    private static string BuildErrorDescription(int statusCode, IReadOnlyCollection<string> codes)
    {
        var description = statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            _ => "Error"
        };

        if (codes.Count == 0)
        {
            return description;
        }

        return $"{description}. Possible internal codes: {string.Join(", ", codes)}";
    }
}

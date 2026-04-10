using Application.Attributes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Reflection;

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
        NormalizeDocumentSchemas(document);

        return Task.CompletedTask;
    }

    public static Task ApplyOperationSecurityAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        ApplyOperationId(operation, context.Description.ActionDescriptor);

        var metadata = GetEndpointMetadata(context.Description.ActionDescriptor);
        if (metadata.Count == 0)
        {
            return Task.CompletedTask;
        }

        ApplyDeclaredErrorResponses(operation, metadata, context.Document);
        NormalizeOperationResponses(operation, metadata);
        NormalizeOperationSchemas(operation);
        ApplyOperationDescription(operation, metadata);

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

    private static void ApplyOperationId(OpenApiOperation operation, Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor actionDescriptor)
    {
        if (!string.IsNullOrWhiteSpace(operation.OperationId))
        {
            return;
        }

        if (actionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            return;
        }

        var controllerName = controllerActionDescriptor.ControllerName;
        if (string.IsNullOrWhiteSpace(controllerName))
        {
            return;
        }

        var actionName = controllerActionDescriptor.MethodInfo.Name;
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return;
        }

        var suffix = RequiresRouteSuffix(controllerActionDescriptor)
            ? BuildRouteSuffix(controllerActionDescriptor.AttributeRouteInfo?.Template)
            : string.Empty;

        operation.OperationId = ToCamelCase($"{controllerName}{actionName}{suffix}");
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

        operation.Responses ??= new OpenApiResponses();

        foreach (var statusCode in declaredErrors)
        {
            var statusKey = statusCode.ToString();

            if (!operation.Responses.TryGetValue(statusKey, out var response))
            {
                response = new OpenApiResponse();
                operation.Responses[statusKey] = response;
            }

            response = EnsureResponseHasContent(operation, statusKey, response);
            response.Description = BuildErrorDescription(statusCode);
            response.Content.Clear();
            response.Content["application/json"] = new OpenApiMediaType
            {
                Schema = GetApiErrorSchema(document)
            };
        }
    }

    private static void ApplyOperationDescription(OpenApiOperation operation, IList<object> metadata)
    {
        var descriptionParts = new List<string>();
        var authorizationDescription = BuildAuthorizationDescription(metadata);
        if (!string.IsNullOrWhiteSpace(authorizationDescription))
        {
            descriptionParts.Add(authorizationDescription);
        }

        var errorCodes = metadata
            .OfType<ApiErrorsAttribute>()
            .SelectMany(static errors => errors.Codes)
            .Where(static code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (errorCodes.Length > 0)
        {
            descriptionParts.Add($"Possible internal codes: {string.Join(", ", errorCodes)}");
        }

        if (descriptionParts.Count == 0)
        {
            return;
        }

        var metadataDescription = string.Join($"{Environment.NewLine}{Environment.NewLine}", descriptionParts);
        operation.Description = string.IsNullOrWhiteSpace(operation.Description)
            ? metadataDescription
            : $"{operation.Description.Trim()}{Environment.NewLine}{Environment.NewLine}{metadataDescription}";
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

    private static void NormalizeDocumentSchemas(OpenApiDocument document)
    {
        if (document.Components?.Schemas is null)
        {
            return;
        }

        foreach (var schema in document.Components.Schemas.Values)
        {
            NormalizeSchema(schema);
        }
    }

    private static void NormalizeOperationSchemas(OpenApiOperation operation)
    {
        if (operation.Parameters is not null)
        {
            foreach (var parameter in operation.Parameters)
            {
                if (parameter.Schema is not null)
                {
                    NormalizeSchema(parameter.Schema);
                }
            }
        }

        if (operation.RequestBody?.Content is not null)
        {
            foreach (var mediaType in operation.RequestBody.Content.Values)
            {
                if (mediaType.Schema is not null)
                {
                    NormalizeSchema(mediaType.Schema);
                }
            }
        }

        if (operation.Responses is null)
        {
            return;
        }

        foreach (var response in operation.Responses.Values)
        {
            if (response.Content is null)
            {
                continue;
            }

            foreach (var mediaType in response.Content.Values)
            {
                if (mediaType.Schema is not null)
                {
                    NormalizeSchema(mediaType.Schema);
                }
            }
        }
    }

    private static void NormalizeSchema(IOpenApiSchema schema)
    {
        if (schema is OpenApiSchema openApiSchema)
        {
            NormalizeSchemaType(openApiSchema);
        }

        if (schema.Properties is not null)
        {
            foreach (var property in schema.Properties.Values)
            {
                NormalizeSchema(property);
            }
        }

        if (schema.Items is not null)
        {
            NormalizeSchema(schema.Items);
        }

        if (schema.AdditionalProperties is not null)
        {
            NormalizeSchema(schema.AdditionalProperties);
        }

        if (schema.AnyOf is not null)
        {
            foreach (var nested in schema.AnyOf)
            {
                NormalizeSchema(nested);
            }
        }

        if (schema.OneOf is not null)
        {
            foreach (var nested in schema.OneOf)
            {
                NormalizeSchema(nested);
            }
        }

        if (schema.AllOf is not null)
        {
            foreach (var nested in schema.AllOf)
            {
                NormalizeSchema(nested);
            }
        }

        if (schema.Not is not null)
        {
            NormalizeSchema(schema.Not);
        }
    }

    private static void NormalizeSchemaType(OpenApiSchema schema)
    {
        var type = schema.Type;
        if (type is null)
        {
            return;
        }

        if (type.Value.HasFlag(JsonSchemaType.Integer) && type.Value.HasFlag(JsonSchemaType.String))
        {
            schema.Type = type.Value.HasFlag(JsonSchemaType.Null)
                ? JsonSchemaType.Integer | JsonSchemaType.Null
                : JsonSchemaType.Integer;
            return;
        }

        if (type.Value.HasFlag(JsonSchemaType.Number) && type.Value.HasFlag(JsonSchemaType.String))
        {
            schema.Type = type.Value.HasFlag(JsonSchemaType.Null)
                ? JsonSchemaType.Number | JsonSchemaType.Null
                : JsonSchemaType.Number;
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

    private static string? BuildAuthorizationDescription(IList<object> metadata)
    {
        if (metadata.OfType<IAllowAnonymous>().Any())
        {
            return null;
        }

        if (metadata.OfType<RequireSensorApiKeyAttribute>().Any())
        {
            return "Authorization: sensor API key";
        }

        var authorizeData = metadata.OfType<IAuthorizeData>().ToArray();
        if (authorizeData.Length == 0)
        {
            return null;
        }

        var roles = authorizeData
            .SelectMany(static auth => SplitRoles(auth.Roles))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return roles.Length == 0
            ? "Authorization: authenticated user"
            : $"Required roles: {string.Join(", ", roles)}";
    }

    private static string BuildErrorDescription(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            _ => "Error"
        };
    }

    private static IEnumerable<string> SplitRoles(string? roles)
    {
        if (string.IsNullOrWhiteSpace(roles))
        {
            return [];
        }

        return roles
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static role => !string.IsNullOrWhiteSpace(role));
    }

    private static bool RequiresRouteSuffix(ControllerActionDescriptor actionDescriptor)
    {
        var declaringType = actionDescriptor.MethodInfo.DeclaringType;
        if (declaringType is null)
        {
            return false;
        }

        return declaringType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Count(m => string.Equals(m.Name, actionDescriptor.MethodInfo.Name, StringComparison.Ordinal)) > 1;
    }

    private static string BuildRouteSuffix(string? template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        var routeParts = template
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        var parameterParts = routeParts
            .Where(static part => part.StartsWith('{') && part.EndsWith('}'))
            .Select(ToRoutePartSuffix)
            .Where(static part => !string.IsNullOrEmpty(part))
            .ToArray();

        if (parameterParts.Length > 0)
        {
            return string.Concat(parameterParts);
        }

        var literalParts = routeParts
            .Select(ToRoutePartSuffix)
            .Where(static part => !string.IsNullOrEmpty(part))
            .ToArray();

        if (literalParts.Length == 0)
        {
            return string.Empty;
        }

        return literalParts[^1];
    }

    private static string ToRoutePartSuffix(string routePart)
    {
        if (routePart.StartsWith('{') && routePart.EndsWith('}'))
        {
            var parameterName = routePart[1..^1];
            var colonIndex = parameterName.IndexOf(':');
            if (colonIndex >= 0)
            {
                parameterName = parameterName[..colonIndex];
            }

            return $"By{ToPascalCase(parameterName)}";
        }

        return ToPascalCase(routePart);
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var parts = value
            .Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static part => !string.IsNullOrWhiteSpace(part))
            .Select(static part => char.ToUpperInvariant(part[0]) + part[1..]);

        return string.Concat(parts);
    }
}

using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Common.API.Documentation;

public sealed class BearerSecurityDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(swaggerDoc);

        if (swaggerDoc.Paths is null)
        {
            return;
        }

        foreach (IOpenApiPathItem pathItem in swaggerDoc.Paths.Values)
        {
            RebindSecurityToHostDocument(pathItem, swaggerDoc);
        }
    }

    private static void RebindSecurityToHostDocument(IOpenApiPathItem pathItem, OpenApiDocument swaggerDoc)
    {
        if (pathItem.Operations is null)
        {
            return;
        }

        foreach (OpenApiOperation operation in pathItem.Operations.Values)
        {
            if (operation.Security?.Count > 0)
            {
                operation.Security = [CreateBearerRequirementBoundTo(swaggerDoc)];
            }
        }
    }

    private static OpenApiSecurityRequirement CreateBearerRequirementBoundTo(OpenApiDocument swaggerDoc) =>
        new()
        {
            [new OpenApiSecuritySchemeReference(SwaggerExtensions.BearerSchemeName, swaggerDoc)] = []
        };
}

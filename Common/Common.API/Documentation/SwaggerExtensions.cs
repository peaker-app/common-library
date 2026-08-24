using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Common.API.Documentation;

public static class SwaggerExtensions
{
    public const string BearerSchemeName = "Bearer";

    public const string DocumentName = "v1";

    public static IServiceCollection AddCommonSwagger(this IServiceCollection services, string serviceName) =>
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(DocumentName, new OpenApiInfo { Title = serviceName, Version = DocumentName });
            options.AddServer(new OpenApiServer { Url = "/" });
            options.AddSecurityDefinition(BearerSchemeName, CreateBearerScheme());
            options.OperationFilter<AuthorizeCheckOperationFilter>();
            options.DocumentFilter<BearerSecurityDocumentFilter>();
        });

    private static OpenApiSecurityScheme CreateBearerScheme() =>
        new()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Access token emitido por auth-service. Se envia como 'Authorization: Bearer <token>'."
        };
}

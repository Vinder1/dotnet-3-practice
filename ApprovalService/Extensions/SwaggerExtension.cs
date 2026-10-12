using System.Reflection;
using Microsoft.OpenApi;

namespace ApprovalService.Extensions;

public static class SwaggerExtension
{
    public static void AddCustomSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Swagger",
                Description = "Put your token here",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });
    }

    /// Modern swagger replacement
    public static void AddCustomScalar(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new();

                document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "Json Web Token",
                        In = ParameterLocation.Header,
                        Description = "Введите JWT токен в формате: Bearer {ваш_токен}"
                    }
                };

                document.Security =
                [
                    new () {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    }
                ];

                return Task.CompletedTask;
            });
        });
    }
}

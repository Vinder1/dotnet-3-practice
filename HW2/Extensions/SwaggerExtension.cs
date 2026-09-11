using System.Reflection;
using Microsoft.OpenApi;

namespace HW2.Extensions;

public static class SwaggerExtension
{
    public static void AddCustomSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

            // options.IncludeXmlComments(xmlPath);

            options.AddSecurityDefinition(builder.Configuration["Swagger:Definition:Id"], new OpenApiSecurityScheme()
            {
                Name = builder.Configuration["Swagger:Definition:Name"],
                Description = builder.Configuration["Swagger:Definition:Description"],
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
}
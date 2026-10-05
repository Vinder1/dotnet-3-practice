using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.OpenSearch;

namespace HW2.Extensions;

public static class SerilogLoggingExtensions
{
    public static WebApplicationBuilder AddCustomSerilog(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Logging.ClearProviders();

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            var opensearch = context.Configuration
                .GetSection("OpenSearch")
                .Get<OpenSearchOptions>() ?? new OpenSearchOptions();

            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithSpan()
                .Enrich.WithProperty("Application", "HW2");

            if (!opensearch.Enabled)
            {
                return;
            }

            var nodeUris = opensearch.NodeUris
                .Where(uri => !string.IsNullOrWhiteSpace(uri))
                .Select(uri => new Uri(uri))
                .ToArray();

            if (nodeUris.Length == 0)
            {
                throw new InvalidOperationException(
                    $"OpenSearch: NodeUris is required when OpenSearch:Enabled is true.");
            }

            loggerConfiguration.WriteTo.OpenSearch(new OpenSearchSinkOptions(nodeUris)
            {
                IndexFormat = opensearch.IndexFormat,
                AutoRegisterTemplate = opensearch.AutoRegisterTemplate,
                DetectOpenSearchVersion = opensearch.DetectOpenSearchVersion,
                NumberOfShards = opensearch.NumberOfShards,
                NumberOfReplicas = opensearch.NumberOfReplicas,
                BatchPostingLimit = opensearch.BatchPostingLimit,
                Period = TimeSpan.FromSeconds(opensearch.PeriodSeconds),
                QueueSizeLimit = opensearch.QueueSizeLimit,
                InlineFields = opensearch.InlineFields,
                ConnectionTimeout = TimeSpan.FromSeconds(opensearch.ConnectionTimeoutSeconds),
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
                FailureCallback = e => Serilog.Debugging.SelfLog.WriteLine(
                    "Unable to submit log event to OpenSearch: {0}", e.MessageTemplate.Text)
            });
        });

        return builder;
    }

    public static WebApplication UseCustomSerilog(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "{RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());
                diagnosticContext.Set("User", httpContext.User.Identity?.Name ?? "anonymous");
            };
        });

        return app;
    }

    public sealed class OpenSearchOptions
    {
        public bool Enabled { get; set; }
        public string[] NodeUris { get; set; } = ["http://localhost:9200"];
        public string IndexFormat { get; set; } = "hw2-logs-{0:yyyy.MM.dd}";
        public bool AutoRegisterTemplate { get; set; } = true;
        public bool DetectOpenSearchVersion { get; set; } = true;
        public int NumberOfShards { get; set; } = 1;
        public int NumberOfReplicas { get; set; } = 0;
        public int BatchPostingLimit { get; set; } = 50;
        public int PeriodSeconds { get; set; } = 2;
        public int QueueSizeLimit { get; set; } = 100_000;
        public bool InlineFields { get; set; } = true;
        public int ConnectionTimeoutSeconds { get; set; } = 5;
    }
}
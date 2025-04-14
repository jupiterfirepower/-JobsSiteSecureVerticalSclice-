using Jobs.Core.Observability.Options;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Settings.Configuration;
using Serilog.Sinks.OpenTelemetry;

namespace Jobs.Core.Extentions;

public static class SerilogBuilderExtention
{
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder, ObservabilityOptions observabilityOptions)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddSerilog((sp, serilog) =>
        {
            serilog
                .ReadFrom.Configuration(configuration, new ConfigurationReaderOptions
                {
                    SectionName = $"{nameof(ObservabilityOptions)}:{nameof(Serilog)}"
                })
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", observabilityOptions.ServiceName)
                .WriteTo.Console();

            serilog
                .WriteTo.OpenTelemetry(c =>
                {
                    c.Endpoint = observabilityOptions.CollectorUrl;
                    c.Protocol = OtlpProtocol.Grpc;
                    c.IncludedData = IncludedData.TraceIdField | IncludedData.SpanIdField | IncludedData.SourceContextAttribute;
                    c.ResourceAttributes = new Dictionary<string, object>
                    {
                        {"service.name", observabilityOptions.ServiceName},
                        {"index", 10},
                        {"flag", true},
                        {"value", 3.14}
                    };
                });
        });

        return builder;
    }
}
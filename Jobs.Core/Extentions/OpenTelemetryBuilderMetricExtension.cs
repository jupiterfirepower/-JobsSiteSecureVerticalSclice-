using Jobs.Core.Observability.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Jobs.Core.Extentions;

public static class OpenTelemetryBuilderMetricExtension
{
    public static OpenTelemetryBuilder AddMetrics(this OpenTelemetryBuilder builder, ObservabilityOptions observabilityOptions)
    {
        builder.WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation();

            /* Add more instrument here */

            /* ============== */
            /* Only export to OpenTelemetry collector */
            /* ============== */

            metrics
                .AddOtlpExporter(exp =>
                {
                    exp.Endpoint = observabilityOptions.CollectorUri;
                    exp.ExportProcessorType = ExportProcessorType.Batch;
                    exp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                });
        });

        return builder;
    }
}
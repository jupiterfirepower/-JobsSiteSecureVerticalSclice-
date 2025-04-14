using Jobs.Core.Observability.Options;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Jobs.Core.Extentions;

public static class OpenTelemetryBuilderTracingExtension
{
    public static OpenTelemetryBuilder AddTracing(this OpenTelemetryBuilder builder, ObservabilityOptions observabilityOptions)
    {
        if (!observabilityOptions.EnabledTracing) return builder;

        builder.WithTracing(tracing =>
        {
            tracing
                .SetErrorStatusOnException()
                .SetSampler(new AlwaysOnSampler())
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                });
            

            /* Add more instrument here: MassTransit, NgSql ... */

            /* ============== */
            /* Only export to OpenTelemetry collector */
            /* ============== */

            tracing
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
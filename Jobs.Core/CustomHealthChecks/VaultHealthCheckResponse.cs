using System.Text.Json.Serialization;

namespace Jobs.Core.CustomHealthChecks;

public record VaultHealthCheckResponse(
    [property: JsonPropertyName("initialized")]
    bool Initialized,
    [property: JsonPropertyName("sealed")] 
    bool Sealed,
    [property: JsonPropertyName("standby")]
    bool Standby,
    [property: JsonPropertyName("performance_standby")]
    bool PerformanceStandby,
    [property: JsonPropertyName("replication_performance_mode")]
    string ReplicationPerformanceMode,
    [property: JsonPropertyName("replication_dr_mode")]
    string ReplicationDrMode,
    [property: JsonPropertyName("server_time_utc")]
    int ServerTimeUtc,
    [property: JsonPropertyName("version")]
    string Version,
    [property: JsonPropertyName("enterprise")]
    bool Enterprise,
    [property: JsonPropertyName("cluster_name")]
    string ClusterName,
    [property: JsonPropertyName("cluster_id")]
    string ClusterId,
    [property: JsonPropertyName("echo_duration_ms")]
    int EchoDurationMs,
    [property: JsonPropertyName("clock_skew_ms")]
    int ClockSkewMs,
    [property: JsonPropertyName("replication_primary_canary_age_ms")]
    int ReplicationPrimaryCanaryAgeMs
);

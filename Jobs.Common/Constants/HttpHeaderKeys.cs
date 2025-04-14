namespace Jobs.Common.Constants;

public static class HttpHeaderKeys
{
    public const string XApiHeaderKey = "x-api-key";
    public const string SNonceHeaderKey = "s-nonce";
    public const string XApiSecretHeaderKey = "x-api-secret";
    public const string AppJsonMediaTypeValue = "application/json";
    public const string XCorrelationIdHeaderKey = "x-correlation-id";
    public const string SerilogCorrelationIdProperty = "CorrelationId";
    public const string XAppIdHeaderKey = "x-app-id";
    public const string XApiNonceHeaderKey = "x-api-nonce";
    public const string Bearer = "Bearer";
    public const int XApiHeaderKeyMaxLength = 100;
    public const int XApiHeaderKeyMinLength = 15;
    public const int SNonceHeaderKeyMaxLength = 64;
    public const int SNonceHeaderKeyMinLength = 10;
    public const int XApiSecretHeaderKeyMaxLength = 64;
    public const int XApiSecretHeaderKeyMinLength = 10;
}
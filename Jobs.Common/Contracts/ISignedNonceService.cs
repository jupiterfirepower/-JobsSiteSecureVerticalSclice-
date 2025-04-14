namespace Jobs.Common.Contracts;

public interface ISignedNonceService
{
    (long, bool) IsSignedNonceValid(string signedNonce);
}
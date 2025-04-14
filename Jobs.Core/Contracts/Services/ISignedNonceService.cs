namespace Jobs.Core.Contracts;

public interface ISignedNonceService
{
    (long, bool) IsSignedNonceValid(string signedNonce);
}
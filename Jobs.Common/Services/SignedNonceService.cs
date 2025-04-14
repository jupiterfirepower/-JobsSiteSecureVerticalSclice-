using Jobs.Common.Contracts;
using Jobs.Common.Helpers;

namespace Jobs.Common.Services;

public class SignedNonceService: ISignedNonceService
{
    private readonly NonceParserHelper _helper = new ();

    public (long, bool) IsSignedNonceValid(string signedNonce) => _helper.IsSignedNonceValid(signedNonce);
}
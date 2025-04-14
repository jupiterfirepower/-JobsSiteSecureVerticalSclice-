using Jobs.Core.Contracts;
using Jobs.Core.Helpers;

namespace Jobs.Core.Services;

public class SignedNonceService: ISignedNonceService
{
    private readonly NonceParserHelper _helper = new ();

    public (long, bool) IsSignedNonceValid(string signedNonce) => _helper.IsSignedNonceValid(signedNonce);
}
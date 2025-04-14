namespace Jobs.Core.Helpers;

public class NonceParserHelper
{
    private const char Delimiter = '-';
    
    public (long, bool) IsSignedNonceValid(string signedNonce)
    {
        var splitted = signedNonce.Split(Delimiter);
        
        if(splitted.Length != 3)
            return (-1, false);
        
        var reverse = new string(splitted[0].Reverse().ToArray());
        var signs = long.Parse(splitted[1]);
        
        var nonce = long.Parse(reverse);

        var roundedSign = GetSignForNonce(nonce);
        var roundedSumSign = GetSumSignForNonce(nonce);
        
        var signsSum = long.Parse(splitted[2]);
        
        return signs == roundedSign && signsSum == roundedSumSign ? (nonce, true) : (-1, false);
    }

    private long GetSignForNonce(long nonce)
    {
        var ticks = nonce;
        var sign = ticks * Math.PI * Math.E;
        var roundedSign= (long)Math.Ceiling(sign);
        return roundedSign;
    }
    
    private long GetSumSignForNonce(long nonce)
    {
        var ticks = nonce;
        var signFirst = ticks * Math.PI;
        var roundedSignFirst = (long)Math.Ceiling(signFirst);
        var signSecond = ticks * Math.E;
        var roundedSignSecond = (long)Math.Ceiling(signSecond);
        
        var roundedSum = roundedSignFirst + roundedSignSecond;
        return roundedSum;
    }
    
}
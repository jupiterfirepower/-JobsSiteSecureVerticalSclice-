namespace Jobs.YarpGateway.Helpers;

public class NonceHelper
{
    public static string GenerateNonce()
    {
        var ticks = DateTime.UtcNow.Ticks;
        var sign = ticks * Math.PI * Math.E;
        var rounded = (long)Math.Ceiling(sign);
        var reverseNonce = new string(ticks.ToString().Reverse().ToArray());
        
        var signFirst = ticks * Math.PI;
        var roundedSignFirst = (long)Math.Ceiling(signFirst);
        var signSecond = ticks * Math.E;
        var roundedSignSecond = (long)Math.Ceiling(signSecond);
        
        var roundedSum = roundedSignFirst + roundedSignSecond;
        
        /*int[] intArray = ticks.ToString()
            .ToArray()
            .Select(x=>x.ToString())
            .Select(int.Parse)
            .ToArray(); */
        
        var nonceValue = $"{reverseNonce}-{rounded}-{roundedSum}";
        
        Console.WriteLine($"s-nonce : {nonceValue}");

        return nonceValue;
    }
}
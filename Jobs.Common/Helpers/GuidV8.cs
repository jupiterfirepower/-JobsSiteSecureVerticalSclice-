using System.Security.Cryptography;

namespace Jobs.Common.Helpers;

public class GuidV8
{
    public static Guid NewGuidV8(int regionId, int shardId, byte firstCode, byte secondCode)
    {
        byte[] uuidBytes = new byte[16];

        // Embed custom region and shard IDs
        uuidBytes[0] = (byte)regionId;
        uuidBytes[1] = (byte)shardId;

        // Fill the remaining bytes with random values
        RandomNumberGenerator.Fill(uuidBytes.AsSpan(2, 14));
        
        // Set version and variant
        uuidBytes[6] = (byte)(8 << 4);  // Version 8
        //uuidBytes[6] = 0b1000_0000;  // Version 8
        uuidBytes[8] = 0b1000_0000; // IETF variant
        
        uuidBytes[14] = firstCode;
        uuidBytes[15] = secondCode;
        
        return new Guid(uuidBytes, true); //true
    }
    
    /*public int GetSecureCode() 
    {
        byte[] bytes = new byte[2];
        _value.ToByteArray(true)[14..15].CopyTo(bytes, 0);
        if (BitConverter.IsLittleEndian) 
        {
            Array.Reverse(bytes);
        }
        return bytes[0] + bytes[1];
    }*/
}
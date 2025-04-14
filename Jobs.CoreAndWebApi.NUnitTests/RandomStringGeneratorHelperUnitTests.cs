using Jobs.Common.Helpers;

namespace JobsWebApiNUnitTests;

public class RandomStringGeneratorHelperUnitTests
{
    [Test]
    public void RandomStringGeneratorHelperTest()
    {
        var randomString = RandomStringGeneratorHelper.GenRandomString(30);
        Assert.IsNotNull(randomString);
        Assert.True(randomString.Length == 30);
        
        var randomString1 = RandomStringGeneratorHelper.GenRandomString(35);
        Assert.IsNotNull(randomString1);
        Assert.True(randomString1.Length == 35);
        
        var randomString2 = RandomStringGeneratorHelper.GenRandomString(50);
        Assert.IsNotNull(randomString2);
        Assert.True(randomString2.Length == 50);
    }
}
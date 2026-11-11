using System.Text.Json;
using DCB.Gateway.Core;
using DCB.Gateway.Tests.TestData;

namespace DCB.Gateway.Tests.Core;

public class DeconzErrorReaderTests
{
    [Fact]
    public void ReadErrors_MixedResult_ReturnsOnlyErrors()
    {
        JsonElement root = JsonDocument.Parse(DeconzSamples.MixedResult).RootElement;

        var errors = DeconzErrorReader.ReadErrors(root);

        var error = Assert.Single(errors);
        Assert.Equal(DeconzErrorTypes.InvalidValue, error.Type);
        Assert.Equal("/lights/3/state/bri", error.Address);
    }

    [Fact]
    public void ReadErrors_ObjectResponse_ReturnsEmpty()
    {
        JsonElement root = JsonDocument.Parse(DeconzSamples.FullConfig).RootElement;

        Assert.Empty(DeconzErrorReader.ReadErrors(root));
    }
}

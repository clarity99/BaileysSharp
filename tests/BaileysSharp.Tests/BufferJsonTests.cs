using System.Text.Json.Nodes;
using BaileysSharp.Utils;

namespace BaileysSharp.Tests;

public class BufferJsonTests
{
    [Fact]
    public void RoundTrip_StringifyAndParse_PreservesBufferShapes()
    {
        var originalObject = new JsonObject
        {
            ["id"] = 1,
            ["key"] = new JsonObject
            {
                ["0"] = 1,
                ["1"] = 2,
                ["2"] = 3,
                ["3"] = 4,
                ["4"] = 5
            },
            ["nested"] = new JsonObject
            {
                ["data"] = new JsonObject
                {
                    ["0"] = 6,
                    ["1"] = 7,
                    ["2"] = 8
                }
            }
        };

        var serialized = BufferJson.Stringify(originalObject);
        Assert.Contains("\"type\":\"Buffer\"", serialized);
        Assert.DoesNotContain("\"data\":[1,2,3,4,5]", serialized);

        var revived = BufferJson.Parse(serialized)!.AsObject();
        Assert.Equal(1, revived["id"]!.GetValue<int>());
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, revived["key"]!.AsArray().Select(x => x!.GetValue<int>()).ToArray());
    }

    [Fact]
    public void Parse_RevivesLegacyBufferObject()
    {
        const string legacyJson = "{\"id\":1,\"key\":{\"0\":1,\"1\":2,\"2\":3,\"3\":4,\"4\":5},\"nested\":{\"data\":{\"0\":6,\"1\":7,\"2\":8}}}";
        var revived = BufferJson.Parse(legacyJson)!.AsObject();

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, revived["key"]!.AsArray().Select(x => x!.GetValue<int>()).ToArray());
        Assert.Equal(new[] { 6, 7, 8 }, revived["nested"]!["data"]!.AsArray().Select(x => x!.GetValue<int>()).ToArray());
    }

    [Fact]
    public void Parse_DoesNotCorruptLegitimateObjects()
    {
        const string json = "{\"0\":\"some-value\",\"1\":\"another-value\"}";
        var revived = BufferJson.Parse(json)!.AsObject();

        Assert.Equal("some-value", revived["0"]!.GetValue<string>());
        Assert.Equal("another-value", revived["1"]!.GetValue<string>());
    }
}

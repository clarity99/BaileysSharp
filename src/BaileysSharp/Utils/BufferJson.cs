using System.Text.Json;
using System.Text.Json.Nodes;

namespace BaileysSharp.Utils;

public static class BufferJson
{
    public static string Stringify<T>(T value)
    {
        var node = JsonSerializer.SerializeToNode(value, SerializerOptions) ?? new JsonObject();
        var transformed = TransformForSerialization(node);
        return transformed.ToJsonString();
    }

    public static JsonNode? Parse(string json)
    {
        var node = JsonNode.Parse(json);
        return TransformForDeserialization(node);
    }

    private static JsonNode TransformForSerialization(JsonNode? node)
    {
        return node switch
        {
            JsonArray arr => new JsonArray(arr.Select(TransformForSerialization).ToArray()),
            JsonObject obj => TransformObjectForSerialization(obj),
            _ => node?.DeepClone() ?? JsonValue.Create((string?)null)!
        };
    }

    private static JsonObject TransformObjectForSerialization(JsonObject obj)
    {
        if (TryReadByteArray(obj, out var bytes))
        {
            return new JsonObject
            {
                ["type"] = "Buffer",
                ["data"] = Convert.ToBase64String(bytes)
            };
        }

        var clone = new JsonObject();
        foreach (var kv in obj)
            clone[kv.Key] = TransformForSerialization(kv.Value);
        return clone;
    }

    private static JsonNode? TransformForDeserialization(JsonNode? node)
    {
        return node switch
        {
            JsonArray arr => new JsonArray(arr.Select(TransformForDeserialization).ToArray()),
            JsonObject obj => TransformObjectForDeserialization(obj),
            _ => node?.DeepClone()
        };
    }

    private static JsonNode TransformObjectForDeserialization(JsonObject obj)
    {
        if (TryReadBufferFormat(obj, out var bytes) || TryReadLegacyNumericObject(obj, out bytes))
            return new JsonArray(bytes.Select(b => JsonValue.Create((int)b)).ToArray());

        var clone = new JsonObject();
        foreach (var kv in obj)
            clone[kv.Key] = TransformForDeserialization(kv.Value);
        return clone;
    }

    private static bool TryReadByteArray(JsonObject obj, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();
        if (obj.Count == 0)
            return false;

        var ordered = new List<byte>();
        foreach (var key in obj.Select(k => k.Key).OrderBy(k => k))
        {
            if (!int.TryParse(key, out _))
                return false;

            if (obj[key] is not JsonValue jsonValue || !jsonValue.TryGetValue<int>(out var intByte) || intByte is < 0 or > 255)
                return false;

            ordered.Add((byte)intByte);
        }

        bytes = ordered.ToArray();
        return true;
    }

    private static bool TryReadBufferFormat(JsonObject obj, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();
        if (!obj.TryGetPropertyValue("type", out var typeNode) ||
            !obj.TryGetPropertyValue("data", out var dataNode) ||
            typeNode is not JsonValue typeValue ||
            !typeValue.TryGetValue<string>(out var type) ||
            type != "Buffer")
        {
            return false;
        }

        if (dataNode is JsonValue stringValue && stringValue.TryGetValue<string>(out var base64))
        {
            try
            {
                bytes = Convert.FromBase64String(base64);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        if (dataNode is not JsonArray dataArray)
            return false;

        var list = new List<byte>();
        foreach (var item in dataArray)
        {
            if (item is not JsonValue val || !val.TryGetValue<int>(out var intByte) || intByte is < 0 or > 255)
                return false;
            list.Add((byte)intByte);
        }

        bytes = list.ToArray();
        return true;
    }

    private static bool TryReadLegacyNumericObject(JsonObject obj, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();
        if (obj.Count == 0)
            return false;

        var pairs = new List<(int index, byte value)>();
        foreach (var kv in obj)
        {
            if (!int.TryParse(kv.Key, out var index) || index < 0)
                return false;
            if (kv.Value is not JsonValue val || !val.TryGetValue<int>(out var intByte) || intByte is < 0 or > 255)
                return false;
            pairs.Add((index, (byte)intByte));
        }

        pairs.Sort((a, b) => a.index.CompareTo(b.index));
        bytes = pairs.Select(p => p.value).ToArray();
        return true;
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Toolbelt.Blazor.WebAssembly.ExtensibleGateway.UserSecretsExtension;

/// <summary>
/// Merges the User Secrets of a project into the "appsettings.*.json" content of a Blazor
/// WebAssembly standalone app, the way the configuration system would layer them.
/// </summary>
internal static class JsonMerger
{
    public static string Merge(string baseJson, string overrideJson)
    {
        var baseNode = JsonNode.Parse(baseJson);
        var overrideNode = JsonNode.Parse(overrideJson);

        if (baseNode is JsonObject baseObj) ExpandColonDelimitedKeys(baseObj);
        if (overrideNode is JsonObject overrideObj) ExpandColonDelimitedKeys(overrideObj);

        var mergedNode = MergeNode(baseNode, overrideNode);
        return mergedNode is null
            ? "null"
            : mergedNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Rewrites a flat key such as "Foo:Bar", which is what "dotnet user-secrets set" writes, into
    /// the nested object the same key means to the configuration system. Without this, merging
    /// would leave both spellings of the key in the response.
    /// </summary>
    private static void ExpandColonDelimitedKeys(JsonObject obj)
    {
        var colonKeys = obj
            .Where(kvp => kvp.Key.Contains(':'))
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToArray();

        foreach (var (key, value) in colonKeys)
        {
            obj.Remove(key);

            var segments = key.Split(':');
            var current = obj;

            for (var i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];
                if (current[segment] is JsonObject existing)
                {
                    current = existing;
                }
                else
                {
                    var newObj = new JsonObject();
                    current[segment] = newObj;
                    current = newObj;
                }
            }

            current[segments[^1]] = value?.DeepClone();
        }

        foreach (var (_, value) in obj)
        {
            if (value is JsonObject child) ExpandColonDelimitedKeys(child);
        }
    }

    private static JsonNode? MergeNode(JsonNode? baseNode, JsonNode? overrideNode)
    {
        if (overrideNode is null) return null;

        if (baseNode is null) return overrideNode.DeepClone();

        if (baseNode is JsonObject baseObj && overrideNode is JsonObject overrideObj)
        {
            var merged = baseObj.DeepClone() as JsonObject ?? new JsonObject();

            foreach (var (key, value) in overrideObj)
            {
                if (merged[key] is JsonObject existingObj && value is JsonObject overrideChild)
                {
                    merged[key] = MergeNode(existingObj, overrideChild);
                }
                else
                {
                    merged[key] = value?.DeepClone();
                }
            }

            return merged;
        }

        return overrideNode.DeepClone();
    }
}

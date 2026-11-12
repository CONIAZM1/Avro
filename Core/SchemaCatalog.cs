using Avro;
namespace AvroSpike.Core;

/// <summary>Central source: each version is an explicit .avsc file. No cache.</summary>
public sealed class SchemaCatalog(string schemasDirectory)
{
    public string Read(int version)
    {
        // Only the three demo releases are allowed, never arbitrary paths.
        if (version is < 1 or > 3) throw new ArgumentOutOfRangeException(nameof(version));
        var json = File.ReadAllText(Path.Combine(schemasDirectory, $"policy.v{version}.avsc"));
        _ = Schema.Parse(json); // The API validates that the document is a real Avro schema.
        return json;
    }

    public static string GenerateCSharp(string schemaJson)
    {
        // OFFICIAL Apache Avro generator: the same Policy name for all versions.
        // Runs when each build request is received; does not reuse previous code.
        var generator = new CodeGen();
        generator.AddSchema(schemaJson);
        generator.GenerateCode();
        return generator.GetTypes()["Policy"];
    }
}

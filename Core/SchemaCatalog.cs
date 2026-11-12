using Avro;
namespace AvroSpike.Core;

/// <summary>Fuente central: cada version es un .avsc explicito. No hay cache.</summary>
public sealed class SchemaCatalog(string schemasDirectory)
{
    public string Read(int version)
    {
        // Solo permitimos las tres publicaciones de la demo, nunca paths arbitrarios.
        if (version is < 1 or > 3) throw new ArgumentOutOfRangeException(nameof(version));
        var json = File.ReadAllText(Path.Combine(schemasDirectory, $"policy.v{version}.avsc"));
        _ = Schema.Parse(json); // La API valida que el documento sea un schema Avro real.
        return json;
    }

    public static string GenerateCSharp(string schemaJson)
    {
        // Generador OFICIAL Apache Avro: mismo nombre Policy para todas las versiones.
        // Se ejecuta al recibir cada peticion del build; no reutiliza codigo previo.
        var generator = new CodeGen();
        generator.AddSchema(schemaJson);
        generator.GenerateCode();
        return generator.GetTypes()["Policy"];
    }
}

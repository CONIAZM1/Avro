using Avro.IO;
using Avro.Specific;
namespace AvroSpike.Core;

public static class AvroCodec
{
    /// <summary>Round trip real de bytes Avro, no una simple copia del objeto.</summary>
    public static T RoundTrip<T>(T policy) where T : ISpecificRecord, new()
    {
        using var buffer = new MemoryStream();
        var encoder = new BinaryEncoder(buffer);
        new SpecificDatumWriter<T>(policy.Schema).Write(policy, encoder);
        encoder.Flush();
        buffer.Position = 0;
        // En este ejemplo BLL y consola comparten EXACTAMENTE la misma version.
        // La lectura entre versiones distintas es otro escenario, no se simula aqui.
        return new SpecificDatumReader<T>(policy.Schema, new T().Schema)
            .Read(new T(), new BinaryDecoder(buffer));
    }
}

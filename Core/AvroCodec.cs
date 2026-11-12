using Avro.IO;
using Avro.Specific;
namespace AvroSpike.Core;

public static class AvroCodec
{
    /// <summary>A real Avro byte round trip, not just an object copy.</summary>
    public static T RoundTrip<T>(T policy) where T : ISpecificRecord, new()
    {
        using var buffer = new MemoryStream();
        var encoder = new BinaryEncoder(buffer);
        new SpecificDatumWriter<T>(policy.Schema).Write(policy, encoder);
        encoder.Flush();
        buffer.Position = 0;
        // In this example, BLL and the console share EXACTLY the same version.
        // Reading across different versions is a separate scenario, not simulated here.
        return new SpecificDatumReader<T>(policy.Schema, new T().Schema)
            .Read(new T(), new BinaryDecoder(buffer));
    }
}

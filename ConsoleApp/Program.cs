using System.Reflection;
using AvroSpike.BLL;
using AvroSpike.Contracts;

// The same program works for v1/v2/v3. The list changes because the C# class
// was generated and compiled with different properties, not because fields are hidden on screen.
var version = typeof(Policy).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
    .Single(a => a.Key == "PolicySchemaVersion").Value!;
var properties = typeof(Policy).GetProperties(BindingFlags.Public | BindingFlags.Instance)
    .Where(p => p.Name != "Schema").ToArray();
var expected = int.Parse(version) * 5;
if (properties.Length != expected)
    throw new InvalidOperationException($"Expected {expected} generated properties; found {properties.Length}.");

Console.WriteLine($"AVRO SPIKE - Policy v{version}");
Console.WriteLine($"Generated C# properties: {properties.Length} (expected {expected})");
Console.WriteLine("Console -> BLL.GetAll() -> typed Policy -> real Avro round trip");
Console.WriteLine(new string('=', 76));

var policies = new PolicyService().GetAll();
foreach (var policy in policies)
{
    foreach (var property in properties)
        Console.WriteLine($"{property.Name,-22}: {property.GetValue(policy)}");
    Console.WriteLine(new string('-', 76));
}
Console.WriteLine($"PASS: {policies.Count} policies, {properties.Length} properties each, version v{version}.");
Console.WriteLine("No schema/API request at runtime: the API was used during BUILD.");
Console.WriteLine("Press Enter to close.");
Console.ReadLine();

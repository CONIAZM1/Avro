using Avro;
using AvroSpike.Contracts;
using AvroSpike.Core;

namespace AvroSpike.BLL;

public sealed class PolicyService
{
    /// <summary>
    /// Devuelve objetos Policy FUERTEMENTE TIPADOS de la version compilada.
    /// Los datos ficticios contienen el superset v3. Se copian solo los campos
    /// que existen realmente en la clase generada, sin recompilar codigo de negocio.
    /// </summary>
    public IReadOnlyList<Policy> GetAll()
    {
        var schema = (RecordSchema)new Policy().Schema;
        return new[] { CreateRow("001"), CreateRow("002") }.Select(row =>
        {
            var policy = new Policy();
            foreach (var field in schema.Fields)
            {
                // La reflexion evita referencias a propiedades que todavia no existen en v1.
                // Esto es una tecnica para los datos de LA DEMO, no una regla de negocio.
                typeof(Policy).GetProperty(field.Name)!.SetValue(policy, row[field.Name]);
            }
            return AvroCodec.RoundTrip(policy); // Verifica el contrato con bytes Avro reales.
        }).ToArray();
    }

    private static Dictionary<string, string> CreateRow(string id) => new()
    {
        ["VendorName"] = "DEMO Vendor " + id,
        ["VendorAddress"] = "100 Example Street",
        ["VendorTaxId"] = "DEMO-TAX-" + id,
        ["CustomerName"] = "DEMO Customer " + id,
        ["CustomerAddress"] = "200 Sample Avenue",
        ["InvoiceId"] = "DEMO-INV-" + id,
        ["PurchaseOrder"] = "DEMO-PO-" + id,
        ["InvoiceDate"] = "2026-11-01",
        ["DueDate"] = "2026-12-01",
        ["SubTotal"] = "1000.00",
        ["TotalTax"] = "210.00",
        ["InvoiceTotal"] = "1210.00",
        ["AmountDue"] = "1210.00",
        ["Currency"] = "USD",
        ["PaymentTerms"] = "NET 30"
    };
}

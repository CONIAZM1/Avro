using Avro;
using AvroSpike.Contracts;
using AvroSpike.Core;

namespace AvroSpike.BLL;

public sealed class PolicyService
{
    /// <summary>
    /// Returns STRONGLY TYPED Policy objects from the compiled version.
    /// The fictional data contains the v3 superset. Only fields that actually
    /// exist in the generated class are copied, without recompiling business logic.
    /// </summary>
    public IReadOnlyList<Policy> GetAll()
    {
        var schema = (RecordSchema)new Policy().Schema;
        return new[] { CreateRow("001"), CreateRow("002") }.Select(row =>
        {
            var policy = new Policy();
            foreach (var field in schema.Fields)
            {
                // Reflection avoids references to properties that do not yet exist in v1.
                // This is a technique for THE DEMO data, not a business rule.
                typeof(Policy).GetProperty(field.Name)!.SetValue(policy, row[field.Name]);
            }
            return AvroCodec.RoundTrip(policy); // Verifies the contract using real Avro bytes.
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

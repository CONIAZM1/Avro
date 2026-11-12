# AvroSpike - build-time generation demo (.NET 10)
## Five projects
- Core: schema reading/validation, official Apache Avro generator, binary round trip.
- SchemaCatalog.Api: central source exposed over HTTP. Independent of Contracts.
- Contracts: downloads the schema AND generated C# on EVERY build. No cache or fallback.
- BLL: GetAll() returns Policy objects from the compiled contract.
- ConsoleApp: calls BLL and displays the actual properties of the generated type.

## Demo in Visual Studio
1. Open AvroSpike.sln.
2. Select the Catalog configuration. Set Startup Project = SchemaCatalog.Api.
3. Ctrl+F5 starts the API at http://localhost:5185. Leave it running.
4. Return to Visual Studio. Set Startup Project = ConsoleApp.
5. Select V1 and press Ctrl+F5: 2 policies, 5 properties each.
6. Close the demo console. Select V2 and press Ctrl+F5: 10 properties.
7. Select V3 and press Ctrl+F5: 15 properties.
8. Output/Build shows AVRO requesting...; /build-requests shows the requests.
9. Build again without changing the version: it queries the API again.
10. If the API is stopped, Contracts must fail to build. The obj files are not a cache/fallback.

The Catalog configuration builds only Core/API; V1-V3 build the consumers.
Core is compiled in Catalog so that BLL reuses that library without rebuilding
the files used by the running API. There is no cycle or automatic API startup.

## Fictional contract versions
V1: VendorName, CustomerName, InvoiceId, InvoiceDate, InvoiceTotal.
V2 adds: VendorAddress, VendorTaxId, CustomerAddress, DueDate, Currency.
V3 adds: PurchaseOrder, SubTotal, TotalTax, AmountDue, PaymentTerms.
Decimal amounts and dates use string in this demo to avoid unvalidated implicit
mappings. The schemas retain the fullname AvroSpike.Contracts.Policy.
New fields have an empty default value for reading historical data.

## Clarifications
The API uses Core/Apache CodeGen to generate C# WHEN IT RECEIVES the build request.
Contracts downloads that source and compiles it; avrogen does not run on the client.
There are no terminal commands or Exec tasks in the build.
Reflection in BLL/Console allows a single implementation to compile with all
three contract shapes. It does not hide fields: Policy actually has 5/10/15 properties.
Each GetAll call also serializes/deserializes Avro bytes using the compiled type.
The data is entirely fictional. No actual customer services/products are used.

## Setup
The Setup folder contains only the writing assistant created and run from Visual Studio.
It is not part of the five projects in AvroSpike.sln or the demo.

## Verification performed on the VM (November 12, 2026)
- Catalog: Core and API built successfully; the API listened on localhost:5185.
- V1: built and listed 2 policies with 5 properties each (PASS).
- V2: built and listed 2 policies with 10 properties each (PASS).
- V3: built and listed 2 policies with 15 properties each (PASS).
- Repeated V3 build without changes: two additional HTTP downloads were observed (avsc + csharp).
- API stopped: Build Solution failed with MSB3923, connection refused; it did not use previous files as a fallback.
- All five projects are created without depending on Docker, Azure, or a broker.

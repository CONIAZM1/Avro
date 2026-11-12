# AvroSpike - demo de generacion en build (.NET 10)
## Cinco proyectos
- Core: lectura/validacion de schemas, generador oficial Apache Avro, round trip binario.
- SchemaCatalog.Api: fuente central publicada por HTTP. Independiente de Contracts.
- Contracts: descarga schema Y C# generado en CADA build. Sin cache ni fallback.
- BLL: GetAll() devuelve Policy del contrato compilado.
- ConsoleApp: llama BLL y muestra las propiedades reales del tipo generado.

## Demo desde Visual Studio
1. Abrir AvroSpike.sln.
2. Elegir configuracion Catalog. Set Startup Project = SchemaCatalog.Api.
3. Ctrl+F5 inicia API en http://localhost:5185. Dejarla abierta.
4. Volver a Visual Studio. Set Startup Project = ConsoleApp.
5. Elegir V1 y Ctrl+F5: 2 policies, 5 propiedades cada una.
6. Cerrar la consola del ejemplo. Elegir V2 y Ctrl+F5: 10 propiedades.
7. Elegir V3 y Ctrl+F5: 15 propiedades.
8. Output/Build muestra AVRO requesting...; /build-requests muestra las consultas.
9. Volver a Build sin cambiar version: consulta la API otra vez.
10. Si API esta apagada, Contracts debe fallar. Los archivos obj no son cache/fallback.

La configuracion Catalog construye solo Core/API; V1-V3 construyen los consumidores.
Core se compila en Catalog para que la BLL reutilice esa biblioteca sin reconstruir
los archivos que usa la API en ejecucion. No hay ciclo ni arranque automatico de API.

## Versiones del contrato ficticio
V1: VendorName, CustomerName, InvoiceId, InvoiceDate, InvoiceTotal.
V2 suma: VendorAddress, VendorTaxId, CustomerAddress, DueDate, Currency.
V3 suma: PurchaseOrder, SubTotal, TotalTax, AmountDue, PaymentTerms.
Importes decimales y fechas usan string en esta demo para evitar mappings implicitos
no validados. Los schemas mantienen fullname AvroSpike.Contracts.Policy.
Los campos nuevos tienen default vacio para lectura de datos historicos.

## Precisiones
La API usa Core/Apache CodeGen para generar C# AL RECIBIR la peticion del build.
Contracts descarga esa fuente y la compila; no se ejecuta avrogen en el cliente.
No hay comandos de terminal ni tareas Exec en el build.
La reflexion en BLL/Console permite una sola implementacion que compila con las
tres formas del contrato. No oculta campos: Policy tiene realmente 5/10/15 propiedades.
Cada GetAll tambien serializa/deserializa bytes Avro usando el tipo compilado.
Los datos son completamente ficticios. No se usan servicios/productos reales del cliente.

## Setup
La carpeta Setup es solo el asistente de escritura creado y ejecutado desde Visual Studio.
No forma parte de los cinco proyectos de AvroSpike.sln ni de la demo.

## Verificacion ejecutada en la VM (12 noviembre 2026)
- Catalog: Core y API compilaron correctamente; API escucho en localhost:5185.
- V1: compilo y listo 2 policies con 5 propiedades cada una (PASS).
- V2: compilo y listo 2 policies con 10 propiedades cada una (PASS).
- V3: compilo y listo 2 policies con 15 propiedades cada una (PASS).
- Build repetido V3 sin cambios: se observaron otras dos descargas HTTP (avsc + csharp).
- API apagada: Build Solution fallo MSB3923, conexion rechazada; no uso archivos previos como fallback.
- Los cinco proyectos se crean sin depender de Docker, Azure ni un broker.

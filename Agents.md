# Guia para agentes

- Proyecto .NET 8 en C# para Windows Forms. Solucion: `Firmador.slnx`.
- Objetivo funcional: cliente local para listar PDFs pendientes y firmarlos digitalmente con certificado de token USB usando iText7.
- Mantener las respuestas y cambios en espanol, salvo APIs/nombres tecnicos existentes.

## Estructura

- `Firmador.Cliente`: aplicacion WinForms. `MainForm` contiene la UI, busqueda, seleccion de documentos y flujo de firma.
- `Firmador.Cliente/Services/WindowsPdfSigningService.cs`: firma PDFs con iText7, certificado Windows y firma visible.
- `Firmador.Cliente/Services/CertificateSelectorService.cs`: selecciona certificados vigentes con clave privada desde `CurrentUser/My`.
- `Firmador.ApiClient`: abstracciones y cliente mock para documentos. Hoy usa `MockDocumentosApiClient`.
- `Firmador.Core`: contratos, modelos y resultados compartidos (`IFirmaPdfService`, `DocumentoResumen`, `PagedResult`, etc.).
- `docs`: PDFs de prueba que se copian al output del cliente.

## Reglas de negocio actuales

- El usuario busca documentos a firmar desde la UI.
- La grilla muestra seleccion, tipo, titulo y boton para ver PDF; id/hash quedan internos.
- Para firmar debe haber al menos un documento seleccionado.
- El certificado debe seleccionarse una vez y reutilizarse hasta cerrar la aplicacion, salvo seleccion manual nueva.
- Los PDFs firmados se guardan localmente en la carpeta de salida definida por `SolutionPaths`.

## Como trabajar

- Preferir cambios chicos y coherentes con la arquitectura actual: UI en `Firmador.Cliente`, contratos en `Core`, integraciones/API en `Firmador.ApiClient`.
- No meter logica de firma dentro del formulario si puede quedar en servicios.
- Cuidar compatibilidad Windows y WinForms; la app depende del almacen de certificados de Windows y de tokens USB expuestos como certificados con clave privada.
- Evitar recorrer o modificar los PDFs de `docs` salvo que la tarea lo pida.

## Comandos utiles

- Compilar: `dotnet build Firmador.slnx`
- Ejecutar cliente: `dotnet run --project Firmador.Cliente/Firmador.Cliente.csproj`

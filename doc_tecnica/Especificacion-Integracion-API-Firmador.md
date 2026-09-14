# Especificación de integración API para Firmador

**Versión:** 1.0  
**Fecha:** 14 de septiembre de 2026  
**Estado:** Propuesta para revisión del equipo de desarrollo web

## Propósito

Este documento define el contrato inicial entre Firmador, una aplicación de escritorio para Windows, y el sistema web que administra los documentos pendientes de firma. El equipo web debe implementar y publicar estas operaciones antes de reemplazar el cliente simulado que utiliza actualmente Firmador.

Firmador autenticará al usuario, consultará sus documentos pendientes, descargará el PDF elegido, lo firmará localmente mediante un certificado disponible en Windows y enviará el PDF firmado al backend. El token USB, la clave privada y el certificado usado para firmar permanecen en el equipo del usuario.

La primera versión del backend almacenará el archivo recibido, pero no validará criptográficamente la firma ni comprobará que la identidad del certificado corresponda al usuario autenticado. Por esa razón, el resultado se denomina `received` y no firma validada.

## Alcance de la primera versión

La integración incluye:

- Inicio, renovación y cierre de sesión.
- Consulta paginada de documentos pendientes del usuario autenticado.
- Descarga controlada del PDF original.
- Verificación local del SHA-256 del archivo descargado.
- Firma local del PDF.
- Carga individual e idempotente del PDF firmado.
- Control de concurrencia mediante una versión lógica y el hash del documento original.
- Errores HTTP con una estructura uniforme.

Quedan fuera de esta versión:

- Validación criptográfica de la firma en el backend.
- Correspondencia entre la identidad del certificado y la cuenta autenticada.
- Validación de cadena de confianza o revocación del certificado en el backend.
- Sellado de tiempo mediante una autoridad TSA.
- Envío de varios PDFs en una única operación transaccional.
- Persistencia de la sesión después de cerrar Firmador.

## Flujo de integración

1. Firmador envía las credenciales del usuario al endpoint de inicio de sesión.
2. El backend devuelve un access token de corta duración y un refresh token rotatorio.
3. Firmador consulta la primera página de documentos pendientes. El backend identifica al usuario mediante el token; el cliente no envía un `userId`.
4. Para ver o firmar un documento, Firmador solicita el PDF indicando la versión obtenida en el listado.
5. Firmador calcula el SHA-256 del PDF descargado y lo compara con `sha256`.
6. Firmador firma localmente el archivo con el certificado seleccionado por el usuario.
7. Firmador envía solamente el PDF firmado, la versión del original y el SHA-256 del original.
8. El backend vuelve a comprobar autorización, estado, versión y hash, almacena el PDF firmado y marca el documento como recibido.
9. Firmador procesa cada documento seleccionado por separado y muestra los éxitos y errores individuales.

## Convenciones generales

### Transporte y formato

- Todas las operaciones deben estar disponibles únicamente mediante HTTPS.
- La URL base será configurable en Firmador y no se incluirá una URL productiva en el código fuente.
- Los endpoints usarán el prefijo `/api/v1`.
- Las respuestas JSON usarán `application/json; charset=utf-8`.
- Los errores usarán `application/problem+json`.
- Las fechas se expresarán en UTC con formato ISO 8601, por ejemplo `2026-09-14T18:45:00Z`.
- Los identificadores serán enteros positivos en esta versión.
- Los valores de `version` serán cadenas opacas. El cliente deberá conservarlos y devolverlos sin interpretarlos.
- Los hashes SHA-256 se expresarán mediante 64 caracteres hexadecimales en minúscula.

### Autorización

Excepto el inicio y la renovación de sesión, toda solicitud requiere:

```http
Authorization: Bearer {accessToken}
```

El backend debe obtener la identidad y los permisos desde el token. No debe aceptar un identificador de usuario enviado por el cliente para decidir qué documentos listar, descargar o actualizar.

### Duración de la sesión

Como configuración inicial se propone:

- Access token: 15 minutos.
- Refresh token: 8 horas como duración máxima de la sesión.
- Rotación del refresh token en cada renovación.
- Tokens almacenados solamente en memoria por Firmador.

Estos valores deben ser configurables. Al cerrar Firmador, la aplicación intentará revocar la sesión y descartará los tokens aunque no haya conectividad.

## Resumen de endpoints

| Método | Ruta | Propósito | Autorización |
| --- | --- | --- | --- |
| `POST` | `/api/v1/sessions` | Autenticar al usuario | No |
| `POST` | `/api/v1/sessions/refresh` | Renovar los tokens | Refresh token |
| `POST` | `/api/v1/sessions/logout` | Revocar la sesión | Access token y refresh token |
| `GET` | `/api/v1/documents` | Listar documentos pendientes | Bearer token |
| `GET` | `/api/v1/documents/{id}/pdf` | Descargar el PDF original | Bearer token |
| `POST` | `/api/v1/documents/{id}/signed-pdf` | Almacenar el PDF firmado | Bearer token |

## Iniciar sesión

### Solicitud

```http
POST /api/v1/sessions
Content-Type: application/json
Accept: application/json, application/problem+json
```

```json
{
  "username": "usuario.ejemplo",
  "password": "contraseña"
}
```

### Respuesta correcta

Estado `200 OK`:

```json
{
  "accessToken": "eyJ...",
  "tokenType": "Bearer",
  "expiresIn": 900,
  "refreshToken": "valor-opaco",
  "user": {
    "id": 123,
    "username": "usuario.ejemplo",
    "displayName": "Usuario Ejemplo"
  }
}
```

`expiresIn` indica la duración del access token en segundos. El refresh token debe ser opaco, impredecible y utilizable una sola vez para renovación.

### Errores

- `400 Bad Request`: faltan campos o el formato es inválido.
- `401 Unauthorized`: credenciales incorrectas. El mensaje no debe revelar si el usuario existe.
- `429 Too Many Requests`: se superó el límite de intentos. Debe incluir `Retry-After` cuando corresponda.
- `500 Internal Server Error`: error inesperado.

### Requisitos de seguridad

- No incluir un `client_secret` dentro de Firmador.
- No registrar contraseñas, tokens ni cuerpos de solicitudes de login.
- Aplicar las mismas políticas de contraseña, bloqueo y auditoría que utilice el sistema web.
- Limitar intentos por cuenta y origen sin revelar información que permita enumerar usuarios.

El login directo es la decisión para esta primera integración. Para una evolución futura se recomienda Authorization Code con PKCE, porque una aplicación de escritorio no puede custodiar de forma segura un secreto fijo.

## Renovar la sesión

### Solicitud

```http
POST /api/v1/sessions/refresh
Content-Type: application/json
Accept: application/json, application/problem+json
```

```json
{
  "refreshToken": "valor-opaco"
}
```

### Respuesta correcta

Estado `200 OK`:

```json
{
  "accessToken": "eyJ...",
  "tokenType": "Bearer",
  "expiresIn": 900,
  "refreshToken": "nuevo-valor-opaco"
}
```

Al emitir la respuesta, el backend debe invalidar el refresh token anterior. Firmador reemplazará ambos tokens en memoria de manera atómica.

### Errores

- `400 Bad Request`: falta el refresh token.
- `401 Unauthorized`: token inválido, expirado, revocado o ya utilizado.
- `429 Too Many Requests`: demasiados intentos.

Si la renovación falla con `401`, Firmador debe descartar la sesión y volver a solicitar credenciales.

## Cerrar la sesión

### Solicitud

```http
POST /api/v1/sessions/logout
Authorization: Bearer {accessToken}
Content-Type: application/json
```

```json
{
  "refreshToken": "valor-opaco"
}
```

### Respuesta correcta

Estado `204 No Content`.

La operación debe ser idempotente. Si la sesión ya fue revocada, el servidor puede responder igualmente `204`. Firmador descartará sus tokens locales incluso si el servidor no está disponible.

## Listar documentos pendientes

### Solicitud

```http
GET /api/v1/documents?status=pending-signature&pageNumber=1&pageSize=10
Authorization: Bearer {accessToken}
Accept: application/json, application/problem+json
```

### Parámetros

| Nombre | Tipo | Obligatorio | Regla |
| --- | --- | --- | --- |
| `status` | string | Sí | En esta versión sólo se admite `pending-signature` |
| `pageNumber` | integer | Sí | Valor mínimo 1 |
| `pageSize` | integer | Sí | Valor entre 1 y 100 |

### Respuesta correcta

Estado `200 OK`:

```json
{
  "items": [
    {
      "id": 123,
      "type": "Contrato",
      "title": "Contrato marco 2026",
      "version": "7",
      "sha256": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
      "updatedAt": "2026-09-14T18:30:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 24
}
```

`sha256` corresponde exactamente a los bytes del PDF original que el backend entregará para esa versión. El orden debe ser estable; se propone ordenar por `updatedAt` y luego por `id`.

Cuando no existan resultados, la API responderá `200` con `items` vacío y `totalCount` igual a cero.

### Errores

- `400 Bad Request`: parámetros de paginación o estado inválidos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `500 Internal Server Error`: error inesperado.

## Descargar el PDF original

### Solicitud

```http
GET /api/v1/documents/123/pdf
Authorization: Bearer {accessToken}
If-Match: "7"
Accept: application/pdf, application/problem+json
```

El valor de `If-Match` debe contener la versión recibida en el listado. Así se evita descargar silenciosamente una versión diferente de la que el usuario seleccionó.

### Respuesta correcta

Estado `200 OK` con el PDF en el cuerpo y los siguientes headers:

```http
Content-Type: application/pdf
Content-Disposition: inline; filename="documento-123.pdf"
ETag: "7"
Cache-Control: no-store
```

Después de descargar, Firmador calculará el SHA-256 y lo comparará con `sha256` del listado. Si no coincide, no mostrará ni firmará el archivo y tratará el caso como un error de integridad.

### Errores

- `401 Unauthorized`: token ausente, inválido o expirado.
- `404 Not Found`: documento inexistente o no autorizado para el usuario.
- `409 Conflict`: el documento ya no está pendiente de firma.
- `412 Precondition Failed`: la versión indicada en `If-Match` ya no es vigente.

Para impedir la enumeración de documentos, un documento inexistente y un documento ajeno deben producir la misma respuesta `404`.

## Almacenar el PDF firmado

### Solicitud

```http
POST /api/v1/documents/123/signed-pdf
Authorization: Bearer {accessToken}
Idempotency-Key: 7e4bbf44-006f-4747-82aa-4964557cdbb4
Content-Type: multipart/form-data; boundary={boundary}
Accept: application/json, application/problem+json
```

El cuerpo `multipart/form-data` contiene:

| Parte | Tipo | Descripción |
| --- | --- | --- |
| `signedFile` | archivo | PDF firmado localmente |
| `sourceDocumentVersion` | string | Versión opaca del PDF original descargado |
| `sourceDocumentSha256` | string | SHA-256 del PDF original descargado y firmado |

El PDF original no se vuelve a enviar. `signedFile` es el único archivo incluido en la solicitud.

### Respuesta correcta

La primera operación aceptada devuelve `201 Created`:

```json
{
  "documentId": 123,
  "status": "received",
  "signedFileSha256": "abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789",
  "receivedAt": "2026-09-14T18:45:00Z"
}
```

`sourceDocumentSha256` identifica el PDF original que Firmador descargó y firmó. `signedFileSha256` identifica el archivo firmado que el servidor recibió y debe ser calculado por el backend, no aceptado como dato confiable del cliente.

### Procesamiento del backend

En una única operación atómica, el backend debe:

1. Validar el token y obtener el usuario.
2. Comprobar que el usuario pueda firmar el documento.
3. Comprobar que el documento continúe pendiente.
4. Comparar `sourceDocumentVersion` con la versión vigente.
5. Comparar `sourceDocumentSha256` con el hash que conserva el servidor.
6. Aplicar los límites de tipo y tamaño del archivo.
7. Calcular `signedFileSha256` sobre los bytes recibidos.
8. Almacenar el PDF firmado y su hash.
9. Registrar `receivedAt` con la hora UTC del servidor.
10. Cambiar el estado del documento a recibido.
11. Guardar el resultado asociado con `Idempotency-Key`.

La primera versión no validará la firma PDF, la cadena del certificado, su revocación ni la relación entre certificado y usuario. El hash del original mejora la trazabilidad y evita procesar una versión obsoleta, pero no demuestra por sí solo que el archivo subido sea una firma válida de ese original.

### Idempotencia

- Firmador generará un UUID nuevo por documento antes del primer intento.
- Los reintentos del mismo envío reutilizarán la misma clave.
- Una repetición con igual clave y contenido devolverá `200 OK` y la misma representación guardada, sin crear otra copia ni repetir la transición de estado.
- La misma clave con contenido o documento diferente devolverá `409 Conflict` con el código `idempotency-conflict`.
- Una carga nueva para un documento ya recibido, con otra clave, devolverá `409 Conflict` con el código `document-already-received`.
- El backend debe conservar las claves al menos durante las 8 horas de duración máxima de la sesión.

### Errores

- `400 Bad Request`: faltan partes o tienen un formato inválido.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `404 Not Found`: documento inexistente o no autorizado.
- `409 Conflict`: documento modificado, ya recibido o conflicto de idempotencia.
- `413 Content Too Large`: el archivo supera el máximo configurado.
- `415 Unsupported Media Type`: el archivo no se admite como PDF.
- `422 Unprocessable Content`: versión o hash con formato válido pero inconsistente con el documento.

## Control de concurrencia

`version` y SHA-256 protegen aspectos diferentes:

- `version` representa el estado lógico del borrador. Puede cambiar por una modificación, reasignación, anulación o transición de estado, aunque el contenido del PDF sea idéntico.
- `sha256` identifica los bytes exactos del PDF original.

El backend debe comparar ambos datos antes de almacenar el firmado. La comprobación y el cambio de estado deben ocurrir en una transacción o mediante una actualización condicional equivalente. Si el documento cambió, la carga devuelve `409` y Firmador informa que debe buscar y descargar nuevamente el documento.

## Formato común de errores

Los errores seguirán Problem Details y usarán `application/problem+json`:

```json
{
  "type": "https://api.example.invalid/problems/document-version-conflict",
  "title": "El documento cambió",
  "status": 409,
  "detail": "La versión enviada ya no es la versión vigente.",
  "code": "document-version-conflict",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00"
}
```

`type` debe identificar una categoría estable y documentada. No debe contener datos sensibles. `detail` puede explicar el caso particular y `traceId` debe permitir localizar el evento en los registros del backend.

### Códigos de dominio iniciales

| Código | Estado HTTP | Acción esperada en Firmador |
| --- | --- | --- |
| `invalid-credentials` | 401 | Informar sin distinguir usuario o contraseña |
| `access-token-expired` | 401 | Renovar una sola vez y repetir la operación |
| `refresh-token-invalid` | 401 | Descartar sesión y volver al login |
| `document-not-found` | 404 | Informar y refrescar el listado |
| `document-not-pending` | 409 | Informar y refrescar el listado |
| `document-version-conflict` | 409 o 412 | Descargar nuevamente antes de firmar |
| `document-hash-mismatch` | 422 | No firmar ni subir; registrar el incidente |
| `document-already-received` | 409 | Refrescar el listado |
| `idempotency-conflict` | 409 | No reintentar con esa clave |
| `file-too-large` | 413 | Informar el límite permitido |
| `unsupported-file-type` | 415 | No reintentar el mismo archivo |
| `rate-limit-exceeded` | 429 | Respetar `Retry-After` |

## Reintentos y expiración

- Ante un `401` por access token expirado, Firmador renovará la sesión una sola vez y repetirá la solicitud original.
- Si la renovación falla, Firmador volverá al login.
- Las consultas y descargas pueden reintentarse ante fallas transitorias de red.
- Las cargas sólo pueden reintentarse reutilizando la misma `Idempotency-Key`.
- El cliente debe respetar `Retry-After` ante `429` o `503` cuando el servidor lo informe.
- No se reintentarán automáticamente errores de validación, autorización o concurrencia.

## Límites y tratamiento de archivos

- El tamaño máximo del PDF será configurable en el backend y deberá acordarse antes de las pruebas integradas.
- El servidor debe comprobar el tipo declarado y que el contenido tenga una estructura PDF básica antes de almacenarlo. Esta comprobación de formato no equivale a validar una firma digital.
- Los nombres de archivo enviados por el cliente no deben utilizarse directamente como rutas de almacenamiento.
- Los PDFs y tokens no deben aparecer en logs, métricas, trazas ni mensajes de error.
- El almacenamiento debe impedir acceso directo fuera de las reglas de autorización del sistema web.

## Responsabilidades del backend

- Autenticar, renovar y revocar sesiones.
- Autorizar cada operación a partir del usuario contenido en el token.
- Listar únicamente documentos pendientes accesibles para ese usuario.
- Mantener la versión y el SHA-256 del PDF original.
- Entregar exactamente los bytes asociados con la versión informada.
- Resolver de forma atómica concurrencia e idempotencia.
- Calcular y almacenar el hash del PDF firmado.
- Registrar auditoría de login, descarga, recepción, conflictos y errores sin incluir secretos ni archivos.
- Publicar el contrato definitivo mediante OpenAPI 3.1.

## Responsabilidades de Firmador

- Capturar las credenciales sin persistirlas ni registrarlas.
- Mantener los tokens solamente en memoria.
- Enviar el access token mediante el header `Authorization`.
- Conservar `version` y `sha256` junto con cada documento mostrado.
- Verificar localmente el SHA-256 después de descargar el PDF.
- No mostrar ni firmar un archivo cuyo hash no coincida.
- Firmar el PDF localmente sin exponer la clave privada.
- Enviar sólo `signedFile`, `sourceDocumentVersion` y `sourceDocumentSha256`.
- Reutilizar la clave de idempotencia en los reintentos de una misma carga.
- Mostrar resultados independientes cuando se firmen varios documentos.
- Descartar los tokens y solicitar logout al cerrar la aplicación.

## Auditoría mínima

El backend debe registrar, como mínimo:

- Identificador del evento y `traceId`.
- Usuario autenticado.
- Documento afectado.
- Fecha y hora UTC del servidor.
- Tipo y resultado de la operación.
- Versión y hashes asociados cuando correspondan.
- Clave de idempotencia para una carga, sin registrar el archivo.

Los registros no deben contener contraseñas, access tokens, refresh tokens ni los bytes de los PDFs.

## Pruebas de aceptación

### Sesión

- Login válido devuelve usuario y tokens con la estructura acordada.
- Credenciales incorrectas devuelven un mensaje genérico.
- El backend limita intentos y devuelve `429` cuando corresponde.
- La renovación invalida el refresh token anterior.
- Reutilizar un refresh token rotado devuelve `401`.
- Logout puede repetirse sin producir un error funcional.
- Cerrar y abrir Firmador requiere autenticarse nuevamente.

### Listado

- El backend devuelve sólo documentos pendientes del usuario autenticado.
- Una búsqueda sin resultados devuelve `items` vacío y `totalCount` cero.
- La última página puede contener menos elementos que `pageSize`.
- Se rechazan páginas menores que 1 y tamaños fuera del rango permitido.
- El orden se mantiene estable entre solicitudes equivalentes.

### Descarga

- El PDF descargado coincide con el SHA-256 anunciado.
- Una versión obsoleta en `If-Match` devuelve `412`.
- Un documento que dejó de estar pendiente devuelve `409`.
- Un documento inexistente o ajeno devuelve `404`.
- La respuesta impide almacenamiento en caché.

### Carga

- La solicitud contiene un único archivo y no incluye el PDF original.
- Una carga válida almacena el firmado, calcula su hash y devuelve `201` con estado `received`.
- La misma carga con la misma clave devuelve el resultado anterior sin duplicados.
- La misma clave con contenido diferente devuelve `409`.
- Una versión obsoleta o un hash original distinto impiden almacenar el archivo.
- Una carga nueva sobre un documento ya recibido devuelve `409`.
- Se rechazan archivos superiores al máximo y contenidos que no sean PDF.
- Un usuario no puede cargar un archivo para un documento ajeno.
- Si el access token expira durante el flujo, Firmador renueva una vez y reintenta con la misma clave.
- Una selección múltiple produce resultados independientes por documento.

## Riesgos y evolución prevista

La recepción sin validación criptográfica permite que un cliente modificado envíe un PDF incorrecto junto con identificadores y hashes válidos. El control de versión y el hash del original no eliminan este riesgo. Antes de que el sistema represente el estado como firma válida, el backend deberá validar la firma PDF, la integridad de las revisiones, la cadena de confianza, la revocación y la identidad del firmante.

La captura directa de credenciales dentro de Firmador aumenta la responsabilidad de seguridad del cliente. Una evolución futura debería reemplazarla por OpenID Connect u OAuth 2.0 Authorization Code con PKCE mediante el navegador del sistema.

Si el negocio exige acreditar de manera confiable el momento de firma, deberá incorporarse una autoridad de sellado de tiempo TSA. `receivedAt` sólo prueba cuándo el backend recibió el archivo.

## Definiciones pendientes antes de pruebas integradas

El equipo web y el responsable funcional deberán confirmar:

- URL de cada ambiente y mecanismo para configurar Firmador.
- Tamaño máximo permitido para cada PDF.
- Algoritmo y política de firma aceptados en una futura validación server-side.
- Política de conservación de PDFs originales, firmados, hashes y eventos de auditoría.
- Catálogo definitivo de tipos y estados de documento.
- Datos de prueba y usuarios habilitados para el ambiente de integración.

## Referencias

- RFC 8252, OAuth 2.0 for Native Apps: https://www.rfc-editor.org/info/rfc8252/
- RFC 9700, Best Current Practice for OAuth 2.0 Security: https://www.rfc-editor.org/rfc/rfc9700.html
- RFC 9457, Problem Details for HTTP APIs: https://www.rfc-editor.org/rfc/rfc9457.html


# Integración API Firmador — v1 (Borradores Protocolizables)

**Rama:** `feature/firmador-api-v1` (sobre `master-v2`)  
**Fecha:** 2026-09-16  
**Spec base:** `/Users/fefo/Downloads/API-Firma Digital/Especificacion-Integracion-API-Firmador.md:1-533` (v1.0, 2026-09-14)  
**Alcance pedido:** sólo documentos protocolizables donde el usuario es *siguiente firmante*; doble verificación de hashes (original + firmado) y sobrescritura del PDF en `storage`; respetar flujo de borradores y transición automática a `ParaProtocolizar`.

---

## 1) Resumen ejecutivo

Se implementó `POST /api/v1/sessions`, `POST /api/v1/sessions/refresh`, `POST /api/v1/sessions/logout` (Sanctum con refresh rotatorio) y el trío de documentos `GET /api/v1/documents`, `GET /api/v1/documents/{id}/pdf`, `POST /api/v1/documents/{id}/signed-pdf` cumpliendo el contrato ProblemDetails (`application/problem+json`) y el control de concurrencia/idempotencia de la spec. El listado filtra `Borrador::paraFirmar($user)` + `tipo.protocolizable=true`; la descarga y la carga exigen `verify_hash()` y `next_firmante==user`. La carga valida **ambos** SHA-256 (original contra bytes en `Storage::disk('local')` y firmado contra bytes subidos), sobrescribe el mismo `path_pdf` y ejecuta la lógica de firma del flujo Livewire `aprobar_firmado` (`resources/views/livewire/borrador-pdf-viewer.blade.php:255-278`): si quedan firmantes permanece en `ParaFirmar`, si era el último y es protocolizable transita a `ParaProtocolizar` (el estampado de nombre queda para el proceso posterior automático).

---

## 2) Decisiones clave (respuestas del usuario)

1. **Dependencia protocolizadora:** sólo para quien protocoliza, **no** para firmar. No se filtra por `tipo.dependencia_protocolizadora_id` en el listado de firma.
2. **Auth:** `laravel/sanctum ^4.3.3` (`composer.json:62`, `app/Models/User.php:32` `HasApiTokens`). Access 900s, refresh 28800s, rotación + invalidación de familia al reuso.
3. **Versión opaca:** `hash` (`Borrador.hash char(64)` `database/migrations/2024_02_22_132245...php:36`). `version` y `sha256` son el mismo valor.
4. **Filtro listado:** sólo `protocolizable==true` para el usuario (siguiente firmante). `ParaProtocolizar` se sigue haciendo vía UI (`BorradorController.php:297`).
5. **Post-firma:** si `debe_protocolizar()` y era último firmante → `ParaProtocolizar`; estampado de nombre posterior automático (no en este API).

---

## 3) Contratos

### 3.1 Transporte y errores

* Prefijo `/api/v1`, `application/json; charset=utf-8`, errores `application/problem+json` (`Especificacion-Integracion-API-Firmador.md:379-410`) con `type`, `title`, `status`, `detail`, `code`, `traceId`.
* Códigos de dominio: `invalid-credentials` 401, `access-token-expired` 401, `refresh-token-invalid` 401, `document-not-found` 404, `document-not-pending` 409, `document-version-conflict` 422, `document-hash-mismatch` 422, `signed-hash-mismatch` 422, `document-already-received` 409, `idempotency-conflict` 409, `file-too-large` 413, `unsupported-file-type` 415, `rate-limit-exceeded` 429.

### 3.2 Sesión

```
POST /api/v1/sessions { username, password } -> 200 { accessToken, tokenType: Bearer, expiresIn: 900, refreshToken, user:{id, username, displayName} }
POST /api/v1/sessions/refresh { refreshToken } -> 200 { accessToken, tokenType, expiresIn, refreshToken } (rota, invalida anterior)
POST /api/v1/sessions/logout { refreshToken } Authorization: Bearer {access} -> 204 (idempotente)
```

Login soporta `email` y `cuil` (11 dígitos) y respeta `config/auth.php:16` `use_ldap` (guard `web`). Throttling por `username|ip` (`config/firmador.php:7`).

### 3.3 Documentos

```
GET /api/v1/documents?status=pending-signature&pageNumber=1&pageSize=10 Authorization: Bearer -> 200 { items:[{id, type, title, version:hash, sha256:hash, updatedAt:ISO8601}], pageNumber, pageSize, totalCount }

GET /api/v1/documents/{id}/pdf Authorization: Bearer If-Match: "hash" -> 200 application/pdf + ETag: "hash" Cache-Control: no-store
  412 si If-Match != hash, 409 si estado != firmar, 404 si ajeno/inexistente, 422 si verify_hash() falla

POST /api/v1/documents/{id}/signed-pdf Authorization: Bearer Idempotency-Key: uuid Content-Type: multipart/form-data
  signedFile (pdf), sourceDocumentVersion (hash), sourceDocumentSha256 (64hex), signedFileSha256 (64hex)
  -> 201 { documentId, status: received, signedFileSha256: hash(firmado), receivedAt: ISO8601 }
  Reintento misma key+contenido -> 200 mismo body; misma key contenido distinto -> 409 idempotency-conflict
```

---

## 4) Modelo de datos y storage

* **`borradores.hash` / `path_pdf`** (`app/Models/Borrador.php:418-444` `generar_hash`/`verify_hash` = `hash('sha256', Storage::disk('local')->get(path_pdf))`, `hash_equals`). `converts` vía `BorradorPdfService.php:39` `borradores/Y/m/Tipo__id-ts.pdf` en `Storage::disk('local')` (`config/filesystems.php:46` root `storage/app`).
* **Filtro siguiente firmante** `app/Models/Borrador.php:293-312` `scopeParaFirmar`: `JOIN firmantes_borrador + MIN(orden) whereNull firmado_at + estado=ParaFirmar + fb.user_id = auth`.
* **Protocolizable** `app/Models/Borrador.php:482` `debe_protocolizar() => tipo.protocolizable` (`app/Models/TipoArchivo.php:12`, migración `2025_12_18_140203...php`).
* **Estados** `app/States/Borradores/EstadosBorrador.php:10-35` `ParaFirmar -> ParaProtocolizar | Finalizado`; `FirmarBorradorViaApiService.php:37` replica `borrador-pdf-viewer.blade.php:255-278` (marca `firmado_at`, recalcula `hash`, transition o `AvisarProximoFirmante`).
* **Sobrescritura** `SignedPdfController.php:122` + `FirmarBorradorViaApiService.php:14` `Storage::disk('local')->put(path_pdf, signedBytes)` mismo path (no cambia `path_pdf`).

---

## 5) Archivos creados / modificados

### 5.1 Configuración

* `config/firmador.php:1` — `access_ttl` (900), `refresh_ttl` (28800), `max_pdf_mb` (20), `token_name`, `token_abilities ['firmador:access']`, `login_throttle_max`, `idempotency_retention`.

### 5.2 Migraciones

* `database/migrations/2019_12_14_000001_create_personal_access_tokens_table.php:10` — añade `expires_at nullable` a `personal_access_tokens` (Sanctum sin publish de `config/sanctum.php`).
* `database/migrations/2026_09_16_145320_create_firmador_refresh_tokens_table.php:6` — `firmador_refresh_tokens (id, user_id FK, token_hash char64 unique sha256, expires_at, revoked_at, replaced_by_id FK, timestamps, índices)`.
* `database/migrations/2026_09_16_145321_create_firmador_idempotency_keys_table.php:6` — `firmador_idempotency_keys (id uuid PK = Idempotency-Key, borrador_id FK, user_id FK, request_hash char64, response_status, response_body json, signed_file_sha256, expires_at, timestamps)`.

### 5.3 Modelos

* `app/Models/FirmadorRefreshToken.php:1` — `isExpired/isRevoked/isValid`, relaciones `user/replacedBy`.
* `app/Models/FirmadorIdempotencyKey.php:1` — PK uuid, `isExpired`.

### 5.4 Middleware

* `app/Http/Middleware/EnsureFirmadorTokenIsValid.php:1` — tras `auth:sanctum`, rechaza `TransientToken`, verifica `expires_at` (si `past` → `delete()` + `401 access-token-expired`), verifica `can('firmador:access')`.

### 5.5 Servicios

* `app/Services/Firmador/FirmadorSessionService.php:1` — `createSession()` (Sanctum `createToken` + `expires_at`, refresh opaco `Str::random(64)` hasheado), `refreshSession()` (transacción `lockForUpdate`, rota, marca `revoked_at/replaced_by_id`), `revoke*`.
* `app/Services/Firmador/FirmarBorradorViaApiService.php:1` — `firmar(Borrador, signedBytes, signedSha)` sobrescribe `Storage::put`, actualiza `hash`, `firmado_at` con `lockForUpdate`, refresca y decide `ParaProtocolizar` vs `ParaFirmar` + `ActivityBorrador` + `AvisarProximoFirmante`/`ParaProtocolizar` dispatch.

### 5.6 Requests (validación ProblemDetails se apoya en `StoreSignedPdfRequest` etc.)

* `app/Http/Requests/Api/V1/LoginRequest.php:1` — `username`, `password` required.
* `app/Http/Requests/Api/V1/RefreshRequest.php:1` — `refreshToken` required.
* `app/Http/Requests/Api/V1/DocumentListRequest.php:1` — `status in:pending-signature`, `pageNumber min1`, `pageSize 1-100`.
* `app/Http/Requests/Api/V1/StoreSignedPdfRequest.php:1` — `signedFile` `mimetypes:application/pdf` `max: max_pdf_mb*1024`, `sourceDocumentVersion`, `sourceDocumentSha256` / `signedFileSha256` regex `^[a-f0-9]{64}$`.

### 5.7 Resources

* `app/Http/Resources/Api/V1/DocumentResource.php:1` — `id, type=tipo.nombre, title=titulo, version=hash, sha256=hash, updatedAt=ISO8601`.

### 5.8 Controllers

* `app/Http/Controllers/Api/V1/SessionController.php:1` — `store` (throttle `RateLimiter::tooManyAttempts`, `attemptLogin` por `email/cuil` sin crear sesión web, evita `TransientToken`; `Hash::check` manual; `createSession`), `refresh`, `destroy` (revoca access+refresh). Helper `attemptLogin` respeta `config('auth.use_ldap')` vía `Auth::guard('web')->attempt`.
* `app/Http/Controllers/Api/V1/DocumentController.php:1` — `index` filtra `paraFirmar + whereHas tipo protocolizable`.
* `app/Http/Controllers/Api/V1/DocumentPdfController.php:1` — `show` con `canAccessForSigning` (`debe_protocolizar + next_firmante==user + verify_hash`), `ETag`, `Cache-Control: no-store`, `404` indistinguible.
* `app/Http/Controllers/Api/V1/SignedPdfController.php:1` — `store` valida `Idempotency-Key uuid`, lee `signedFile` y calcula `signedSha` temprano, verifica idempotencia **antes** de validar estado (permite reintento `200` tras transición), re-valida `canAccessForSigning`/`estado==firmar`/`version==hash`/`hash==sourceSha` + `Storage::get` vs `sourceSha`, `hash_equals(signed)`, `%PDF-` header, tamaño, transacción `lockForUpdate` + `VersionConflictException`/`NotPendingException`, delega a `FirmarBorradorViaApiService`, persiste `FirmadorIdempotencyKey` (`201` primera vez, `200` en reintento).

### 5.9 Excepciones

* `app/Exceptions/Firmador/VersionConflictException.php:1`
* `app/Exceptions/Firmador/NotPendingException.php:1`

### 5.10 Rutas

* `routes/api.php:60` — grupo `prefix v1`: `POST /sessions`, `POST /sessions/refresh` públicas; `middleware [auth:sanctum, EnsureFirmadorTokenIsValid]` → `POST /sessions/logout`, `GET /documents`, `GET /documents/{id}/pdf`, `POST /documents/{id}/signed-pdf`. `php artisan route:list --path=api/v1` 6 rutas.

### 5.11 Tests

* `tests/Feature/FirmadorApiTest.php:1` — 8 casos Pest con `RefreshDatabase` + `Storage::fake('local')`: login ok/inválido, listado sólo protocolizables siguiente firmante, descarga `If-Match` + ETag, upload doble hash + sobrescritura + `protocolizar`, `422` hash original, idempotencia `201→200`, multi-firmantes `firmar→protocolizar` (con `forgetGuards()` por caché de guard en tests).

---

## 6) Flujo detallado de firma (secuencia)

1. `POST /sessions` → Sanctum `personal_access_tokens` + `firmador_refresh_tokens`.
2. `GET /documents?status=pending-signature` → `scopeParaFirmar` + `tipo.protocolizable`.
3. `GET /documents/{id}/pdf` con `If-Match: "hash"` → bytes exactos + `ETag`.
4. Firmador calcula SHA-256 del descargado, firma local (clave permanece en Windows), calcula SHA-256 del firmado.
5. `POST /documents/{id}/signed-pdf` con `Idempotency-Key` + `multipart (signedFile, sourceVersion, sourceSha, signedSha)`:
   * verifica `request_hash` vs idempotencia (early return `200`),
   * autoriza `firmar` (`BorradorPolicy.php:258` equivalente),
   * compara `sourceVersion==hash` y `sourceSha==hash==Storage::get`,
   * compara `signedSha==hash(signedBytes)` y `%PDF-`,
   * `put(path_pdf, signedBytes)` + `borrador.hash = signedSha` + `firmante.firmado_at=now()` en transacción,
   * si `next_firmante==null` y `debe_protocolizar()` → `ParaProtocolizar` (`app/States/Borradores/ParaProtocolizar.php`), sino permanece `ParaFirmar`.

---

## 7) Seguridad y auditoría

* No se loguean `password`, tokens ni bytes PDF (`Especificacion-Integracion-API-Firmador.md:424`).
* `ActivityBorrador` (`app/Observers/FirmanteObserver.php:36`, `FirmarBorradorViaApiService.php:50`) registra `firmado por X (Firmador API)` y `enviado a protocolizar`.
* `traceId` (`Str::uuid()`) en cada ProblemDetails.
* `personal_access_tokens.expires_at` se chequea y se borra expirado; `firmador_refresh_tokens` expira 8h, rotación invalida familia al reuso.

---

## 8) Verificación

```bash
php artisan migrate --force   # crea firmador_refresh_tokens + firmador_idempotency_keys
php artisan route:list --path=api/v1
vendor/bin/pest tests/Feature/FirmadorApiTest.php --compact  # 8 passed
```

---

## 9) Pendientes / riesgos

* Spec prevé `400 Bad Request` para validación; Laravel FormRequest devuelve `422`. Mantener `422` o mapear a `400` vía handler si integración lo exige.
* Firmas PAdES incrementales: sobrescribir preserva firmas previas si Firmador incrusta incremental; documentar y validar con PDFs reales.
* LDAP en prod (`config/auth.php:152`) usa `Auth::guard('web')->attempt` en `SessionController.php:100`.
* OpenAPI 3.1 aún no publicado (`Especificacion-Integracion-API-Firmador.md:438`); generar desde `routes/api.php:60`.

---

## 10) Referencias

* `app/Models/Borrador.php:293-329,418-482`, `app/Policies/BorradorPolicy.php:258-280`, `resources/views/livewire/borrador-pdf-viewer.blade.php:101-278`, `config/filesystems.php:46`, `docs/integrations/02-pdf-signing.md`.

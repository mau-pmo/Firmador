# Configuración del cliente Firmador

La primera versión del backend está descrita en `04-firmador-api.md`. Antes de ejecutar el cliente, configurar `ApiBaseUrl` en `Firmador.Cliente/appsettings.json` (archivo que se copia junto al ejecutable):

```json
{
  "ApiBaseUrl": "https://servidor-de-prueba/",
  "TsaUrl": "http://5.0.96.197:8080/signserver/tsa?workerName=TimeStampSigner"
}
```

Para una API local se admite `http://localhost:puerto/`. Para el servidor de prueba `http://daf-expediente-digital.test/`, configurar también `"AllowInsecureHttp": true`. Esta excepción envía credenciales y tokens por HTTP, por lo que debe quitarse al pasar a HTTPS o producción. El valor de la URL puede sobrescribirse con la variable de entorno `FIRMADOR_API_BASE_URL`, útil para cambiar entre ambientes sin modificar el archivo. La URL debe apuntar a la raíz del servidor, sin `/api/v1`, porque el cliente agrega ese prefijo.

`TsaUrl` indica la autoridad de sellado de tiempo utilizada al firmar los PDF. Puede sobrescribirse con la variable de entorno `TSA_URL`. La aplicación requiere una URL absoluta HTTP o HTTPS y no completa la firma si la TSA no puede emitir el sello de tiempo.

Al iniciar se solicitan usuario (email o CUIL) y contraseña. Los tokens permanecen en memoria y se renuevan cuando vencen. El cliente intenta cerrar la sesión remota al salir.

La vista previa descarga el PDF a `%LOCALAPPDATA%/Firmador/temporales`; la firma vuelve a descargar y verifica SHA-256 antes de firmar. El PDF firmado también queda en `%LOCALAPPDATA%/Firmador/docs/firmados` y se envía a la API. Si una carga falla, reintentar desde la misma ejecución reutiliza el mismo archivo y la misma clave de idempotencia.

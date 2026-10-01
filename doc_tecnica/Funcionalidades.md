# Firmador

Aplicativo cliente para instalar en equipos con sistema operativo Windows que permite realizar la firma digital, con token USB, de un documento PDF.
El cliente no tiene la información, sino que interactúa con un sistema web (vía APIs) para obtener la información y luego enviarle el resultado de lo realizado localmente. 

## Flujo general

### 1. Login de usuario

La aplicación realiza el login del usuario, con usuario y contraseña, contra un API que devuelve si está autorizado o no.

- En caso de no estar autorizado, informa al usuario.
- En caso de estar autorizado, permite ingresar al `MainForm`.
- Si el refresh token vence o deja de ser válido, descarta la sesión y vuelve a mostrar el login para solicitar nuevamente las credenciales.

### 2. Buscar documentos a firmar

En `MainForm` se encuentra el botón `Buscar`. Al presionar ese botón, la aplicación realiza una consulta a un API que retorna una lista de documentos a firmar, también llamados borradores.

#### Datos obtenidos

- Id borrador
- Tipo de documento
- Título borrador
- Hash

#### Visualización en la tabla

Los datos se muestran en una tabla, ocultando el id y el hash. La tabla queda compuesta así:

- Checkbox de selección múltiple, visible
- Id, oculto
- Tipo de documento, visible
- Título, visible
- Botón `Ver documento`, visible
- Botón `Ver participantes`, visible

#### Ver participantes

El botón `Ver participantes` de cada fila abre una ventana modal centrada sobre Firmador con el título del documento y las secciones Creador, Editor, Revisores y Firmantes. No requiere seleccionar el documento con el checkbox.

La ventana consulta `GET /api/v1/documents/{id}/participants` en cada apertura, muestra un indicador de carga y permite cerrar o reintentar ante errores. Cerrar durante la carga cancela la consulta; si la sesión vence, vuelve al login.

Los revisores y firmantes se muestran por `orden`, con su estado y, cuando corresponda, la fecha de revisión o firma en formato `d/M/yyyy`, conservando el huso horario recibido. Los pendientes se muestran como `pendiente`; las revisiones y firmas sin fecha indican `fecha no informada`. Otros estados se muestran tal como llegan de la API.

Cuando falta el creador o editor se muestra `No informado`; las listas vacías indican `Sin revisores` o `Sin firmantes`. La ventana admite nombres largos y desplazamiento para listas extensas.

### 3. Firmar

La aplicación valida que haya al menos un documento seleccionado en la tabla. Luego toma ese documento, lo firma digitalmente con token USB y, por ahora, lo guarda en una ruta del disco local.

#### Reglas de firma

- Debe pedir al usuario que seleccione el certificado con el cual se va a firmar.
- El certificado debe provenir de un token USB conectado.
- Una vez seleccionado, se aplica la firma.
- La firma incorpora un sello de tiempo emitido por la TSA configurada.
- En `MainForm` se debe mostrar el certificado seleccionado.
- No se debe volver a pedir el certificado hasta que se cierre el programa.

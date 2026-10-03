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
- Columna `Documento` con acción `Ver PDF` en cada fila, visible
- Columna `Intervinientes` con acción `Ver` en cada fila, visible

#### Presentación de la pantalla principal

- La cabecera identifica la aplicación y mantiene la acción `Salir`.
- La cabecera muestra únicamente `Firmador EDA`. La franja compacta del certificado muestra su etiqueta y el titular en la primera línea, con el vencimiento debajo, y separadores horizontales arriba y abajo.
- El título de documentos y las acciones de selección tienen alturas compactas para priorizar la tabla, que se delimita con un borde gris fino tanto con documentos como en los estados vacíos.
- La franja de certificado muestra el titular, el vencimiento y el estado de selección. Permite seleccionar o cambiar el certificado; no indica que el token esté conectado.
- La tabla usa filas celestes únicamente para documentos marcados con el checkbox. El foco de teclado no equivale a seleccionar un documento para firmar.
- Las acciones `Marcar todos` y `Limpiar selección` afectan la página actual. El contador y el botón de firma se actualizan con la selección.
- La barra inferior muestra el resumen de selección y el certificado. El botón indica la cantidad de documentos a firmar y permanece deshabilitado cuando no hay documentos seleccionados.
- La pantalla distingue la búsqueda inicial, la carga y una búsqueda sin resultados. La paginación muestra el rango de documentos y la página actual.
- Durante la búsqueda o firma se bloquean las acciones incompatibles. Durante la firma se muestra el número de documento en proceso.
- La ventana inicial se ajusta al área útil del monitor para mantener visible la barra de firma. Los anchos de las columnas y el área del checkbox se adaptan al escalado de Windows.

#### Ver participantes

El botón `ver` de la columna `Intervinientes` de cada fila abre una ventana modal centrada sobre Firmador con el título del documento y las secciones Creador, Editor, Revisores y Firmantes. No requiere seleccionar el documento con el checkbox.

La ventana consulta `GET /api/v1/documents/{id}/participants` en cada apertura, muestra un indicador de carga y permite cerrar o reintentar ante errores. Cerrar durante la carga cancela la consulta; si la sesión vence, vuelve al login.

Los revisores y firmantes se muestran por `orden`, con su estado y, cuando corresponda, la fecha de revisión o firma en formato `d/M/yyyy`, conservando el huso horario recibido. Los pendientes se muestran como `pendiente`; las revisiones y firmas sin fecha indican `fecha no informada`. Otros estados se muestran tal como llegan de la API.

Cuando falta el creador o editor se muestra `No informado`; las listas vacías indican `Sin revisores` o `Sin firmantes`. La ventana admite nombres largos y desplazamiento para listas extensas.

Los firmantes pueden ser individuales o grupos. Para grupos se usa el campo `nombre`, precedido por el texto fijo `Grupo: `: si el grupo está pendiente se muestra `Grupo: nombre — pendiente`; si está firmado se muestra `Grupo: nombre — firmado por: persona1, persona2`, incluyendo únicamente miembros con estado `signed`, en el orden recibido y sin fechas individuales. El estado del grupo es el que determina su presentación: puede estar firmado aunque algunos miembros sigan pendientes, y puede estar pendiente aunque algún miembro ya haya firmado. Otros estados se muestran literalmente. Si un grupo firmado no informa miembros firmados, se muestra `Grupo: nombre — firmado — firmantes no informados`; nombres ausentes se muestran como `No informado`.

### 3. Firmar

La aplicación valida que haya al menos un documento seleccionado en la tabla. Luego toma ese documento, lo firma digitalmente con token USB y, por ahora, lo guarda en una ruta del disco local.

#### Reglas de firma

- Debe pedir al usuario que seleccione el certificado con el cual se va a firmar.
- El certificado debe provenir de un token USB conectado.
- Una vez seleccionado, se aplica la firma.
- La firma incorpora un sello de tiempo emitido por la TSA configurada.
- Si no hay acceso a la TSA, la firma se detiene e informa `No hay acceso al servidor de sello de tiempo (TSA)`. Si falla la conexión con la API, informa `No hay acceso al servidor EDA`.
- En `MainForm` se debe mostrar el certificado seleccionado.
- No se debe volver a pedir el certificado hasta que se cierre el programa.

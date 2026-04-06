# Firmador

Aplicativo cliente para instalar en equipos con sistema operativo Windows que permite realizar la firma digital, con token USB, de un documento PDF.

## Flujo general

### 1. Login de usuario

La aplicación realiza el login del usuario, con usuario y contraseña, contra un API que devuelve si está autorizado o no.

- En caso de no estar autorizado, informa al usuario.
- En caso de estar autorizado, permite ingresar al `MainForm`.

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

### 3. Firmar

La aplicación valida que haya al menos un documento seleccionado en la tabla. Luego toma ese documento, lo firma digitalmente con token USB y, por ahora, lo guarda en una ruta del disco local.

#### Reglas de firma

- Debe pedir al usuario que seleccione el certificado con el cual se va a firmar.
- El certificado debe provenir de un token USB conectado.
- Una vez seleccionado, se aplica la firma.
- En `MainForm` se debe mostrar el certificado seleccionado.
- No se debe volver a pedir el certificado hasta que se cierre el programa.

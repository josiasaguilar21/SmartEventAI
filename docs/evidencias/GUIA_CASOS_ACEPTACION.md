# Guía de casos de aceptación CA-01 a CA-10

Procedimiento paso a paso para reproducir y evidenciar cada caso. Cada apartado indica **qué
hacer**, **qué debe ocurrir** y **qué capturar**.

Guardar las capturas en esta misma carpeta con el nombre indicado (`CA-01.png`, `CA-02.png`…).

---

## Preparación

```bash
# 1. Base de datos desde cero
sqlcmd -S ".\SQLEXPRESS" -E -C -b -i "database\00_SmartEventAI.sql" -W

# 2. Cadena de conexión (reabrir la terminal después)
setx SMARTEVENT_CONNECTION "Server=.\SQLEXPRESS;Database=SmartEventAI;Integrated Security=True;TrustServerCertificate=True"

# 3. Aplicación
dotnet run --project src\SmartEvent.UI
```

Entrar con `admin` / `Admin123*`.

> **Atajo para CA-01 a CA-06:** los dos bancos de pruebas los demuestran automáticamente.
> Sirven como evidencia adicional, pero **no sustituyen** las capturas de la aplicación:
> el enunciado pide evidencia de ejecución del sistema.
>
> ```bash
> sqlcmd -S ".\SQLEXPRESS" -E -C -i "database\99_pruebas_CA.sql" -W
> dotnet run --project tests\SmartEvent.PruebasIntegracion
> ```

---

## CA-01 · Guardar una reserva válida con tres detalles

**Qué hacer**

1. Menú **Reservas → Nueva reserva** (`Ctrl+N`).
2. Cliente: *Corporacion Andina S.A.* · Salón: *Salon Diamante*.
3. Fecha: dentro de dos meses · Horario: 09:00 a 13:00 · Invitados: 120.
4. Agregar **tres** recursos, por ejemplo:
   - Proyector Full HD — cantidad 2
   - Microfono inalambrico — cantidad 4
   - Servicio de coffee break — cantidad 120
5. Observar cómo el bloque **Totales** se actualiza al escribir.
6. **Guardar reserva**.
7. Ir a **Reservas → Consultar reservas** (`Ctrl+B`), buscar y **doble clic** en la reserva.

**Qué debe ocurrir**

- Se asigna un código `RSV-AAAA-NNNNNN` y el estado queda en **BORRADOR**.
- Al reabrirla aparecen **exactamente los tres detalles** con sus cantidades y precios.
- Los totales mostrados son los **recalculados por SQL Server**:
  `Subtotal = tarifa base del salón + Σ subtotales de línea`, `Impuesto = 15 %`.

**Capturar:**

- `CA-01-a.png` — la reserva abierta con su código, las **tres líneas** de detalle y los totales.
- `CA-01-b.png` — la **pantalla de consulta** mostrando esa reserva con la columna **Detalles = 3**
  y su total.

> Las dos capturas deben ser distintas. Al guardar, la aplicación **recarga la reserva desde la
> base** para mostrar los importes definitivos calculados por SQL Server, así que la pantalla de
> edición después de guardar ya es la reserva releída. Lo que demuestra el "consultar" del
> enunciado es la grilla de `sp_Reserva_Consultar`, no volver a abrir la misma ficha.

---

## CA-02 · Error en un detalle → rollback completo

La interfaz impide construir un detalle inválido, así que este caso se fuerza desde SQL, que es
donde la atomicidad debe garantizarse.

**Qué hacer**

1. Anotar el número de reservas actual:
   ```sql
   SELECT COUNT(*) FROM evt.Reserva;
   ```
2. Ejecutar `database/99_pruebas_CA.sql` y observar la sección **CA-02**, que intenta guardar
   una reserva cuya tercera línea apunta a un recurso inexistente (`IdRecurso = 999999`).
3. Volver a contar las reservas.

**Qué debe ocurrir**

- El procedimiento rechaza la operación con un error de negocio (código `51300`).
- El recuento de reservas y de detalles es **idéntico** antes y después.
- **No queda ninguna cabecera huérfana.**

**Capturar:** `CA-02.png` con la salida del banco mostrando el rechazo y la comprobación
*"Sin cabeceras huerfanas ni detalles parciales tras el rollback"*.

---

## CA-03 · Cruce de franja horaria rechazado

**Qué hacer**

1. Con la reserva de CA-01 ya guardada (Salon Diamante, 09:00–13:00), crear otra reserva:
   - **Mismo salón**, **misma fecha**, horario **12:00 a 16:00** (se solapa una hora).
2. Pulsar **Validar disponibilidad**.
3. Pulsar **Guardar reserva** para comprobar que también se rechaza desde el servidor.

**Qué debe ocurrir**

- La validación informa del conflicto indicando el **código de la reserva que ocupa el salón** y
  su horario.
- El guardado se rechaza con el mismo motivo.
- **Comprobación adicional recomendada:** intentar **13:00 a 17:00** (franja adyacente, fin =
  inicio). Debe **aceptarse**: no hay solapamiento.

**Capturar:** `CA-03-a.png` (mensaje de conflicto) y `CA-03-b.png` (franja adyacente aceptada).

---

## CA-04 · Editar un BORRADOR sin autoconflicto

**Qué hacer**

1. Abrir la reserva de CA-01 (en estado BORRADOR).
2. **Sin cambiar el horario**, modificar el número de invitados o la observación.
3. **Validar disponibilidad** y después **Guardar reserva**.

**Qué debe ocurrir**

- La validación responde *"Salón y recursos disponibles…"*.
- El guardado funciona con normalidad.
- La reserva **no se detecta a sí misma** como conflicto, porque el procedimiento excluye su
  propio `IdReserva`.

**Capturar:** `CA-04.png` con la reserva editada y guardada correctamente.

---

## CA-05 · Rechazo desde SQL por capacidad y por stock

**Parte A — capacidad del salón**

1. Nueva reserva en **Sala Ejecutiva** (capacidad 20).
2. Invitados: **500**. El campo se marca en rojo al escribirlo.
3. **Guardar reserva**.

**Parte B — stock concurrente**

El control de cantidad se acota al inventario del recurso, así que no se puede escribir 500 en
el panel de agregar. Y eso está bien: lo que hay que demostrar es el **stock concurrente**, que
la interfaz no puede conocer porque depende de qué otras reservas ocupan esa franja horaria.

1. **Reserva A** — Salón `Salon Diamante`, una fecha futura, `09:00`–`13:00`, 50 invitados,
   `Silla plegable acolchada` × **250**. Se guarda sin problema (250 de 400).
2. **Reserva B** — Salón `Terraza Jardin` (**otro salón**, para que el rechazo no sea por cruce),
   **la misma fecha**, `10:00`–`14:00` (franja solapada), 50 invitados,
   `Silla plegable acolchada` × **200**.
3. **Guardar reserva**.

Cada cantidad por separado cabe de sobra en el inventario; juntas no: 250 + 200 = 450 sobre 400.

**Parte C — descuento no autorizado por rol**

1. Cerrar sesión y entrar como `coordinador` / `Coord123*`.
2. En una reserva, intentar un descuento de línea del **15 %**.

**Qué debe ocurrir**

- A: rechazo indicando invitados solicitados y capacidad del salón.
- B: *"Stock insuficiente de Silla plegable acolchada: solicita 200, comprometido 250,
  disponible 150 de 400."* El mensaje desglosa lo pedido, lo ya comprometido y lo disponible.
- C: el control **no deja escribir más del 10 %**, y si se fuerza desde la base, el
  procedimiento lo rechaza comprobando el rol del usuario.

**Capturar:** `CA-05-a.png` (capacidad), `CA-05-b.png` (stock), `CA-05-c.png` (descuento por rol).

---

## CA-06 · Confirmación con correo y auditoría

Requiere **SMTP configurado** (ver [configuración de integraciones](../configuracion-integraciones.md)).

**Qué hacer**

1. Abrir una reserva en BORRADOR con detalles.
2. Pulsar **Analizar reserva con IA** (o preparar la justificación de contingencia).
3. Pulsar **Confirmar reserva** y aceptar.
4. Ir a **Auditoría → Correos y análisis de IA** (`Ctrl+U`).
5. Comprobar el buzón de pruebas (Mailtrap).

**Qué debe ocurrir**

- El estado pasa a **CONFIRMADA** una sola vez.
- Se envía el correo HTML con código, cliente, salón, horario, tabla de recursos y totales.
- En la auditoría aparece un registro con estado **ENVIADO**.
- Al intentar confirmar de nuevo, se rechaza: *"La reserva ya se encuentra en el estado
  solicitado"*.

**Capturar:** `CA-06-a.png` (reserva confirmada), `CA-06-b.png` (correo recibido),
`CA-06-c.png` (auditoría con el registro ENVIADO).

---

## CA-07 · Fallo de SMTP y reintento idempotente

**Qué hacer**

1. Provocar el fallo cambiando el servidor a uno inexistente y **reiniciar la aplicación**:
   ```bash
   setx SMARTEVENT_SMTP_HOST "smtp.no-existe.invalid"
   ```
2. Cancelar una reserva confirmada (motivo de al menos 20 caracteres).
3. Observar el aviso: el **estado sí cambió**, el correo **no se envió**.
4. Restaurar el host correcto y **reiniciar la aplicación**.
5. En **Auditoría → Correos**, seleccionar el intento con estado **ERROR** y pulsar
   **Reenviar notificación**.
6. Volver a consultar la reserva.

**Qué debe ocurrir**

- La cancelación se aplica aunque el correo falle; el intento queda auditado como **ERROR** con
  su motivo técnico.
- El reenvío genera un **segundo registro** con estado **ENVIADO**.
- La reserva **sigue CANCELADA una sola vez**.

Para comprobarlo **desde la propia aplicación**: abrir la reserva y pulsar el botón
**Historial de estados**. Muestra las transiciones numeradas y un recuento por estado:

```
3 cambio(s) de estado   |   Borrador: 1   |   Confirmada: 1   |   Cancelada: 1
```

Hubo **3 intentos de correo** y solo **3 transiciones de estado**, una por cada cambio real. El
reenvío añadió un registro en `com.CorreoEnviado` y **ninguna** fila en `evt.ReservaAuditoria`,
porque no toca el estado de la reserva.

**Capturar:** `CA-07-a.png` (aviso de correo fallido tras cambiar el estado), `CA-07-b.png`
(auditoría con los tres intentos: ENVIADO, ERROR, ENVIADO), `CA-07-c.png` (historial de estados
con una sola transición por cambio).

---

## CA-08 · Análisis con IA y salida estructurada

Requiere **`OPENAI_API_KEY` configurada** (ver [configuración de integraciones](../configuracion-integraciones.md)).

**Qué hacer**

1. Abrir una reserva guardada con al menos un recurso.
2. Pulsar **Analizar reserva con IA**.
3. Revisar la ventana de resultado.
4. Ir a **Auditoría → Análisis de IA** y seleccionar el registro.

**Qué debe ocurrir**

- Se muestra: **nivel de riesgo** (BAJO/MEDIO/ALTO), **resumen**, **alertas**,
  **recomendaciones** y **borrador de correo sugerido**.
- El pie indica el modelo, la versión del prompt y los tokens consumidos.
- El aviso deja claro que el análisis **no se envía automáticamente** y que la decisión es del
  usuario.
- En la auditoría, el **JSON estructurado** aparece formateado y con nivel de riesgo coincidente.

**Capturar:** `CA-08-a.png` (ventana de análisis), `CA-08-b.png` (JSON persistido en auditoría).

---

## CA-09 · Timeout o clave ausente sin colapsar la aplicación

**Parte A — sin clave**

```bash
setx OPENAI_API_KEY ""
```
Reiniciar la aplicación. La barra de estado muestra *"IA: sin configurar"*. Pulsar
**Analizar reserva con IA**.

**Parte B — timeout**

```bash
setx SMARTEVENT_IA_TIMEOUT "1"
```
Reiniciar y volver a analizar.

**Parte C — confirmación por contingencia**

Con la IA no disponible, pulsar **Confirmar reserva**: la aplicación ofrece registrar una
**justificación de contingencia** de al menos 20 caracteres.

**Qué debe ocurrir**

- En A y B aparece un mensaje explicativo y **la aplicación sigue funcionando con normalidad**.
- Ambos intentos quedan auditados en `evt.AnalisisIA` con `Exitoso = 0` y su motivo.
- En C, la reserva se confirma y la justificación queda guardada y auditada.

**Capturar:** `CA-09-a.png` (mensaje sin clave), `CA-09-b.png` (mensaje de timeout),
`CA-09-c.png` (intentos fallidos en la auditoría), `CA-09-d.png` (confirmación con contingencia).

---

## CA-10 · Reproducibilidad desde un clon limpio

**Qué hacer en un equipo distinto (o en una carpeta nueva)**

1. `git clone <URL> SmartEventAI && cd SmartEventAI`
2. Ejecutar `database/00_SmartEventAI.sql`.
3. Definir `SMARTEVENT_CONNECTION` y reabrir la terminal.
4. `dotnet run --project src\SmartEvent.UI`
5. Entrar con `admin` / `Admin123*` y completar el flujo: crear reserva → validar → guardar →
   analizar → confirmar.

**Qué debe ocurrir**

- Todo funciona **siguiendo únicamente el README**, sin pasos adicionales no documentados.
- No hace falta editar ningún archivo del proyecto.

**Capturar:** `CA-10-a.png` (salida del script en la máquina nueva), `CA-10-b.png` (aplicación
en marcha con la reserva creada allí).

---

## Resumen de evidencias

| Caso | Capturas | Estado |
|---|---|---|
| CA-01 | `CA-01-a`, `CA-01-b` | ☐ |
| CA-02 | `CA-02` | ☐ |
| CA-03 | `CA-03-a`, `CA-03-b` | ☐ |
| CA-04 | `CA-04` | ☐ |
| CA-05 | `CA-05-a`, `CA-05-b`, `CA-05-c` | ☐ |
| CA-06 | `CA-06-a`, `CA-06-b`, `CA-06-c` | ☐ |
| CA-07 | `CA-07-a`, `CA-07-b`, `CA-07-c` | ☐ |
| CA-08 | `CA-08-a`, `CA-08-b` | ☐ |
| CA-09 | `CA-09-a` … `CA-09-d` | ☐ |
| CA-10 | `CA-10-a`, `CA-10-b` | ☐ |

> **Importante:** usar siempre datos ficticios y el buzón de pruebas. Ninguna captura debe
> mostrar claves, contraseñas ni la cadena de conexión completa.

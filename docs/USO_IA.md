# Uso de inteligencia artificial en el desarrollo

> El enunciado permite usar IA y exige declararlo. Usarla no resta puntos; **ocultarlo o
> entregar código que no se comprende, sí**. Este documento describe con exactitud cómo se usó.

---

## 1. Herramienta utilizada

| Herramienta | Uso |
|---|---|
| **Claude (Claude Code)** | Asistente principal: generación de código, revisión y ejecución de pruebas |
| `sqlcmd` | Ejecución y verificación real del script SQL |
| `dotnet` CLI | Compilación y ejecución de la solución y del arnés de pruebas |
| SQL Server 2025 Developer (local) | Base de datos de desarrollo |

**El trabajo se hizo por fases**, cada una compilada y ejecutada antes de pasar a la siguiente:

| Fase | Contenido | Verificación |
|---|---|---|
| 1 | Script SQL completo | 22 aserciones del banco `99_pruebas_CA.sql`, todas correctas |
| 2 | Core + Infrastructure.Data | 39 aserciones contra la base real |
| 3 | Application | 47 aserciones más (86 acumuladas) |
| 4 | Integraciones (MailKit + IA) | 39 aserciones más (125 acumuladas) |
| 5 | Windows Forms | Aplicación ejecutada; capturas en `docs/capturas/` |
| 6 | Documentación y evidencias | Este documento, README y guía de casos |
| 7 | Configuración real y evidencias CA-01 a CA-10 | Groq y Mailtrap configurados; 4 defectos más detectados y corregidos |

---

## 2. Qué se generó con asistencia de IA

**Prácticamente todo el código fue escrito con asistencia de IA.** No tiene sentido afirmar
otra cosa: lo que importa es que cada pieza se verificó ejecutándola y que puedo explicar por
qué está hecha así.

| Parte | Generado | Verificado cómo |
|---|---|---|
| `database/00_SmartEventAI.sql` | Sí | Ejecutado contra SQL Server; inventario de objetos comprobado |
| Procedimientos almacenados | Sí | 22 aserciones del banco de pruebas SQL |
| `SmartEvent.Core` | Sí | Compila sin advertencias; sin dependencias externas |
| Repositorios ADO.NET + TVP | Sí | 39 pruebas de integración contra la base real |
| Servicios de aplicación | Sí | 47 pruebas con dobles de correo e IA |
| Cliente SMTP y cliente de IA | Sí | 39 pruebas con manipulador HTTP simulado |
| Formularios Windows Forms | Sí | Aplicación ejecutada; 9 defectos de interfaz detectados y corregidos |
| Documentación | Sí | Revisada contra el código real |

---

## 3. Prompts relevantes

Los prompts se estructuraron por fases, con las restricciones del enunciado explícitas. Los más
determinantes fueron:

**Sobre la arquitectura**

> "Arquitectura en capas: UI (Windows Forms MDI), Core (entidades, DTOs, interfaces),
> Application (lógica de negocio), Infrastructure.Data (ADO.NET con TVP), Infrastructure.Integrations
> (MailKit y OpenAI). La UI no puede contener SQL, connection strings ni llamadas directas a
> OpenAI/SMTP."

**Sobre la transacción cabecera-detalle**

> "`evt.sp_Reserva_Guardar`: transacción atómica obligatoria. Inserta o actualiza cabecera y
> sincroniza detalles mediante TVP. Si falla un detalle, ROLLBACK completo, sin cabeceras
> huérfanas. Recalcula y persiste Subtotal, Impuesto (15 %) y Total **en SQL**."

**Sobre las restricciones técnicas**

> "100 % asíncrono, sin `.Result`, `.Wait()` ni `Thread.Sleep()`. Sin SQL concatenado. Sin
> contraseñas planas. Sin API keys en el código."

**Sobre el proveedor de IA**

> "Cualquier cosa que uses para conectar con IA, si es de pago busca una alternativa gratuita."

Este último cambió una decisión de diseño: en lugar de acoplar el cliente al SDK oficial de
OpenAI, se implementó un **cliente HTTP propio del contrato de la Responses API** con `BaseUrl`
y modelo configurables. El enunciado lo admite explícitamente
(*"o una implementación HTTP equivalente bien encapsulada"*), y permite apuntar a proveedores
compatibles con nivel gratuito sin cambiar una línea de código.

---

## 4. Errores detectados y corregidos durante el desarrollo

Esta es la parte más útil del documento: **compilar no es funcionar**. Cada uno de estos fallos
apareció al ejecutar de verdad, no al escribir el código.

### En la base de datos

| # | Problema | Causa | Corrección |
|---|---|---|---|
| 1 | `CREATE TABLE failed ... 'QUOTED_IDENTIFIER'` | `sqlcmd` deja `QUOTED_IDENTIFIER` en OFF y la columna calculada persistida lo exige en ON | Se fija `SET QUOTED_IDENTIFIER ON` explícitamente en el script |
| 2 | `Incorrect syntax near '('` en el banco de pruebas | `EXEC` no admite subconsultas como valor de parámetro | Se asigna a una variable antes de llamar |
| 3 | `Subqueries are not allowed in this context` | `PRINT` tampoco admite subconsultas | Se asigna a una variable antes de imprimir |
| 4 | Las etiquetas `[OK]` desaparecían de la salida | `sqlcmd` suprime los corchetes al inicio de línea en `PRINT` | Se cambió el formato a `OK    \| texto` |
| 5 | El símbolo `%` desaparecía de los mensajes | Se pierde en la consola | Los mensajes dicen "10 por ciento" en lugar de "10 %" |

### En el banco de pruebas

| # | Problema | Causa | Corrección |
|---|---|---|---|
| 6 | **CA-03 falló**: no detectaba el cruce de horario | El banco calculaba `hoy + 30` para localizar la reserva demo, pero la instalación se había ejecutado antes de medianoche y las pruebas después: apuntaban a fechas distintas | Se rediseñó el banco para que sea **autocontenido**: crea sus propias reservas ancla y no depende de la fecha de instalación |

Este fue el error más instructivo: la prueba no era incorrecta, era **frágil**. Una prueba que
falla según la hora a la que se ejecuta no sirve como evidencia.

### En el código C#

| # | Problema | Causa | Corrección |
|---|---|---|---|
| 7 | Un análisis de riesgo **ALTO** podía auditarse como **BAJO** | `NivelRiesgoEnum` tenía setter privado y solo se poblaba al llamar a `Validar()`; si el orden de llamadas cambiaba, quedaba el valor por defecto | Se convirtió en **propiedad derivada** del campo de texto: ya no depende del orden de llamadas |
| 8 | `error CS8417` al compilar el servicio de correo | `SmtpClient` de MailKit implementa `IDisposable`, no `IAsyncDisposable` | `using` síncrono; las operaciones de red siguen siendo asíncronas |
| 9 | `error CS0234: 'Run' no existe en 'SmartEvent.Application'` | Colisión entre mi capa `SmartEvent.Application` y `System.Windows.Forms.Application` | Alias explícito `using AplicacionWinForms = System.Windows.Forms.Application;` |
| 10 | El arnés interpretaba `--nologo` como cadena de conexión | Aceptaba `args[0]` sin comprobar | Solo se acepta un argumento que contenga `=` y no empiece por `-` |

El **número 7 es el más grave**: no rompía nada visible y corrompía datos en silencio. Lo
detectó una prueba que compara el nivel persistido con el devuelto por el modelo.

### En la interfaz (detectados al ejecutar y capturar pantalla)

| # | Problema | Corrección |
|---|---|---|
| 11 | La etiqueta de conexión se solapaba con el botón *Ingresar* cuando el nombre del servidor era largo | Se reorganizaron las filas del formulario de login |
| 12 | La etiqueta "Nombre / razón social" se solapaba con su cuadro de texto | Se acortó a "Razón social" |
| 13 | `FrmCatalogos` lanzaba las tres consultas al abrir y la barra de estado mostraba un mensaje que no correspondía a la pestaña visible | **Carga diferida**: cada catálogo se consulta la primera vez que se muestra |
| 14 | El combo de recursos dejaba un precio residual del primer elemento aunque no hubiera selección | El evento `SelectedIndexChanged` se dispara al enlazar el `DataSource`, antes de `SelectedIndex = -1`; se resetea el precio cuando no hay selección |
| 15 | La columna *Código* truncaba el código de reserva y la etiqueta "Por página" se solapaba con su combo | Ajuste de anchos y posiciones |

### Detectados al generar las evidencias CA-01 a CA-10

Esta segunda tanda apareció usando la aplicación con datos reales, ya con las integraciones
configuradas. Son los más interesantes porque ninguno impedía que el programa funcionara.

| # | Problema | Causa | Corrección |
|---|---|---|---|
| 16 | La pantalla de auditoría mostraba `Corporación` en lugar de `Corporación` | `JsonSerializer` escapa por defecto todo lo que no sea ASCII; es una protección para incrustar JSON en HTML o JavaScript, innecesaria en un `TextBox` de Windows Forms | Se configuró el `Encoder` para permitir Latin-1. **El JSON que se persiste no se toca**: solo cambia cómo se muestra |
| 17 | Las recomendaciones del análisis se cortaban por la derecha | Estaban en un `ListBox`, que no ajusta línea | Se cambiaron a `TextBox` multilínea de solo lectura |
| 18 | El historial de estados existía en la base y en los servicios, pero **no se podía consultar desde la aplicación** | `sp_Reserva_Auditoria_Consultar` y `ObtenerHistorialAsync` estaban implementados y nunca se expusieron en la interfaz | Se añadió el botón *Historial de estados* y el formulario `FrmHistorialEstados` |
| 19 | Al rechazar un descuento no autorizado, **la celda quedaba bloqueada sin ningún mensaje visible** | El aviso se escribía en `ErrorText` de la fila, que el `DataGridView` dibuja en el encabezado de fila… y estaba configurado con `RowHeadersVisible = false` | Se activó el encabezado de fila y se añadió un aviso modal con el motivo |

El **19 es el más instructivo de todos**: la validación funcionaba perfectamente y rechazaba el
valor como debía. El fallo era que el usuario no tenía forma de enterarse. Una regla de negocio
correctamente implementada pero invisible es, en la práctica, una aplicación rota.

El **18 revela otra cosa**: tener la funcionalidad en la base y en los servicios no sirve de
nada si nadie puede llegar a ella. Apareció al preparar la evidencia de CA-07, cuando hubo que
demostrar que el reenvío de un correo no genera transiciones de estado duplicadas.

Ninguno de estos nueve defectos de interfaz lo detecta el compilador ni una prueba de
integración. Aparecieron **ejecutando la aplicación y mirando la pantalla**.

---

## 5. Decisiones de diseño que conviene poder defender

Estas decisiones no son automáticas: se tomaron y se justificaron.

### 5.1 Autenticación en dos llamadas, no en una

El enunciado pide *"consultar usuario activo y datos de autorización sin exponer el hash a la
interfaz"*. Con BCrypt habría que traer el hash al cliente para verificarlo. El diseño elegido:

1. `seg.sp_Usuario_ObtenerParametrosHash` devuelve **solo el salt** y las iteraciones.
2. La aplicación deriva la clave con PBKDF2 y envía **solo el resultado**.
3. `seg.sp_Usuario_Autenticar` compara **dentro del motor**.

Además, si el usuario no existe se devuelve un **salt señuelo** derivado de su nombre, para que
el coste y la forma de la respuesta sean idénticos y no se puedan enumerar cuentas válidas.

### 5.2 Los límites del contrato de IA se validan en C#, no en el JSON Schema

Los límites (`resumen` ≤ 300 caracteres, 0–5 alertas, 1–5 recomendaciones) **no** se expresaron
como `minItems`/`maxItems` en el esquema, porque esas palabras clave no están soportadas de
forma uniforme en modo estricto y algunos proveedores rechazan el esquema entero.

Van en la descripción de cada campo **y se comprueban en el cliente** con
`AnalisisIARespuesta.Validar()` antes de mostrar o persistir nada. El proveedor garantiza la
forma; la aplicación verifica el contenido.

### 5.3 El correo se envía **después** del cambio de estado

Si fuera antes, un fallo de SMTP obligaría a decidir si se confirma o no una reserva que el
motor ya validó. Poniéndolo después, la confirmación es firme y el correo es un efecto
secundario reintentable. Por eso `ReenviarNotificacionAsync` **no toca el estado**, y por eso
el reintento es idempotente por diseño, sin flags que mantener sincronizados.

### 5.4 El cálculo de totales está duplicado a propósito

`CalculadoraTotales` replica la fórmula del procedimiento almacenado. Es una **previsualización**
para que la grilla responda mientras el usuario escribe; el importe que se persiste lo recalcula
siempre SQL Server. El DTO de guardado ni siquiera tiene campos para `Subtotal`, `Impuesto` o
`Total`.

Detalle fino: el redondeo usa `MidpointRounding.AwayFromZero` porque el modo por defecto de .NET
(`ToEven`) produciría diferencias de un centavo respecto a SQL Server.

### 5.5 Inyección de dependencias manual

El enunciado admite contenedor o inyección manual. Con cinco proyectos, un contenedor añadiría
una dependencia sin resolver ningún problema real. `ContenedorServicios` muestra el grafo
completo de la aplicación en un solo constructor.

---

## 6. Qué debo revisar y poder explicar

> **Esta sección es responsabilidad del estudiante.** El enunciado advierte que entregar código
> que no se comprende afecta la evaluación.

Puntos que debo tener claros antes de la defensa:

- [ ] Por qué el TVP hace posible la atomicidad y por qué un `INSERT` por fila no la garantiza.
- [ ] Qué hace exactamente `inicioNuevo < finExistente AND finNuevo > inicioExistente` y por
      qué una franja adyacente (fin = inicio) **no** es un cruce.
- [ ] Por qué `SubtotalLinea` es una columna calculada persistida y qué se gana con ello.
- [ ] Qué diferencia hay entre `IntentosFallidos` en la tabla y un contador en memoria.
- [ ] Por qué los errores con número ≥ 50000 se muestran tal cual y los demás no.
- [ ] Por qué los parámetros de salida se leen **después** de cerrar el `SqlDataReader`.
- [ ] Por qué la validación en C# no sustituye a la de SQL Server, y qué reglas **solo** puede
      comprobar el motor.

### Elementos que modifiqué o ajusté personalmente

> [COMPLETAR ANTES DE ENTREGAR: anotar aquí lo que se cambió respecto de lo generado —
> datos semilla, nombres, textos, disposición de los formularios, reglas adicionales, etc.]

### Qué aprendí

> [COMPLETAR ANTES DE ENTREGAR]

---

## 7. Declaración

El código de este repositorio se desarrolló con asistencia de IA, de forma iterativa y
verificada: cada fase se compiló, se ejecutó y se probó contra una base de datos real antes de
continuar con la siguiente. Los 19 errores documentados en la sección 4 se detectaron durante
esas ejecuciones y quedaron corregidos en el código entregado.

Asumo la responsabilidad de revisar, comprender y poder explicar cualquier archivo de esta
entrega.

**Josias Aguilar**

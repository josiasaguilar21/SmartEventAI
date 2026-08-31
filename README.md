# SmartEvent AI

Sistema de escritorio para la gestión de **reservas de salones, recursos y comunicación** en
eventos corporativos. Windows Forms sobre .NET 8, persistencia en SQL Server, notificación por
correo con MailKit y análisis de riesgo asistido por IA con salidas estructuradas.

**Autor:** Josias Aguilar
**Asignatura:** Desarrollo e Implementación de Aplicaciones de Escritorio
**Versión de entrega:** `v1.0.0` — commit `PENDIENTE_HASH_CORTO`

> Antes de entregar: crear el tag y reemplazar `PENDIENTE_HASH_CORTO` por el hash corto real.
> Ver la sección [Entrega](#12-entrega).

---

## Índice

1. [Qué resuelve](#1-qué-resuelve)
2. [Arquitectura](#2-arquitectura)
3. [Requisitos previos](#3-requisitos-previos)
4. [Puesta en marcha desde cero](#4-puesta-en-marcha-desde-cero)
5. [Configuración](#5-configuración)
6. [Usuarios semilla](#6-usuarios-semilla)
7. [Ejecución](#7-ejecución)
8. [Reglas de negocio implementadas](#8-reglas-de-negocio-implementadas)
9. [Procedimientos almacenados](#9-procedimientos-almacenados)
10. [Pruebas](#10-pruebas)
11. [Casos de aceptación CA-01 a CA-10](#11-casos-de-aceptación-ca-01-a-ca-10)
12. [Entrega](#12-entrega)
13. [Resolución de problemas](#13-resolución-de-problemas)

---

## 1. Qué resuelve

La empresa gestionaba las reservas en hojas de cálculo, con tres consecuencias: **doble
asignación de salones**, **cantidades de recursos superiores al inventario** y **totales
inconsistentes**. SmartEvent AI centraliza el proceso completo:

- Autenticación con roles **ADMINISTRADOR** y **COORDINADOR**, con contraseñas hasheadas.
- Mantenimiento de clientes, salones y recursos, con inactivación lógica.
- Reservas **cabecera–detalle** guardadas de forma **atómica** en una sola transacción.
- Control de **cruce de franja horaria**, **capacidad del salón** y **stock concurrente**.
- Flujo de estados `BORRADOR → CONFIRMADA → FINALIZADA`, con cancelación auditada.
- Correo HTML al confirmar o cancelar, con **reintento idempotente** si el SMTP falla.
- Análisis de riesgo con IA mediante **JSON Schema estricto**, validado y auditado.

---

## 2. Arquitectura

Cinco proyectos, más un arnés de pruebas. Las dependencias apuntan **siempre hacia adentro**:

```
┌──────────────────────────────────────────────────────────────────────┐
│  SmartEvent.UI                          (Windows Forms · net8.0-windows)
│  Formularios MDI · raíz de composición · manejo central de errores    │
│  NO contiene: SQL, cadenas de conexión, MailKit ni llamadas HTTP      │
└───────────────┬──────────────────────────────────────────────────────┘
                │ usa
┌───────────────▼──────────────────────────────────────────────────────┐
│  SmartEvent.Application                              (net8.0)         │
│  Servicios de negocio · validaciones · cálculo · control de estados   │
│  Referencia ÚNICAMENTE a Core                                         │
└───────────────┬──────────────────────────────────────────────────────┘
                │ depende de abstracciones
┌───────────────▼──────────────────────────────────────────────────────┐
│  SmartEvent.Core                                     (net8.0)         │
│  Entidades · DTOs · enums · excepciones · INTERFACES                  │
│  CERO paquetes NuGet. No conoce ninguna tecnología concreta.          │
└───────────────▲──────────────────────▲───────────────────────────────┘
                │ implementa           │ implementa
┌───────────────┴────────────┐  ┌──────┴────────────────────────────────┐
│ SmartEvent.Infrastructure  │  │ SmartEvent.Infrastructure             │
│ .Data                      │  │ .Integrations                         │
│ Microsoft.Data.SqlClient   │  │ MailKit + cliente HTTP del modelo IA  │
│ Repositorios · TVP · PBKDF2│  │ Plantilla HTML · JSON Schema estricto  │
└────────────────────────────┘  └───────────────────────────────────────┘
```

**Por qué importa esta dirección:** `Core` no referencia ningún paquete NuGet, y `Application`
solo referencia a `Core`. Cambiar SQL Server por otro motor, o el proveedor de IA por otro,
significa reescribir un proyecto de infraestructura **sin tocar una línea de lógica de negocio**.

| Proyecto | Responsabilidad |
|---|---|
| `src/SmartEvent.Core` | Dominio puro: entidades, DTOs, enums, excepciones e interfaces. |
| `src/SmartEvent.Application` | Reglas de negocio, orquestación, validaciones y cálculo de totales. |
| `src/SmartEvent.Infrastructure.Data` | Repositorios ADO.NET, parámetros tipados, TVP, hashing y registro. |
| `src/SmartEvent.Infrastructure.Integrations` | SMTP con MailKit y cliente del modelo de IA. |
| `src/SmartEvent.UI` | Formularios Windows Forms y composición de dependencias. |
| `tests/SmartEvent.PruebasIntegracion` | Arnés de pruebas. **No es la aplicación principal.** |

> El proyecto de pruebas es una consola porque es un **arnés de verificación**, no la
> aplicación. La aplicación es `SmartEvent.UI`, en Windows Forms.

El modelo de datos está en [docs/modelo-datos.md](docs/modelo-datos.md) y en
[docs/modelo-datos.png](docs/modelo-datos.png).

---

## 3. Requisitos previos

| Componente | Versión | Comprobación |
|---|---|---|
| .NET SDK | 8.0 o superior | `dotnet --version` |
| Runtime Windows Desktop | 8.0 | `dotnet --list-runtimes` debe incluir `Microsoft.WindowsDesktop.App 8.x` |
| SQL Server | 2017 o superior (Express sirve) | `sqlcmd -S .\INSTANCIA -E -C -Q "SELECT @@VERSION"` |
| Sistema operativo | Windows 10/11 | — |

Opcional: Visual Studio 2022 (17.8+) o SQL Server Management Studio.

> Con un SDK más reciente que el 8 la solución compila igual: los proyectos fijan
> `net8.0` / `net8.0-windows` de forma explícita.

---

## 4. Puesta en marcha desde cero

### Paso 1 — Clonar

```bash
git clone <URL-DEL-REPOSITORIO> SmartEventAI
cd SmartEventAI
```

### Paso 2 — Crear la base de datos

Un solo script hace todo: esquemas, tablas, restricciones, índices, tipo tabla, datos semilla
y los 23 procedimientos almacenados.

```bash
sqlcmd -S ".\SQLEXPRESS" -E -C -b -i "database\00_SmartEventAI.sql" -W
```

Sustituir `.\SQLEXPRESS` por el nombre de la instancia. Al terminar imprime un inventario de
objetos creados y las credenciales semilla.

> **Atención:** el script **elimina y recrea** la base `SmartEventAI` si ya existe. Es
> intencional, para garantizar que la instalación sea reproducible. No ejecutarlo sobre una
> base con datos que se quieran conservar.
>
> Desde SSMS o Azure Data Studio: abrir el archivo y ejecutarlo. Funciona igual, porque el
> script fija `SET QUOTED_IDENTIFIER ON` explícitamente (sqlcmd lo deja en OFF por defecto y
> eso impediría crear la columna calculada persistida).

### Paso 3 — Configurar la conexión

La vía más simple, sin tocar ningún archivo:

```bash
setx SMARTEVENT_CONNECTION "Server=.\SQLEXPRESS;Database=SmartEventAI;Integrated Security=True;TrustServerCertificate=True"
```

Cerrar y volver a abrir la terminal después de `setx`.

### Paso 4 — Compilar y ejecutar

```bash
dotnet run --project src\SmartEvent.UI
```

Iniciar sesión con `admin` / `Admin123*`.

**Con esto la aplicación ya funciona.** El correo y la IA son opcionales: sin ellos todo lo
demás opera con normalidad y los intentos quedan auditados con su motivo.

---

## 5. Configuración

Tres orígenes, de **menor a mayor** prioridad:

1. `src/SmartEvent.UI/appsettings.json` — archivo local, **excluido por `.gitignore`**.
2. **User Secrets** — `dotnet user-secrets`, fuera del árbol del proyecto.
3. **Variables de entorno** — ganan siempre. Es la vía recomendada.

El repositorio incluye [`appsettings.example.json`](src/SmartEvent.UI/appsettings.example.json)
con valores ficticios. Para usarlo, copiarlo como `appsettings.json` y editarlo.

### Variables disponibles

| Variable | Para qué | Obligatoria |
|---|---|---|
| `SMARTEVENT_CONNECTION` | Cadena de conexión a SQL Server | **Sí** |
| `OPENAI_API_KEY` | Clave del servicio de IA | No |
| `SMARTEVENT_IA_BASEURL` | Raíz de la API (por defecto OpenAI) | No |
| `SMARTEVENT_IA_MODELO` | Modelo a usar | No |
| `SMARTEVENT_IA_TIMEOUT` | Segundos de espera (45) | No |
| `SMARTEVENT_IA_REINTENTOS` | Reintentos ante 429/5xx (2) | No |
| `SMARTEVENT_SMTP_HOST` | Servidor SMTP | No |
| `SMARTEVENT_SMTP_PUERTO` | Puerto (587) | No |
| `SMARTEVENT_SMTP_USUARIO` | Cuenta SMTP | No |
| `SMARTEVENT_SMTP_CLAVE` | Contraseña o clave de aplicación | No |
| `SMARTEVENT_SMTP_REDIRECCION_PRUEBAS` | Redirige **todos** los correos a esta dirección | No |

El paso a paso para obtener credenciales **gratuitas** de IA y un buzón de pruebas está en
**[docs/configuracion-integraciones.md](docs/configuracion-integraciones.md)**.

> **Ninguna clave real aparece en el repositorio, en el historial de Git ni en las capturas.**
> El `.gitignore` excluye `appsettings.json`, `.env` y cualquier archivo de secretos, y fue
> creado **antes** del primer commit que incluyó configuración.

---

## 6. Usuarios semilla

| Usuario | Contraseña | Rol | Puede |
|---|---|---|---|
| `admin` | `Admin123*` | ADMINISTRADOR | Todo, incluidos descuentos de línea superiores al 10 % |
| `coordinador` | `Coord123*` | COORDINADOR | Gestionar reservas y **consultar** catálogos, sin modificarlos |

Son credenciales **de laboratorio**, documentadas a propósito para que el proyecto sea
reproducible. Las contraseñas se almacenan como **PBKDF2-HMAC-SHA256** con salt propio por
usuario y 120 000 iteraciones; en la base no existe texto plano.

Tras **5 intentos fallidos** la cuenta se bloquea 5 minutos. El bloqueo está persistido en
`seg.Usuario.BloqueadoHasta`: cerrar y reabrir la aplicación **no** lo evita.

---

## 7. Ejecución

```bash
# Aplicación
dotnet run --project src\SmartEvent.UI

# Arnés de pruebas de integración (requiere la base creada)
dotnet run --project tests\SmartEvent.PruebasIntegracion

# Banco de pruebas SQL de las reglas de negocio
sqlcmd -S ".\SQLEXPRESS" -E -C -i "database\99_pruebas_CA.sql" -W
```

Desde Visual Studio: abrir `SmartEvent.sln`, marcar `SmartEvent.UI` como proyecto de inicio y
pulsar F5.

---

## 8. Reglas de negocio implementadas

Cada regla se aplica en **SQL Server** —que es la fuente de verdad— y se anticipa en el
cliente para dar respuesta inmediata al usuario.

| Regla | Dónde se garantiza |
|---|---|
| `HoraFin > HoraInicio`, duración entre 2 y 12 h | `CK_Reserva_Duracion` + `sp_Disponibilidad_Validar` |
| Invitados > 0 y ≤ capacidad del salón | `CK_Reserva_Invitados` + `sp_Disponibilidad_Validar` |
| Sin cruce de franja: `inicioNuevo < finExistente AND finNuevo > inicioExistente` | `sp_Disponibilidad_Validar` |
| Al menos un detalle; sin recursos repetidos | `UQ_Detalle_Reserva_Rec` + `sp_Reserva_Guardar` |
| Cantidad > 0 y ≤ stock concurrente | `sp_Disponibilidad_Validar` |
| Precio ≥ 0; descuento de línea 0–20 %, > 10 % solo ADMINISTRADOR | `CK_Detalle_Descuento` + `sp_Reserva_Guardar` |
| `Subtotal` = tarifa base + Σ líneas; `Impuesto` = 15 % de la base neta | `sp_Reserva_Guardar` (recálculo) |
| `SubtotalLinea` no manipulable desde la aplicación | **Columna calculada PERSISTIDA** |
| Una reserva CONFIRMADA no edita cliente, salón, fecha, horario ni detalles | `sp_Reserva_Guardar` |
| Confirmar exige email válido, disponibilidad y análisis IA **o** contingencia auditada | `sp_Reserva_CambiarEstado` |
| Cancelar exige motivo de ≥ 20 caracteres | `CK_Reserva_Cancel` + `sp_Reserva_CambiarEstado` |
| `FINALIZADA` y `CANCELADA` son terminales | Tabla `evt.TransicionEstado` |

**Los importes son de solo lectura para la aplicación**: el DTO de guardado ni siquiera tiene
campos para `Subtotal`, `Impuesto` o `Total`. Los calcula y persiste el procedimiento.

---

## 9. Procedimientos almacenados

Los seis obligatorios más los de apoyo. **No hay una sola sentencia SQL fuera de la base**: la
clase base de los repositorios fija `CommandType.StoredProcedure`, de modo que es
estructuralmente imposible escribir SQL suelto en la capa de datos.

| Procedimiento | Responsabilidad |
|---|---|
| `evt.sp_Reserva_Guardar` | **Transacción atómica**. Cabecera + todo el detalle vía TVP. Recalcula importes. |
| `evt.sp_Reserva_Consultar` | Filtros combinables con paginación. Sin SQL dinámico. |
| `evt.sp_Reserva_ObtenerPorId` | Devuelve **dos** conjuntos: cabecera y detalle. |
| `evt.sp_Reserva_CambiarEstado` | Valida la transición, exige motivo y registra auditoría. |
| `evt.sp_Disponibilidad_Validar` | Cruce de horario, capacidad y stock concurrente. |
| `seg.sp_Usuario_Autenticar` | Compara el hash **dentro del motor**; nunca lo devuelve. |

Apoyo: `seg.sp_Usuario_ObtenerParametrosHash`, `seg.sp_Usuario_Consultar`, CRUD de los tres
catálogos, `evt.sp_Reserva_Auditoria_Consultar`, `com.sp_Correo_Registrar`,
`com.sp_Correo_Consultar`, `evt.sp_AnalisisIA_Registrar`, `evt.sp_AnalisisIA_Consultar` y
`evt.sp_AnalisisIA_ObtenerUltimo`.

**Convención de errores:** los de negocio se lanzan con `THROW` y número ≥ 50000 y un texto
redactado para el usuario final. La capa de datos los distingue del resto: los ≥ 50000 se
muestran tal cual; los demás se registran en el log y se sustituyen por un mensaje genérico.
Nunca se expone SQL, nombres de tablas ni cadenas de conexión.

---

## 10. Pruebas

Dos bancos de pruebas ejecutables y reproducibles:

### Banco SQL — reglas de negocio en el motor

```bash
sqlcmd -S ".\SQLEXPRESS" -E -C -i "database\99_pruebas_CA.sql" -W
```

**22 aserciones.** Demuestra CA-01 a CA-06 sin pasar por la interfaz: que el rechazo ocurre en
la base aunque se omita la validación visual. Es autocontenido y limpia lo que crea.

### Arnés de integración — capas de datos, aplicación e integraciones

```bash
set SMARTEVENT_CONNECTION=Server=.\SQLEXPRESS;Database=SmartEventAI;Integrated Security=True;TrustServerCertificate=True
dotnet run --project tests\SmartEvent.PruebasIntegracion
```

**125 aserciones** en tres bloques:

| Bloque | Qué comprueba |
|---|---|
| Capa de datos (39) | TVP, parámetros de salida, dos result sets, rollback, PBKDF2 real contra el hash sembrado |
| Capa de aplicación (47) | Orquestación, permisos por rol, cálculo de totales, CA-06/07/08/09 con dobles |
| Integraciones (39) | Codificación HTML del correo, JSON Schema estricto, timeout, 429, 401, JSON inválido |

El correo y la IA se sustituyen por **dobles**, lo que permite reproducir el fallo de SMTP y el
timeout del modelo de forma determinista, sin clave y sin coste.

---

## 11. Casos de aceptación CA-01 a CA-10

Guía paso a paso con capturas: **[docs/evidencias/GUIA_CASOS_ACEPTACION.md](docs/evidencias/GUIA_CASOS_ACEPTACION.md)**

| Caso | Qué se verifica | Verificación automática |
|---|---|---|
| CA-01 | Reserva con 3 detalles guardada y recuperada íntegra | Banco SQL + arnés |
| CA-02 | Error en un detalle → rollback completo, sin datos parciales | Banco SQL + arnés |
| CA-03 | Cruce parcial de franja rechazado | Banco SQL + arnés |
| CA-04 | Edición de BORRADOR sin autoconflicto | Banco SQL + arnés |
| CA-05 | Exceso de capacidad o de stock rechazado **desde SQL** | Banco SQL + arnés |
| CA-06 | Confirmación: una sola transición, correo y auditoría | Banco SQL + arnés |
| CA-07 | Fallo SMTP y reintento sin duplicar estados | Arnés (doble de correo) |
| CA-08 | Análisis IA con JSON estructurado, mostrado y persistido | Arnés (handler simulado) |
| CA-09 | Timeout o clave ausente sin colapsar la aplicación | Arnés (6 modos de fallo) |
| CA-10 | Clonar, ejecutar el script y completar el flujo solo con el README | Manual |

---

## 12. Entrega

### Checklist previo

- [ ] Clonar el repositorio en una carpeta nueva y comprobar que compila.
- [ ] Ejecutar `database/00_SmartEventAI.sql` en una instancia limpia.
- [ ] Ejecutar los dos bancos de pruebas y comprobar que no hay fallos.
- [ ] Ejecutar la aplicación y completar CA-01 a CA-10, capturando cada caso.
- [ ] Verificar que **no hay secretos** en los archivos ni en el historial:
      `git log -p | Select-String -Pattern "password=|api[_-]?key|sk-"`
- [ ] Completar `docs/USO_IA.md` con los prompts y decisiones propias.
- [ ] Crear el tag y anotar su hash corto en este README.

### Crear el tag de entrega

```bash
git tag -a v1.0.0 -m "Entrega SmartEvent AI v1.0.0"
```

Obtener el hash corto y escribirlo al principio de este README:

```bash
git rev-parse --short v1.0.0
```

Publicar:

```bash
git push origin main --tags
```

### Estructura del repositorio

```
SmartEventAI/
├── database/
│   ├── 00_SmartEventAI.sql          Script único de instalación
│   └── 99_pruebas_CA.sql            Banco de pruebas de reglas de negocio
├── docs/
│   ├── capturas/                    Capturas de los formularios
│   ├── evidencias/                  Guía y evidencias CA-01 a CA-10
│   ├── configuracion-integraciones.md
│   ├── modelo-datos.md / .png
│   └── USO_IA.md
├── src/
│   ├── SmartEvent.Core/
│   ├── SmartEvent.Application/
│   ├── SmartEvent.Infrastructure.Data/
│   ├── SmartEvent.Infrastructure.Integrations/
│   └── SmartEvent.UI/
├── tests/
│   └── SmartEvent.PruebasIntegracion/
├── .gitignore
├── README.md
└── SmartEvent.sln
```

---

## 13. Resolución de problemas

| Síntoma | Causa y solución |
|---|---|
| *"No se encontró la cadena de conexión"* al arrancar | Falta `SMARTEVENT_CONNECTION`. Definirla y **reabrir la terminal**: `setx` no afecta a las ventanas ya abiertas. |
| `CREATE TABLE failed because the following SET options have incorrect settings: 'QUOTED_IDENTIFIER'` | Se está ejecutando una versión modificada del script. El original fija `SET QUOTED_IDENTIFIER ON`. |
| Error de certificado al conectar a SQL Server | Añadir `TrustServerCertificate=True` a la cadena de conexión. |
| La barra de estado dice *"Sin conexión con la base de datos"* | El servicio de SQL Server no está iniciado o el nombre de instancia es incorrecto. Comprobar con `sqlcmd -S ".\INSTANCIA" -E -C -Q "SELECT 1"`. |
| *"Correo: sin configurar"* en la barra de estado | Normal si no se definieron las variables `SMARTEVENT_SMTP_*`. La aplicación funciona igual; los intentos se auditan con el motivo. |
| *"IA: sin configurar"* | Normal si no se definió `OPENAI_API_KEY`. Se puede confirmar reservas con la **justificación de contingencia**. |
| El análisis de IA responde *"El modelo o el endpoint configurados no existen"* | El modelo indicado no existe en el proveedor o no admite `json_schema` estricto. Ver [docs/configuracion-integraciones.md](docs/configuracion-integraciones.md). |
| No aparece el diseñador de formularios en Visual Studio | Compilar la solución una vez (`dotnet build`) y reabrir el formulario. |

El registro local de la aplicación está en
`%LOCALAPPDATA%\SmartEventAI\logs\smartevent-AAAAMMDD.log`, y su ruta exacta se muestra en la
barra de estado de la pantalla *Auditoría de integraciones*. **El registro enmascara
automáticamente** cualquier contraseña, clave de API o cadena de conexión que pudiera colarse
en un mensaje.

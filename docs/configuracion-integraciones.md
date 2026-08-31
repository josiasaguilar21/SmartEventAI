# Configuración de las integraciones externas

Esta guía explica cómo dejar operativos el **envío de correo** y el **análisis con IA** sin
escribir ninguna credencial en el código ni en el repositorio.

> **Regla que no se rompe nunca:** ninguna clave real aparece en archivos versionados, capturas
> ni historial de Git. Todo se configura mediante **variables de entorno** o mediante un
> `appsettings.json` local que el `.gitignore` excluye. El repositorio solo contiene
> `appsettings.example.json` con valores ficticios.

La aplicación arranca y funciona **aunque ninguna de estas dos integraciones esté configurada**:

| Integración sin configurar | Comportamiento |
|---|---|
| Correo | La reserva se confirma o cancela igualmente. El intento se audita como `ERROR` con el motivo y la interfaz ofrece reenviar. |
| IA | El análisis devuelve un mensaje explicativo y se audita. La reserva puede confirmarse usando la **justificación de contingencia**. |

---

## 1. Análisis con IA

El cliente habla el contrato de la **Responses API con salidas estructuradas** (`text.format`
con `type: json_schema` y `strict: true`). Como ese contrato lo implementan varios proveedores
de forma compatible, basta con apuntar `SMARTEVENT_IA_BASEURL` a uno u otro: **no hay que
cambiar una sola línea de código**.

Si el proveedor no expone `/responses`, el cliente **se repliega solo** a `/chat/completions`
enviando el mismo esquema en `response_format.json_schema`.

### Variables de entorno

| Variable | Obligatoria | Descripción |
|---|---|---|
| `OPENAI_API_KEY` | Sí | Clave de acceso. Es el nombre que exige el enunciado. |
| `SMARTEVENT_IA_BASEURL` | No | Raíz de la API, sin barra final. Por defecto `https://api.openai.com/v1`. |
| `SMARTEVENT_IA_MODELO` | No | Identificador del modelo. Por defecto `gpt-4o-mini`. |
| `SMARTEVENT_IA_TIMEOUT` | No | Segundos de espera máxima. Por defecto `45`. |
| `SMARTEVENT_IA_REINTENTOS` | No | Reintentos ante error temporal (429 / 5xx). Por defecto `2`. |
| `SMARTEVENT_IA_CHAT_COMPLETIONS` | No | `true` para ir directo a `/chat/completions` sin tantear `/responses`. |

### Opción A — Groq (recomendada: gratuita y expone Responses API)

Groq mantiene un nivel gratuito sin tarjeta y su API es compatible con la de OpenAI,
incluida la Responses API.

1. Crear cuenta en <https://console.groq.com> e iniciar sesión.
2. Ir a **API Keys** → **Create API Key** y copiar el valor (solo se muestra una vez).
3. Configurar las variables:

```bash
setx OPENAI_API_KEY "gsk_tu_clave_aqui"
setx SMARTEVENT_IA_BASEURL "https://api.groq.com/openai/v1"
setx SMARTEVENT_IA_MODELO "openai/gpt-oss-20b"
```

**Sobre el modelo:** hay que elegir uno que admita `json_schema` en modo estricto. Según la
documentación de Groq, lo admiten `openai/gpt-oss-20b`, `openai/gpt-oss-120b` y
`qwen/qwen3.8-27b`. La lista cambia con el tiempo: consultar
<https://console.groq.com/docs/structured-outputs> antes de la entrega. Si el modelo elegido no
soporta el esquema estricto, el servicio devolverá un error controlado indicándolo —
la aplicación no se cae.

### Opción B — OpenRouter (modelos con sufijo `:free`)

1. Crear cuenta en <https://openrouter.ai> → **Keys** → **Create Key**.
2. Elegir en <https://openrouter.ai/models> un modelo con sufijo `:free` que indique soporte de
   *structured outputs*.

```bash
setx OPENAI_API_KEY "sk-or-v1-tu_clave_aqui"
setx SMARTEVENT_IA_BASEURL "https://openrouter.ai/api/v1"
setx SMARTEVENT_IA_MODELO "el-modelo-elegido:free"
setx SMARTEVENT_IA_CHAT_COMPLETIONS "true"
```

### Opción C — Google AI Studio (Gemini, endpoint compatible con OpenAI)

1. Obtener la clave en <https://aistudio.google.com/apikey>.

```bash
setx OPENAI_API_KEY "tu_clave_de_google_ai_studio"
setx SMARTEVENT_IA_BASEURL "https://generativelanguage.googleapis.com/v1beta/openai"
setx SMARTEVENT_IA_MODELO "gemini-2.5-flash"
setx SMARTEVENT_IA_CHAT_COMPLETIONS "true"
```

### Opción D — OpenAI

```bash
setx OPENAI_API_KEY "sk-tu_clave_aqui"
```

No hace falta nada más: los valores por defecto ya apuntan a `https://api.openai.com/v1`.

> `setx` escribe la variable de forma permanente para el usuario, pero **no** afecta a las
> ventanas ya abiertas. Cerrar y volver a abrir la terminal (o Visual Studio) después de
> ejecutarlo.

### Comprobar que funciona

Con las variables puestas, abrir una reserva con al menos un recurso y pulsar
**Analizar reserva con IA**. El resultado, exitoso o fallido, queda registrado en
`evt.AnalisisIA` y se puede consultar en *Auditoría de integraciones*.

---

## 2. Correo SMTP

| Variable | Obligatoria | Descripción |
|---|---|---|
| `SMARTEVENT_SMTP_HOST` | Sí | Servidor SMTP. |
| `SMARTEVENT_SMTP_PUERTO` | No | `587` (STARTTLS) por defecto; `465` para SSL implícito. |
| `SMARTEVENT_SMTP_USUARIO` | Sí | Cuenta de acceso. |
| `SMARTEVENT_SMTP_CLAVE` | Sí | Contraseña o clave de aplicación. |
| `SMARTEVENT_SMTP_SSL` | No | `true` para SSL implícito (puerto 465). Por defecto `false` (STARTTLS). |
| `SMARTEVENT_SMTP_REMITENTE` | No | Dirección del remitente. Si se omite, se usa el usuario. |
| `SMARTEVENT_SMTP_REMITENTE_NOMBRE` | No | Nombre visible. Por defecto `SmartEvent AI`. |
| `SMARTEVENT_SMTP_TIMEOUT` | No | Segundos de espera. Por defecto `20`. |
| `SMARTEVENT_SMTP_REDIRECCION_PRUEBAS` | No | **Redirige todos los correos a esta dirección.** |

### Opción recomendada para las evidencias: Mailtrap

Un buzón de pruebas captura los mensajes sin entregarlos a nadie. Es la forma más segura de
generar las capturas de CA-06 y CA-07 sin escribir a direcciones reales.

1. Crear cuenta gratuita en <https://mailtrap.io> → **Email Testing** → **Inbox**.
2. Copiar las credenciales SMTP que muestra la bandeja.

```bash
setx SMARTEVENT_SMTP_HOST "sandbox.smtp.mailtrap.io"
setx SMARTEVENT_SMTP_PUERTO "587"
setx SMARTEVENT_SMTP_USUARIO "usuario_que_muestra_mailtrap"
setx SMARTEVENT_SMTP_CLAVE "clave_que_muestra_mailtrap"
```

### Opción con Gmail

Requiere verificación en dos pasos y una **contraseña de aplicación** (no la contraseña normal
de la cuenta): <https://myaccount.google.com/apppasswords>.

```bash
setx SMARTEVENT_SMTP_HOST "smtp.gmail.com"
setx SMARTEVENT_SMTP_PUERTO "587"
setx SMARTEVENT_SMTP_USUARIO "tu.cuenta@gmail.com"
setx SMARTEVENT_SMTP_CLAVE "la_clave_de_aplicacion_de_16_letras"
setx SMARTEVENT_SMTP_REDIRECCION_PRUEBAS "tu.cuenta@gmail.com"
```

La última variable es importante durante las pruebas: garantiza que ningún correo llegue a las
direcciones ficticias de los clientes semilla.

---

## 3. Cómo demostrar CA-07 (fallo de SMTP y reintento)

No hace falta romper nada de forma permanente:

1. Configurar SMTP correctamente y **confirmar** una reserva → el correo se envía y se audita
   como `ENVIADO`.
2. Cambiar `SMARTEVENT_SMTP_HOST` a un valor inexistente (por ejemplo
   `smtp.no-existe.invalid`) y **reiniciar la aplicación**.
3. Cancelar otra reserva → el estado cambia igualmente y el intento se audita como `ERROR`.
4. Restaurar el host correcto, reiniciar y pulsar **Reenviar** desde *Auditoría de
   integraciones* → queda un segundo registro `ENVIADO`, **sin** que la reserva cambie de
   estado por segunda vez.

## 4. Cómo demostrar CA-09 (fallo de IA)

1. Borrar temporalmente la variable: `setx OPENAI_API_KEY ""` y reiniciar la aplicación.
2. Pulsar **Analizar reserva con IA** → aparece un mensaje explicativo, la aplicación sigue
   funcionando y el intento queda auditado en `evt.AnalisisIA` con `Exitoso = 0`.
3. Para simular un tiempo de espera agotado: `setx SMARTEVENT_IA_TIMEOUT "1"` y reintentar.
4. Confirmar la reserva usando la **justificación de contingencia** (mínimo 20 caracteres),
   que queda guardada y auditada en la propia reserva.

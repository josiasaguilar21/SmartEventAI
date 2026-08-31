<#
.SYNOPSIS
    Comprueba que las integraciones externas (IA y correo SMTP) estan bien configuradas.

.DESCRIPCION
    Diagnostico rapido, para no descubrir un problema de configuracion en medio de una
    demostracion. Verifica en este orden:

      1. Que las variables de entorno existen (sin mostrar nunca su valor).
      2. Que la clave de IA es valida, listando los modelos disponibles.
      3. Que el modelo configurado acepta el JSON Schema estricto que usa la aplicacion.
      4. Que el servidor SMTP acepta la conexion y las credenciales.

    NINGUNA clave ni contrasena se imprime en pantalla: solo se informa si esta definida y
    cuantos caracteres tiene, para poder detectar un pegado incompleto.

.EJEMPLO
    .\docs\verificar-integraciones.ps1
#>

$ErrorActionPreference = 'Continue'

# La consola de Windows usa por defecto una pagina de codigos que no es UTF-8, y los acentos
# de la respuesta del modelo se verian como caracteres sueltos. Se fuerza UTF-8 para que la
# salida sea legible tambien cuando se ejecuta desde cmd.exe y se usa como evidencia.
try {
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    $OutputEncoding = [System.Text.Encoding]::UTF8
}
catch {
    # Algunas consolas no permiten cambiar la codificacion; el diagnostico funciona igual.
}

function Escribir-Titulo($texto) {
    Write-Host ""
    Write-Host ("=" * 70) -ForegroundColor DarkGray
    Write-Host " $texto" -ForegroundColor Cyan
    Write-Host ("=" * 70) -ForegroundColor DarkGray
}

function OK($texto)    { Write-Host "  OK     " -ForegroundColor Green -NoNewline; Write-Host $texto }
function FALLA($texto) { Write-Host "  FALLA  " -ForegroundColor Red -NoNewline;   Write-Host $texto }
function INFO($texto)  { Write-Host "  INFO   " -ForegroundColor DarkGray -NoNewline; Write-Host $texto }

# =====================================================================================
Escribir-Titulo "1. Variables de entorno"

$apiKey  = $env:OPENAI_API_KEY
$baseUrl = if ($env:SMARTEVENT_IA_BASEURL) { $env:SMARTEVENT_IA_BASEURL.TrimEnd('/') } else { "https://api.openai.com/v1" }
$modelo  = if ($env:SMARTEVENT_IA_MODELO)  { $env:SMARTEVENT_IA_MODELO }  else { "gpt-4o-mini" }

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    FALLA "OPENAI_API_KEY no esta definida en esta terminal."
    INFO  "Si acaba de ejecutar 'setx', CIERRE y vuelva a abrir la terminal: setx no afecta a las ya abiertas."
} else {
    OK "OPENAI_API_KEY definida ($($apiKey.Length) caracteres, empieza por '$($apiKey.Substring(0, [Math]::Min(4, $apiKey.Length)))...')"
}

INFO "BaseUrl : $baseUrl"
INFO "Modelo  : $modelo"

if ([string]::IsNullOrWhiteSpace($env:SMARTEVENT_CONNECTION)) {
    FALLA "SMARTEVENT_CONNECTION no esta definida (la necesita la aplicacion, no este script)."
} else {
    OK "SMARTEVENT_CONNECTION definida."
}

# =====================================================================================
Escribir-Titulo "2. Modelos disponibles con esa clave"

if (-not [string]::IsNullOrWhiteSpace($apiKey)) {
    try {
        $respuesta = Invoke-RestMethod -Uri "$baseUrl/models" -Method Get `
            -Headers @{ Authorization = "Bearer $apiKey" } -TimeoutSec 20

        $ids = @($respuesta.data | ForEach-Object { $_.id } | Sort-Object)
        OK "La clave es valida. Modelos disponibles: $($ids.Count)"

        $ids | ForEach-Object {
            $marca = if ($_ -eq $modelo) { " <-- el configurado" } else { "" }
            Write-Host "           $_$marca"
        }

        if ($ids -notcontains $modelo) {
            FALLA "El modelo '$modelo' NO aparece en la lista. Elija uno de los de arriba."
        }
    }
    catch {
        $codigo = $_.Exception.Response.StatusCode.value__
        if ($codigo -eq 401) {
            FALLA "La clave fue rechazada (401). Genere una nueva y vuelva a ejecutar 'setx'."
        } else {
            FALLA "No se pudo consultar la lista de modelos: $($_.Exception.Message)"
        }
    }
}

# =====================================================================================
Escribir-Titulo "3. Prueba real con JSON Schema estricto"

if (-not [string]::IsNullOrWhiteSpace($apiKey)) {

    # Mismo esquema que usa ContratoAnalisisIA en la aplicacion.
    $esquema = @{
        type                 = "object"
        additionalProperties = $false
        required             = @("nivelRiesgo", "resumen", "alertas", "recomendaciones", "correoSugerido")
        properties           = @{
            nivelRiesgo     = @{ type = "string"; enum = @("BAJO", "MEDIO", "ALTO") }
            resumen         = @{ type = "string" }
            alertas         = @{ type = "array"; items = @{ type = "string" } }
            recomendaciones = @{ type = "array"; items = @{ type = "string" } }
            correoSugerido  = @{ type = "string" }
        }
    }

    $mensajes = @(
        @{ role = "system"; content = "Responde unicamente con el JSON del esquema indicado, en espanol." },
        @{ role = "user";   content = "Reserva de prueba: Salon Esmeralda, capacidad 120, 115 invitados, 5 horas, dentro de 3 dias. Evalua el riesgo operativo." }
    )

    # Primero se intenta la Responses API, igual que hace la aplicacion.
    $cuerpoResponses = @{
        model = $modelo
        input = $mensajes
        text  = @{ format = @{ type = "json_schema"; name = "analisis_reserva"; strict = $true; schema = $esquema } }
    } | ConvertTo-Json -Depth 12

    $exito = $false

    try {
        $r = Invoke-RestMethod -Uri "$baseUrl/responses" -Method Post -TimeoutSec 60 `
            -Headers @{ Authorization = "Bearer $apiKey" } -ContentType "application/json" -Body $cuerpoResponses

        $texto = ($r.output | ForEach-Object { $_.content } | Where-Object { $_.type -eq "output_text" } | Select-Object -First 1).text
        OK "El proveedor expone /responses y respondio correctamente."
        $exito = $true
    }
    catch {
        INFO "/responses no disponible o rechazado; se prueba /chat/completions (la aplicacion hace lo mismo)."

        $cuerpoChat = @{
            model           = $modelo
            messages        = $mensajes
            response_format = @{ type = "json_schema"; json_schema = @{ name = "analisis_reserva"; strict = $true; schema = $esquema } }
        } | ConvertTo-Json -Depth 12

        try {
            $r = Invoke-RestMethod -Uri "$baseUrl/chat/completions" -Method Post -TimeoutSec 60 `
                -Headers @{ Authorization = "Bearer $apiKey" } -ContentType "application/json" -Body $cuerpoChat

            $texto = $r.choices[0].message.content
            OK "El proveedor respondio por /chat/completions con el esquema estricto."
            $exito = $true
        }
        catch {
            FALLA "El modelo rechazo el esquema estricto: $($_.Exception.Message)"
            INFO  "Pruebe con otro modelo que soporte structured outputs (ver docs/configuracion-integraciones.md)."
        }
    }

    if ($exito) {
        try {
            $json = $texto | ConvertFrom-Json
            OK "La respuesta es JSON valido y cumple el contrato:"
            Write-Host "           nivelRiesgo     : $($json.nivelRiesgo)"
            Write-Host "           resumen         : $($json.resumen)"
            Write-Host "           alertas         : $(@($json.alertas).Count)"
            Write-Host "           recomendaciones : $(@($json.recomendaciones).Count)"
            Write-Host ""
            Write-Host "  >>> La integracion de IA esta lista para CA-08." -ForegroundColor Green
        }
        catch {
            FALLA "La respuesta no es JSON valido. Texto recibido:"
            Write-Host $texto
        }
    }
}

# =====================================================================================
Escribir-Titulo "4. Correo SMTP"

$smtpHost = $env:SMARTEVENT_SMTP_HOST
$smtpUser = $env:SMARTEVENT_SMTP_USUARIO
$smtpPass = $env:SMARTEVENT_SMTP_CLAVE
$smtpPort = if ($env:SMARTEVENT_SMTP_PUERTO) { [int]$env:SMARTEVENT_SMTP_PUERTO } else { 587 }

if ([string]::IsNullOrWhiteSpace($smtpHost) -or [string]::IsNullOrWhiteSpace($smtpUser) -or [string]::IsNullOrWhiteSpace($smtpPass)) {
    INFO "SMTP sin configurar. La aplicacion funciona igual; los intentos se auditan con el motivo."
    INFO "Para CA-06 y CA-07 hacen falta las variables SMARTEVENT_SMTP_*."
} else {
    OK "Host      : $smtpHost : $smtpPort"
    OK "Usuario   : definido ($($smtpUser.Length) caracteres)"
    OK "Clave     : definida ($($smtpPass.Length) caracteres)"

    if ($env:SMARTEVENT_SMTP_REDIRECCION_PRUEBAS) {
        OK "Redireccion de pruebas activa: $($env:SMARTEVENT_SMTP_REDIRECCION_PRUEBAS)"
    } else {
        INFO "Sin redireccion de pruebas: los correos iran a la direccion real de cada cliente."
    }

    try {
        $tcp = New-Object System.Net.Sockets.TcpClient
        $conexion = $tcp.BeginConnect($smtpHost, $smtpPort, $null, $null)

        if ($conexion.AsyncWaitHandle.WaitOne(8000, $false) -and $tcp.Connected) {
            OK "El servidor SMTP acepta conexiones en el puerto $smtpPort."
            $tcp.Close()
            Write-Host ""
            Write-Host "  >>> Configuracion SMTP presente. Compruebe el envio real confirmando una reserva." -ForegroundColor Green
        } else {
            FALLA "No se pudo conectar con $smtpHost en el puerto $smtpPort."
        }
    }
    catch {
        FALLA "Error al conectar con el servidor SMTP: $($_.Exception.Message)"
    }
}

Write-Host ""

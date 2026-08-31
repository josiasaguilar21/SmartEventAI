# Genera docs/modelo-datos.png dibujando el modelo entidad-relacion con System.Drawing.
Add-Type -AssemblyName System.Drawing

$W = 1680; $H = 1200
$bmp = New-Object System.Drawing.Bitmap($W, $H)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit
$g.Clear([System.Drawing.Color]::White)

$fTitulo   = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
$fCampo    = New-Object System.Drawing.Font("Consolas", 8.5)
$fCampoPk  = New-Object System.Drawing.Font("Consolas", 8.5, [System.Drawing.FontStyle]::Bold)
$fCabecera = New-Object System.Drawing.Font("Segoe UI", 20, [System.Drawing.FontStyle]::Bold)
$fSub      = New-Object System.Drawing.Font("Segoe UI", 10)
$fLeyenda  = New-Object System.Drawing.Font("Segoe UI", 9)

$colSeg = [System.Drawing.Color]::FromArgb(120, 60, 130)
$colEvt = [System.Drawing.Color]::FromArgb(31, 56, 100)
$colCom = [System.Drawing.Color]::FromArgb(20, 110, 90)
$colBorde = [System.Drawing.Color]::FromArgb(190, 198, 212)
$colTexto = [System.Drawing.Color]::FromArgb(32, 36, 43)
$colGris  = [System.Drawing.Color]::FromArgb(110, 118, 130)

$brTexto = New-Object System.Drawing.SolidBrush($colTexto)
$brBlanco = [System.Drawing.Brushes]::White
$brGris = New-Object System.Drawing.SolidBrush($colGris)
$penBorde = New-Object System.Drawing.Pen($colBorde, 1)
$penRel = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(140, 150, 165), 1.6)
$penRel.EndCap = [System.Drawing.Drawing2D.LineCap]::ArrowAnchor

$cajaX = @{}
$cajaY = @{}
$cajaW = @{}
$cajaH = @{}

function Dibujar-Tabla($nombre, $x, $y, $ancho, $color, $campos) {
    $altoTitulo = 30
    $altoCampo = 17
    $alto = $altoTitulo + ($campos.Count * $altoCampo) + 8

    $rect = New-Object System.Drawing.Rectangle($x, $y, $ancho, $alto)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), $rect)
    $g.DrawRectangle($penBorde, $rect)

    $rectTit = New-Object System.Drawing.Rectangle($x, $y, $ancho, $altoTitulo)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($color)), $rectTit)
    $g.DrawString($nombre, $fTitulo, $brBlanco, ($x + 8), ($y + 6))

    $yc = $y + $altoTitulo + 4
    foreach ($c in $campos) {
        $esClave = $c.StartsWith("PK") -or $c.StartsWith("FK") -or $c.StartsWith("UQ")
        $fuente = if ($esClave) { $fCampoPk } else { $fCampo }
        $brocha = if ($esClave) { $brTexto } else { $brGris }
        $g.DrawString($c, $fuente, $brocha, ($x + 8), $yc)
        $yc += $altoCampo
    }

    $script:cajaX[$nombre] = [int]$x
    $script:cajaY[$nombre] = [int]$y
    $script:cajaW[$nombre] = [int]$ancho
    $script:cajaH[$nombre] = [int]$alto
}

function Unir($origen, $destino) {
    $aX = $script:cajaX[$origen]; $aY = $script:cajaY[$origen]
    $aW = $script:cajaW[$origen]; $aH = $script:cajaH[$origen]
    $bX = $script:cajaX[$destino]; $bY = $script:cajaY[$destino]
    $bW = $script:cajaW[$destino]; $bH = $script:cajaH[$destino]

    $acx = $aX + [int]($aW / 2); $acy = $aY + [int]($aH / 2)
    $bcx = $bX + [int]($bW / 2); $bcy = $bY + [int]($bH / 2)

    # Se sale y se entra por el borde mas cercano, para que la linea no cruce las cajas.
    if ([Math]::Abs($bcx - $acx) -gt [Math]::Abs($bcy - $acy)) {
        if ($bcx -gt $acx) { $x1 = $aX + $aW; $y1 = $acy; $x2 = $bX;      $y2 = $bcy }
        else               { $x1 = $aX;       $y1 = $acy; $x2 = $bX + $bW; $y2 = $bcy }
    } else {
        if ($bcy -gt $acy) { $x1 = $acx; $y1 = $aY + $aH; $x2 = $bcx; $y2 = $bY }
        else               { $x1 = $acx; $y1 = $aY;       $x2 = $bcx; $y2 = $bY + $bH }
    }

    $mx = [int](($x1 + $x2) / 2)
    $g.DrawLine($penRel, [int]$x1, [int]$y1, $mx, [int]$y1)
    $g.DrawLine($penRel, $mx, [int]$y1, $mx, [int]$y2)
    $g.DrawLine($penRel, $mx, [int]$y2, [int]$x2, [int]$y2)
}

# ---------------------------------------------------------------- encabezado
$g.DrawString("SmartEvent AI - Modelo de datos", $fCabecera, $brTexto, 40, 26)
$g.DrawString("SQL Server - esquemas seg (seguridad), evt (eventos) y com (comunicaciones)", $fSub, $brGris, 42, 62)
$g.DrawLine((New-Object System.Drawing.Pen($colBorde, 2)), 40, 92, ($W - 40), 92)

# ---------------------------------------------------------------- seguridad
Dibujar-Tabla "seg.Rol" 45 130 280 $colSeg @(
    "PK IdRol            INT IDENTITY",
    "UQ Nombre           NVARCHAR(30)",
    "   Descripcion      NVARCHAR(150)",
    "   Estado           BIT",
    "   FechaCreacion    DATETIME2")

Dibujar-Tabla "seg.Usuario" 45 275 280 $colSeg @(
    "PK IdUsuario        INT IDENTITY",
    "UQ NombreUsuario    NVARCHAR(50)",
    "   NombreCompleto   NVARCHAR(120)",
    "   PasswordHash     VARBINARY(64)",
    "   PasswordSalt     VARBINARY(32)",
    "   Iteraciones      INT",
    "   Algoritmo        VARCHAR(20)",
    "FK IdRol            INT",
    "   Estado           BIT",
    "   IntentosFallidos INT",
    "   BloqueadoHasta   DATETIME2",
    "   UltimoAcceso     DATETIME2",
    "   FechaCreacion    DATETIME2")

# ---------------------------------------------------------------- catalogos
Dibujar-Tabla "evt.Cliente" 385 130 285 $colEvt @(
    "PK IdCliente        INT IDENTITY",
    "UQ Identificacion   NVARCHAR(20)",
    "   Nombres          NVARCHAR(150)",
    "   Email            NVARCHAR(150)",
    "   Telefono         NVARCHAR(20)",
    "   Estado           BIT")

Dibujar-Tabla "evt.Salon" 385 305 285 $colEvt @(
    "PK IdSalon          INT IDENTITY",
    "UQ Nombre           NVARCHAR(100)",
    "   Ubicacion        NVARCHAR(150)",
    "   Capacidad        INT",
    "   TarifaBase       DECIMAL(12,2)",
    "   Estado           BIT")

Dibujar-Tabla "evt.Recurso" 385 480 285 $colEvt @(
    "PK IdRecurso        INT IDENTITY",
    "UQ Nombre           NVARCHAR(100)",
    "   Tipo             VARCHAR(20)",
    "   StockTotal       INT",
    "   PrecioUnitario   DECIMAL(12,2)",
    "   Estado           BIT")

Dibujar-Tabla "evt.TransicionEstado" 385 665 285 $colEvt @(
    "PK EstadoOrigen     VARCHAR(12)",
    "PK EstadoDestino    VARCHAR(12)",
    "   RequiereMotivo   BIT")

# ---------------------------------------------------------------- transaccional
Dibujar-Tabla "evt.Reserva" 745 130 320 $colEvt @(
    "PK IdReserva        INT IDENTITY",
    "UQ Codigo           NVARCHAR(20)",
    "FK IdCliente        INT",
    "FK IdSalon          INT",
    "   FechaEvento      DATE",
    "   HoraInicio       TIME(0)",
    "   HoraFin          TIME(0)",
    "   NumeroInvitados  INT",
    "   Estado           VARCHAR(12)",
    "   Subtotal         DECIMAL(12,2)",
    "   Descuento        DECIMAL(12,2)",
    "   Impuesto         DECIMAL(12,2)",
    "   Total            DECIMAL(12,2)",
    "   Observacion      NVARCHAR(500)",
    "   MotivoCancelacion",
    "   JustificacionContingencia",
    "FK IdUsuarioCreacion",
    "   FechaCreacion    DATETIME2",
    "FK IdUsuarioModificacion",
    "   FechaModificacion")

Dibujar-Tabla "evt.ReservaDetalle" 745 560 320 $colEvt @(
    "PK IdDetalle        INT IDENTITY",
    "FK IdReserva        INT  (CASCADE)",
    "FK IdRecurso        INT",
    "   Cantidad         INT",
    "   PrecioUnitario   DECIMAL(12,2)",
    "   PorcentajeDescuento DEC(5,2)",
    "   SubtotalLinea    CALCULADA",
    "UQ (IdReserva, IdRecurso)")

Dibujar-Tabla "evt.ReservaAuditoria" 745 760 320 $colEvt @(
    "PK IdAuditoria      BIGINT",
    "FK IdReserva        INT  (CASCADE)",
    "   EstadoAnterior   VARCHAR(12)",
    "   EstadoNuevo      VARCHAR(12)",
    "   Motivo           NVARCHAR(500)",
    "FK IdUsuario        INT",
    "   Fecha            DATETIME2")

# ---------------------------------------------------------------- integraciones
Dibujar-Tabla "evt.AnalisisIA" 1145 130 300 $colEvt @(
    "PK IdAnalisis       INT IDENTITY",
    "FK IdReserva        INT  (CASCADE)",
    "   Modelo           NVARCHAR(100)",
    "   PromptVersion    NVARCHAR(20)",
    "   RespuestaJson    NVARCHAR(MAX)",
    "   NivelRiesgo      VARCHAR(5)",
    "   TokensEntrada    INT",
    "   TokensSalida     INT",
    "   Fecha            DATETIME2",
    "   Exitoso          BIT",
    "   Error            NVARCHAR(500)",
    "FK IdUsuario        INT")

Dibujar-Tabla "com.CorreoEnviado" 1145 420 300 $colCom @(
    "PK IdCorreo         INT IDENTITY",
    "FK IdReserva        INT  (CASCADE)",
    "   TipoNotificacion VARCHAR(20)",
    "   Destinatario     NVARCHAR(150)",
    "   Asunto           NVARCHAR(200)",
    "   FechaIntento     DATETIME2",
    "   Estado           VARCHAR(10)",
    "   Error            NVARCHAR(500)",
    "FK IdUsuario        INT")

Dibujar-Tabla "TYPE evt.ReservaDetalleType" 1145 660 300 $colCom @(
    "   IdRecurso        INT",
    "   Cantidad         INT",
    "   PrecioUnitario   DECIMAL(12,2)",
    "   PorcentajeDescuento DEC(5,2)")

# ---------------------------------------------------------------- relaciones
Unir "seg.Rol" "seg.Usuario"
Unir "evt.Cliente" "evt.Reserva"
Unir "evt.Salon" "evt.Reserva"
Unir "evt.Reserva" "evt.ReservaDetalle"
Unir "evt.Recurso" "evt.ReservaDetalle"
Unir "evt.Reserva" "evt.ReservaAuditoria"
Unir "evt.Reserva" "evt.AnalisisIA"
Unir "evt.Reserva" "com.CorreoEnviado"

# ---------------------------------------------------------------- notas
$yn = 945
$notas = @(
    "PK clave primaria    FK clave foranea    UQ restriccion unica    (CASCADE) el borrado de la reserva arrastra sus filas dependientes",
    "",
    "evt.ReservaDetalle.SubtotalLinea es una COLUMNA CALCULADA PERSISTIDA: Cantidad * PrecioUnitario * (1 - PorcentajeDescuento / 100).",
    "El importe de linea lo deriva el motor, no la aplicacion.",
    "",
    "TYPE evt.ReservaDetalleType es el parametro tipo tabla (TVP) que transporta TODO el detalle en una sola llamada a evt.sp_Reserva_Guardar,",
    "de modo que la cabecera y sus lineas se confirman o se revierten dentro de una unica transaccion.",
    "",
    "seg.Usuario no almacena contrasenas: guarda el resultado de PBKDF2-HMAC-SHA256 con salt propio por usuario y 120000 iteraciones."
)
foreach ($n in $notas) {
    $g.DrawString($n, $fLeyenda, $brGris, 45, $yn)
    $yn += 19
}

$g.DrawString("Generado a partir de database/00_SmartEventAI.sql", $fLeyenda, $brGris, 45, ($H - 42))

$g.Dispose()
# La imagen se guarda junto al script, para que funcione desde cualquier ubicacion del repositorio.
$bmp.Save((Join-Path $PSScriptRoot "modelo-datos.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
"Diagrama generado: $W x $H"

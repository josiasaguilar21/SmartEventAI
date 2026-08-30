/*==============================================================================================
  PROYECTO INTEGRADOR : SmartEvent AI
  ARCHIVO             : /database/00_SmartEventAI.sql
  AUTOR               : Josias Aguilar
  MOTOR               : SQL Server 2019+ (compatible con 2017+)
  DESCRIPCION         : Script UNICO y REPRODUCIBLE. Crea la base de datos completa desde cero:
                        esquemas, tablas, PK/FK/CHECK, indices, tipo tabla (TVP), datos semilla
                        y todos los procedimientos almacenados. No requiere intervencion manual.

  ORDEN DE EJECUCION INTERNO
    SECCION 00 : Creacion / recreacion de la base de datos
    SECCION 01 : Esquemas (seg, evt, com)
    SECCION 02 : Tablas de seguridad        (seg.Rol, seg.Usuario)
    SECCION 03 : Tablas de catalogo         (evt.Cliente, evt.Salon, evt.Recurso)
    SECCION 04 : Tablas transaccionales     (evt.Reserva, evt.ReservaDetalle, auditoria, estados)
    SECCION 05 : Tablas de integracion      (evt.AnalisisIA, com.CorreoEnviado)
    SECCION 06 : Secuencia y tipo tabla     (evt.sq_Reserva, evt.ReservaDetalleType)
    SECCION 07 : Indices no clustered
    SECCION 08 : Datos semilla
    SECCION 09 : SP de seguridad
    SECCION 10 : SP de catalogos
    SECCION 11 : SP de disponibilidad
    SECCION 12 : SP de reservas (transaccion cabecera-detalle con TVP)
    SECCION 13 : SP de integraciones (correo / analisis IA)
    SECCION 14 : Reserva de demostracion + verificacion final

  ADVERTENCIA: la SECCION 00 ELIMINA la base SmartEventAI si ya existe. Es intencional para
  garantizar la reproducibilidad exigida (CA-10). No ejecutar sobre un servidor productivo.
==============================================================================================*/

SET NOCOUNT ON;
-- ANSI_NULLS y QUOTED_IDENTIFIER deben estar en ON para poder crear la columna calculada
-- persistida evt.ReservaDetalle.SubtotalLinea. sqlcmd los deja en OFF por defecto, por eso
-- se fijan explicitamente y el script funciona igual desde SSMS, Azure Data Studio o sqlcmd.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*==============================================================================================
  SECCION 00 : CREACION / RECREACION DE LA BASE DE DATOS
==============================================================================================*/
USE master;
GO

IF DB_ID(N'SmartEventAI') IS NOT NULL
BEGIN
    PRINT '>> Base de datos SmartEventAI existente. Se elimina para recrearla desde cero.';
    ALTER DATABASE SmartEventAI SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SmartEventAI;
END
GO

CREATE DATABASE SmartEventAI;
GO

ALTER DATABASE SmartEventAI SET RECOVERY SIMPLE;
GO

-- READ COMMITTED SNAPSHOT reduce el bloqueo entre las lecturas asincronicas de la UI y las
-- escrituras de la transaccion cabecera-detalle, sin recurrir a NOLOCK ni a lecturas sucias.
ALTER DATABASE SmartEventAI SET READ_COMMITTED_SNAPSHOT ON;
GO

USE SmartEventAI;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*==============================================================================================
  SECCION 01 : ESQUEMAS
    seg = seguridad / autenticacion
    evt = negocio de eventos (clientes, salones, recursos, reservas, IA)
    com = comunicaciones (auditoria de correos)
==============================================================================================*/
CREATE SCHEMA seg AUTHORIZATION dbo;
GO
CREATE SCHEMA evt AUTHORIZATION dbo;
GO
CREATE SCHEMA com AUTHORIZATION dbo;
GO

/*==============================================================================================
  SECCION 02 : TABLAS DE SEGURIDAD
==============================================================================================*/
CREATE TABLE seg.Rol
(
    IdRol           INT             IDENTITY(1,1) NOT NULL,
    Nombre          NVARCHAR(30)    NOT NULL,
    Descripcion     NVARCHAR(150)   NULL,
    Estado          BIT             NOT NULL CONSTRAINT DF_Rol_Estado DEFAULT (1),
    FechaCreacion   DATETIME2(0)    NOT NULL CONSTRAINT DF_Rol_FechaCreacion DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Rol           PRIMARY KEY CLUSTERED (IdRol),
    CONSTRAINT UQ_Rol_Nombre    UNIQUE (Nombre),
    CONSTRAINT CK_Rol_Nombre    CHECK (Nombre IN (N'ADMINISTRADOR', N'COORDINADOR'))
);
GO

/*  seg.Usuario
    La contrasena se almacena como PBKDF2-HMAC-SHA256: hash de 32 bytes + salt de 16 bytes por
    usuario + numero de iteraciones. Nunca se guarda texto plano y el hash NUNCA viaja hacia la
    aplicacion: la comparacion se hace dentro de seg.sp_Usuario_Autenticar.
    IntentosFallidos / BloqueadoHasta implementan el bloqueo temporal exigido en FrmLogin,
    persistido en la base para que no pueda evadirse reiniciando la aplicacion.                */
CREATE TABLE seg.Usuario
(
    IdUsuario           INT             IDENTITY(1,1) NOT NULL,
    NombreUsuario       NVARCHAR(50)    NOT NULL,
    NombreCompleto      NVARCHAR(120)   NOT NULL,
    Email               NVARCHAR(150)   NULL,
    PasswordHash        VARBINARY(64)   NOT NULL,
    PasswordSalt        VARBINARY(32)   NOT NULL,
    Iteraciones         INT             NOT NULL CONSTRAINT DF_Usuario_Iteraciones DEFAULT (120000),
    Algoritmo           VARCHAR(20)     NOT NULL CONSTRAINT DF_Usuario_Algoritmo DEFAULT ('PBKDF2-SHA256'),
    IdRol               INT             NOT NULL,
    Estado              BIT             NOT NULL CONSTRAINT DF_Usuario_Estado DEFAULT (1),
    IntentosFallidos    INT             NOT NULL CONSTRAINT DF_Usuario_Intentos DEFAULT (0),
    BloqueadoHasta      DATETIME2(0)    NULL,
    UltimoAcceso        DATETIME2(0)    NULL,
    FechaCreacion       DATETIME2(0)    NOT NULL CONSTRAINT DF_Usuario_FechaCreacion DEFAULT (SYSDATETIME()),
    FechaModificacion   DATETIME2(0)    NULL,
    CONSTRAINT PK_Usuario           PRIMARY KEY CLUSTERED (IdUsuario),
    CONSTRAINT UQ_Usuario_Nombre    UNIQUE (NombreUsuario),
    CONSTRAINT FK_Usuario_Rol       FOREIGN KEY (IdRol) REFERENCES seg.Rol (IdRol),
    CONSTRAINT CK_Usuario_Intentos  CHECK (IntentosFallidos >= 0),
    CONSTRAINT CK_Usuario_Iter      CHECK (Iteraciones >= 10000),
    CONSTRAINT CK_Usuario_Nombre    CHECK (LEN(LTRIM(RTRIM(NombreUsuario))) >= 4)
);
GO

/*==============================================================================================
  SECCION 03 : TABLAS DE CATALOGO
==============================================================================================*/
CREATE TABLE evt.Cliente
(
    IdCliente           INT             IDENTITY(1,1) NOT NULL,
    Identificacion      NVARCHAR(20)    NOT NULL,
    Nombres             NVARCHAR(150)   NOT NULL,
    Email               NVARCHAR(150)   NOT NULL,
    Telefono            NVARCHAR(20)    NULL,
    Estado              BIT             NOT NULL CONSTRAINT DF_Cliente_Estado DEFAULT (1),
    FechaCreacion       DATETIME2(0)    NOT NULL CONSTRAINT DF_Cliente_FechaCreacion DEFAULT (SYSDATETIME()),
    FechaModificacion   DATETIME2(0)    NULL,
    CONSTRAINT PK_Cliente           PRIMARY KEY CLUSTERED (IdCliente),
    CONSTRAINT UQ_Cliente_Ident     UNIQUE (Identificacion),
    CONSTRAINT CK_Cliente_Ident     CHECK (LEN(LTRIM(RTRIM(Identificacion))) >= 5),
    CONSTRAINT CK_Cliente_Nombres   CHECK (LEN(LTRIM(RTRIM(Nombres))) >= 3),
    -- Validacion estructural minima del correo: texto + arroba + dominio + punto + extension.
    CONSTRAINT CK_Cliente_Email     CHECK (Email LIKE N'%_@__%.__%' AND Email NOT LIKE N'% %')
);
GO

CREATE TABLE evt.Salon
(
    IdSalon             INT             IDENTITY(1,1) NOT NULL,
    Nombre              NVARCHAR(100)   NOT NULL,
    Ubicacion           NVARCHAR(150)   NULL,
    Capacidad           INT             NOT NULL,
    TarifaBase          DECIMAL(12,2)   NOT NULL,
    Estado              BIT             NOT NULL CONSTRAINT DF_Salon_Estado DEFAULT (1),
    FechaCreacion       DATETIME2(0)    NOT NULL CONSTRAINT DF_Salon_FechaCreacion DEFAULT (SYSDATETIME()),
    FechaModificacion   DATETIME2(0)    NULL,
    CONSTRAINT PK_Salon             PRIMARY KEY CLUSTERED (IdSalon),
    CONSTRAINT UQ_Salon_Nombre      UNIQUE (Nombre),
    CONSTRAINT CK_Salon_Capacidad   CHECK (Capacidad > 0),
    CONSTRAINT CK_Salon_Tarifa      CHECK (TarifaBase >= 0)
);
GO

CREATE TABLE evt.Recurso
(
    IdRecurso           INT             IDENTITY(1,1) NOT NULL,
    Nombre              NVARCHAR(100)   NOT NULL,
    Tipo                VARCHAR(20)     NOT NULL,
    StockTotal          INT             NOT NULL,
    PrecioUnitario      DECIMAL(12,2)   NOT NULL,
    Estado              BIT             NOT NULL CONSTRAINT DF_Recurso_Estado DEFAULT (1),
    FechaCreacion       DATETIME2(0)    NOT NULL CONSTRAINT DF_Recurso_FechaCreacion DEFAULT (SYSDATETIME()),
    FechaModificacion   DATETIME2(0)    NULL,
    CONSTRAINT PK_Recurso           PRIMARY KEY CLUSTERED (IdRecurso),
    CONSTRAINT UQ_Recurso_Nombre    UNIQUE (Nombre),
    CONSTRAINT CK_Recurso_Tipo      CHECK (Tipo IN ('EQUIPO', 'MOBILIARIO', 'SERVICIO', 'CATERING')),
    CONSTRAINT CK_Recurso_Stock     CHECK (StockTotal >= 0),
    CONSTRAINT CK_Recurso_Precio    CHECK (PrecioUnitario >= 0)
);
GO

/*==============================================================================================
  SECCION 04 : TABLAS TRANSACCIONALES (CABECERA - DETALLE)
==============================================================================================*/
/*  evt.Reserva : cabecera.
    Reglas garantizadas por el motor (no solo por la UI):
      - HoraFin > HoraInicio y duracion entre 2 y 12 horas (CK_Reserva_Duracion)
      - NumeroInvitados > 0 (la capacidad del salon se valida en evt.sp_Disponibilidad_Validar)
      - Estado restringido al flujo BORRADOR / CONFIRMADA / FINALIZADA / CANCELADA
      - Importes no negativos y coherencia Total = (Subtotal - Descuento) + Impuesto
      - Una reserva CANCELADA obliga a tener motivo de al menos 20 caracteres                  */
CREATE TABLE evt.Reserva
(
    IdReserva                   INT             IDENTITY(1,1) NOT NULL,
    Codigo                      NVARCHAR(20)    NOT NULL,
    IdCliente                   INT             NOT NULL,
    IdSalon                     INT             NOT NULL,
    FechaEvento                 DATE            NOT NULL,
    HoraInicio                  TIME(0)         NOT NULL,
    HoraFin                     TIME(0)         NOT NULL,
    NumeroInvitados             INT             NOT NULL,
    Estado                      VARCHAR(12)     NOT NULL CONSTRAINT DF_Reserva_Estado DEFAULT ('BORRADOR'),
    Subtotal                    DECIMAL(12,2)   NOT NULL CONSTRAINT DF_Reserva_Subtotal DEFAULT (0),
    Descuento                   DECIMAL(12,2)   NOT NULL CONSTRAINT DF_Reserva_Descuento DEFAULT (0),
    Impuesto                    DECIMAL(12,2)   NOT NULL CONSTRAINT DF_Reserva_Impuesto DEFAULT (0),
    Total                       DECIMAL(12,2)   NOT NULL CONSTRAINT DF_Reserva_Total DEFAULT (0),
    Observacion                 NVARCHAR(500)   NULL,
    MotivoCancelacion           NVARCHAR(500)   NULL,
    JustificacionContingencia   NVARCHAR(500)   NULL,
    IdUsuarioCreacion           INT             NOT NULL,
    FechaCreacion               DATETIME2(0)    NOT NULL CONSTRAINT DF_Reserva_FechaCreacion DEFAULT (SYSDATETIME()),
    IdUsuarioModificacion       INT             NULL,
    FechaModificacion           DATETIME2(0)    NULL,
    CONSTRAINT PK_Reserva           PRIMARY KEY CLUSTERED (IdReserva),
    CONSTRAINT UQ_Reserva_Codigo    UNIQUE (Codigo),
    CONSTRAINT FK_Reserva_Cliente   FOREIGN KEY (IdCliente) REFERENCES evt.Cliente (IdCliente),
    CONSTRAINT FK_Reserva_Salon     FOREIGN KEY (IdSalon)   REFERENCES evt.Salon (IdSalon),
    CONSTRAINT FK_Reserva_UsuCrea   FOREIGN KEY (IdUsuarioCreacion)     REFERENCES seg.Usuario (IdUsuario),
    CONSTRAINT FK_Reserva_UsuModi   FOREIGN KEY (IdUsuarioModificacion) REFERENCES seg.Usuario (IdUsuario),
    CONSTRAINT CK_Reserva_Estado    CHECK (Estado IN ('BORRADOR', 'CONFIRMADA', 'FINALIZADA', 'CANCELADA')),
    CONSTRAINT CK_Reserva_Horas     CHECK (HoraFin > HoraInicio),
    CONSTRAINT CK_Reserva_Duracion  CHECK (DATEDIFF(MINUTE, HoraInicio, HoraFin) BETWEEN 120 AND 720),
    CONSTRAINT CK_Reserva_Invitados CHECK (NumeroInvitados > 0),
    CONSTRAINT CK_Reserva_Importes  CHECK (Subtotal >= 0 AND Descuento >= 0 AND Impuesto >= 0 AND Total >= 0),
    CONSTRAINT CK_Reserva_DescMax   CHECK (Descuento <= Subtotal),
    CONSTRAINT CK_Reserva_TotalOk   CHECK (Total = CAST((Subtotal - Descuento) + Impuesto AS DECIMAL(12,2))),
    CONSTRAINT CK_Reserva_Cancel    CHECK (Estado <> 'CANCELADA' OR LEN(LTRIM(RTRIM(ISNULL(MotivoCancelacion, N'')))) >= 20)
);
GO

/*  evt.ReservaDetalle : lineas de la reserva.
    SubtotalLinea es una COLUMNA CALCULADA PERSISTIDA: el importe de linea no puede ser
    manipulado desde la aplicacion, siempre lo deriva el motor de base de datos.              */
CREATE TABLE evt.ReservaDetalle
(
    IdDetalle           INT             IDENTITY(1,1) NOT NULL,
    IdReserva           INT             NOT NULL,
    IdRecurso           INT             NOT NULL,
    Cantidad            INT             NOT NULL,
    PrecioUnitario      DECIMAL(12,2)   NOT NULL,
    PorcentajeDescuento DECIMAL(5,2)    NOT NULL CONSTRAINT DF_Detalle_Desc DEFAULT (0),
    SubtotalLinea       AS CAST(Cantidad * PrecioUnitario * (1 - PorcentajeDescuento / 100.0) AS DECIMAL(12,2)) PERSISTED,
    CONSTRAINT PK_ReservaDetalle        PRIMARY KEY CLUSTERED (IdDetalle),
    CONSTRAINT FK_Detalle_Reserva       FOREIGN KEY (IdReserva) REFERENCES evt.Reserva (IdReserva) ON DELETE CASCADE,
    CONSTRAINT FK_Detalle_Recurso       FOREIGN KEY (IdRecurso) REFERENCES evt.Recurso (IdRecurso),
    CONSTRAINT UQ_Detalle_Reserva_Rec   UNIQUE (IdReserva, IdRecurso),
    CONSTRAINT CK_Detalle_Cantidad      CHECK (Cantidad > 0),
    CONSTRAINT CK_Detalle_Precio        CHECK (PrecioUnitario >= 0),
    CONSTRAINT CK_Detalle_Descuento     CHECK (PorcentajeDescuento >= 0 AND PorcentajeDescuento <= 20)
);
GO

/*  evt.ReservaAuditoria : bitacora de transiciones de estado (quien, cuando, desde/hacia, motivo). */
CREATE TABLE evt.ReservaAuditoria
(
    IdAuditoria     BIGINT          IDENTITY(1,1) NOT NULL,
    IdReserva       INT             NOT NULL,
    EstadoAnterior  VARCHAR(12)     NULL,
    EstadoNuevo     VARCHAR(12)     NOT NULL,
    Motivo          NVARCHAR(500)   NULL,
    IdUsuario       INT             NOT NULL,
    Fecha           DATETIME2(0)    NOT NULL CONSTRAINT DF_ResAud_Fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_ReservaAuditoria  PRIMARY KEY CLUSTERED (IdAuditoria),
    CONSTRAINT FK_ResAud_Reserva    FOREIGN KEY (IdReserva) REFERENCES evt.Reserva (IdReserva) ON DELETE CASCADE,
    CONSTRAINT FK_ResAud_Usuario    FOREIGN KEY (IdUsuario) REFERENCES seg.Usuario (IdUsuario)
);
GO

/*  evt.TransicionEstado : matriz de transiciones permitidas. Se modela como dato y no como
    IF encadenados para que el flujo sea auditable y ampliable sin recompilar el procedimiento. */
CREATE TABLE evt.TransicionEstado
(
    EstadoOrigen    VARCHAR(12)     NOT NULL,
    EstadoDestino   VARCHAR(12)     NOT NULL,
    RequiereMotivo  BIT             NOT NULL CONSTRAINT DF_Transicion_Motivo DEFAULT (0),
    CONSTRAINT PK_TransicionEstado  PRIMARY KEY CLUSTERED (EstadoOrigen, EstadoDestino),
    CONSTRAINT CK_Transicion_Orig   CHECK (EstadoOrigen  IN ('BORRADOR', 'CONFIRMADA', 'FINALIZADA', 'CANCELADA')),
    CONSTRAINT CK_Transicion_Dest   CHECK (EstadoDestino IN ('BORRADOR', 'CONFIRMADA', 'FINALIZADA', 'CANCELADA'))
);
GO

/*==============================================================================================
  SECCION 05 : TABLAS DE INTEGRACION
==============================================================================================*/
/*  evt.AnalisisIA : auditoria de cada llamada al modelo. Guarda el JSON estructurado devuelto,
    el modelo y la version del prompt. NUNCA almacena la API key.                             */
CREATE TABLE evt.AnalisisIA
(
    IdAnalisis      INT             IDENTITY(1,1) NOT NULL,
    IdReserva       INT             NOT NULL,
    Modelo          NVARCHAR(100)   NOT NULL,
    PromptVersion   NVARCHAR(20)    NOT NULL,
    RespuestaJson   NVARCHAR(MAX)   NULL,
    NivelRiesgo     VARCHAR(5)      NULL,
    TokensEntrada   INT             NULL,
    TokensSalida    INT             NULL,
    Fecha           DATETIME2(0)    NOT NULL CONSTRAINT DF_AnalisisIA_Fecha DEFAULT (SYSDATETIME()),
    Exitoso         BIT             NOT NULL,
    Error           NVARCHAR(500)   NULL,
    IdUsuario       INT             NULL,
    CONSTRAINT PK_AnalisisIA        PRIMARY KEY CLUSTERED (IdAnalisis),
    CONSTRAINT FK_AnalisisIA_Res    FOREIGN KEY (IdReserva) REFERENCES evt.Reserva (IdReserva) ON DELETE CASCADE,
    CONSTRAINT FK_AnalisisIA_Usu    FOREIGN KEY (IdUsuario) REFERENCES seg.Usuario (IdUsuario),
    CONSTRAINT CK_AnalisisIA_Riesgo CHECK (NivelRiesgo IS NULL OR NivelRiesgo IN ('BAJO', 'MEDIO', 'ALTO')),
    CONSTRAINT CK_AnalisisIA_Json   CHECK (RespuestaJson IS NULL OR ISJSON(RespuestaJson) = 1),
    CONSTRAINT CK_AnalisisIA_Tokens CHECK ((TokensEntrada IS NULL OR TokensEntrada >= 0)
                                       AND (TokensSalida  IS NULL OR TokensSalida  >= 0)),
    -- Un analisis exitoso obliga a tener JSON y nivel de riesgo; uno fallido obliga a tener error.
    CONSTRAINT CK_AnalisisIA_Coher  CHECK ((Exitoso = 1 AND RespuestaJson IS NOT NULL AND NivelRiesgo IS NOT NULL)
                                        OR (Exitoso = 0 AND Error IS NOT NULL))
);
GO

/*  com.CorreoEnviado : auditoria de cada INTENTO de envio (ENVIADO / ERROR). Permite el reintento
    explicito y auditable exigido en CA-07. No almacena credenciales SMTP.                     */
CREATE TABLE com.CorreoEnviado
(
    IdCorreo            INT             IDENTITY(1,1) NOT NULL,
    IdReserva           INT             NOT NULL,
    TipoNotificacion    VARCHAR(20)     NOT NULL,
    Destinatario        NVARCHAR(150)   NOT NULL,
    Asunto              NVARCHAR(200)   NOT NULL,
    FechaIntento        DATETIME2(0)    NOT NULL CONSTRAINT DF_Correo_Fecha DEFAULT (SYSDATETIME()),
    Estado              VARCHAR(10)     NOT NULL,
    Error               NVARCHAR(500)   NULL,
    IdUsuario           INT             NULL,
    CONSTRAINT PK_CorreoEnviado     PRIMARY KEY CLUSTERED (IdCorreo),
    CONSTRAINT FK_Correo_Reserva    FOREIGN KEY (IdReserva) REFERENCES evt.Reserva (IdReserva) ON DELETE CASCADE,
    CONSTRAINT FK_Correo_Usuario    FOREIGN KEY (IdUsuario) REFERENCES seg.Usuario (IdUsuario),
    CONSTRAINT CK_Correo_Estado     CHECK (Estado IN ('ENVIADO', 'ERROR')),
    CONSTRAINT CK_Correo_Tipo       CHECK (TipoNotificacion IN ('CONFIRMACION', 'CANCELACION', 'REENVIO')),
    CONSTRAINT CK_Correo_Error      CHECK (Estado = 'ENVIADO' OR Error IS NOT NULL)
);
GO

/*==============================================================================================
  SECCION 06 : SECUENCIA Y TIPO TABLA (TVP)
==============================================================================================*/
/*  Secuencia para el codigo legible de reserva: RSV-<anio>-<consecutivo de 6 digitos>.
    Se usa una SEQUENCE y no MAX(Codigo)+1 para evitar condiciones de carrera.                 */
CREATE SEQUENCE evt.sq_Reserva AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 NO CYCLE;
GO

/*  evt.ReservaDetalleType : parametro tipo tabla que transporta TODO el detalle en una sola
    llamada. Es el mecanismo que permite guardar cabecera + N detalles dentro de una unica
    transaccion, en vez de enviar un INSERT por fila desde el formulario.                     */
CREATE TYPE evt.ReservaDetalleType AS TABLE
(
    IdRecurso           INT             NOT NULL,
    Cantidad            INT             NOT NULL,
    PrecioUnitario      DECIMAL(12,2)   NOT NULL,
    PorcentajeDescuento DECIMAL(5,2)    NOT NULL DEFAULT (0)
);
GO

/*==============================================================================================
  SECCION 07 : INDICES NO CLUSTERED
==============================================================================================*/
-- Soporta la deteccion de cruce de franja horaria por salon y fecha (evt.sp_Disponibilidad_Validar).
CREATE NONCLUSTERED INDEX IX_Reserva_Salon_Fecha
    ON evt.Reserva (IdSalon, FechaEvento, Estado)
    INCLUDE (IdReserva, HoraInicio, HoraFin, NumeroInvitados);
GO

-- Soporta el calculo de stock concurrente por recurso, fecha y franja horaria.
CREATE NONCLUSTERED INDEX IX_Reserva_Fecha_Estado
    ON evt.Reserva (FechaEvento, Estado)
    INCLUDE (IdReserva, HoraInicio, HoraFin);
GO

CREATE NONCLUSTERED INDEX IX_Reserva_Cliente
    ON evt.Reserva (IdCliente)
    INCLUDE (Codigo, FechaEvento, Estado, Total);
GO

CREATE NONCLUSTERED INDEX IX_Detalle_Recurso
    ON evt.ReservaDetalle (IdRecurso)
    INCLUDE (IdReserva, Cantidad);
GO

CREATE NONCLUSTERED INDEX IX_Cliente_Nombres   ON evt.Cliente (Nombres)  INCLUDE (Identificacion, Email, Estado);
GO
CREATE NONCLUSTERED INDEX IX_AnalisisIA_Reserva ON evt.AnalisisIA (IdReserva, Fecha DESC);
GO
CREATE NONCLUSTERED INDEX IX_Correo_Reserva     ON com.CorreoEnviado (IdReserva, FechaIntento DESC);
GO
CREATE NONCLUSTERED INDEX IX_ResAud_Reserva     ON evt.ReservaAuditoria (IdReserva, Fecha DESC);
GO

/*==============================================================================================
  SECCION 08 : DATOS SEMILLA
==============================================================================================*/
INSERT INTO seg.Rol (Nombre, Descripcion) VALUES
    (N'ADMINISTRADOR', N'Acceso total: catalogos, reservas, descuentos mayores al 10% y auditoria.'),
    (N'COORDINADOR',   N'Gestion operativa de reservas y consulta de catalogos.');
GO

/*  Usuarios semilla.
    Hash = PBKDF2-HMAC-SHA256(password, salt, 120000 iteraciones, 32 bytes de salida).
    Estas credenciales son de laboratorio y estan documentadas en el README; deben cambiarse
    en cualquier uso real. El texto plano NO se almacena en ningun lugar de la base.
        admin       / Admin123*    -> ADMINISTRADOR
        coordinador / Coord123*    -> COORDINADOR                                              */
INSERT INTO seg.Usuario (NombreUsuario, NombreCompleto, Email, PasswordHash, PasswordSalt, Iteraciones, Algoritmo, IdRol, Estado)
VALUES
    (N'admin', N'Josias Aguilar', N'admin@smartevent.local',
     0xD69ABAF7F22BF4FF747958A2CA8107A8102755A1C10BE7299F992406BC860539,
     0xA1B2C3D4E5F60718293A4B5C6D7E8F90,
     120000, 'PBKDF2-SHA256',
     (SELECT IdRol FROM seg.Rol WHERE Nombre = N'ADMINISTRADOR'), 1),
    (N'coordinador', N'Coordinador de Eventos', N'coordinador@smartevent.local',
     0x828E041E8F01358B4F8E16AB483A39CA2C87296D1528B2D7F8D8267D072B9A4B,
     0x0F1E2D3C4B5A69788796A5B4C3D2E1F0,
     120000, 'PBKDF2-SHA256',
     (SELECT IdRol FROM seg.Rol WHERE Nombre = N'COORDINADOR'), 1);
GO

INSERT INTO evt.TransicionEstado (EstadoOrigen, EstadoDestino, RequiereMotivo) VALUES
    ('BORRADOR',   'CONFIRMADA', 0),
    ('BORRADOR',   'CANCELADA',  1),
    ('CONFIRMADA', 'FINALIZADA', 0),
    ('CONFIRMADA', 'CANCELADA',  1);
GO

INSERT INTO evt.Cliente (Identificacion, Nombres, Email, Telefono, Estado) VALUES
    (N'0102030405', N'Corporacion Andina S.A.',      N'eventos@corpandina.com',    N'0991234567', 1),
    (N'0987654321', N'Fundacion Educar',             N'contacto@educar.org',       N'0987654321', 1),
    (N'1717171717', N'TecnoSoluciones Cia. Ltda.',   N'gerencia@tecnosol.com',     N'0961122334', 1),
    (N'1804567890', N'Maria Fernanda Vasquez',       N'mf.vasquez@correo.com',     N'0955566778', 1),
    (N'0923456781', N'Grupo Logistico del Pacifico', N'admin@logipacifico.com',    N'0944433221', 1);
GO

INSERT INTO evt.Salon (Nombre, Ubicacion, Capacidad, TarifaBase, Estado) VALUES
    (N'Salon Esmeralda', N'Piso 1 - Ala Norte',  120, 450.00, 1),
    (N'Salon Zafiro',    N'Piso 2 - Ala Sur',     60, 280.00, 1),
    (N'Salon Diamante',  N'Piso 3 - Panoramico', 300, 900.00, 1),
    (N'Sala Ejecutiva',  N'Piso 2 - Ala Norte',   20, 150.00, 1),
    (N'Terraza Jardin',  N'Azotea',              200, 700.00, 1);
GO

INSERT INTO evt.Recurso (Nombre, Tipo, StockTotal, PrecioUnitario, Estado) VALUES
    (N'Proyector Full HD',        'EQUIPO',     10,  35.00, 1),
    (N'Pantalla 120 pulgadas',    'EQUIPO',      6,  25.00, 1),
    (N'Microfono inalambrico',    'EQUIPO',     20,  12.50, 1),
    (N'Sistema de sonido 2000W',  'EQUIPO',      4,  90.00, 1),
    (N'Silla plegable acolchada', 'MOBILIARIO', 400,   1.75, 1),
    (N'Mesa redonda 10 puestos',  'MOBILIARIO',  40,   8.00, 1),
    (N'Servicio de coffee break', 'CATERING',   300,   4.50, 1),
    (N'Almuerzo ejecutivo',       'CATERING',   250,  12.00, 1),
    (N'Personal de logistica',    'SERVICIO',    15,  30.00, 1),
    (N'Transmision en vivo',      'SERVICIO',     2, 180.00, 1);
GO

PRINT '>> SECCION 08 completada: datos semilla insertados.';
GO

/*==============================================================================================
  SECCION 09 : PROCEDIMIENTOS DE SEGURIDAD

  Convención de errores de negocio: todos los errores controlados se lanzan con THROW usando
  números >= 50000 y un mensaje redactado para el usuario final. La capa de aplicación distingue
  ese rango del resto de errores del motor: los primeros se muestran tal cual, los demás se
  registran en el log y se reemplazan por un mensaje genérico (nunca se expone SQL ni cadenas
  de conexión). Rangos usados:
      51000-51099 : seguridad
      51100-51199 : catálogos
      51200-51299 : validación de reserva
      51300-51399 : disponibilidad y stock
      51400-51499 : transiciones de estado
==============================================================================================*/

/*----------------------------------------------------------------------------------------------
  seg.sp_Usuario_ObtenerParametrosHash
  Devuelve únicamente los parámetros públicos necesarios para derivar la clave (salt e
  iteraciones). El salt no es secreto por definición; el hash jamás sale de la base.
  Si el usuario no existe o está inactivo se devuelve un salt determinístico derivado del
  nombre para que la respuesta sea indistinguible y no se pueda enumerar usuarios válidos.
----------------------------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE seg.sp_Usuario_ObtenerParametrosHash
    @NombreUsuario NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Salt VARBINARY(32), @Iteraciones INT, @Algoritmo VARCHAR(20);

    SELECT  @Salt        = u.PasswordSalt,
            @Iteraciones = u.Iteraciones,
            @Algoritmo   = u.Algoritmo
    FROM    seg.Usuario AS u
    WHERE   u.NombreUsuario = @NombreUsuario
      AND   u.Estado = 1;

    IF @Salt IS NULL
    BEGIN
        -- Salt señuelo: mismo formato y coste, sin revelar si el usuario existe.
        SET @Salt        = CONVERT(VARBINARY(16), HASHBYTES('SHA2_256', CONCAT(N'smartevent::', @NombreUsuario)));
        SET @Iteraciones = 120000;
        SET @Algoritmo   = 'PBKDF2-SHA256';
    END

    SELECT  PasswordSalt = @Salt,
            Iteraciones  = @Iteraciones,
            Algoritmo    = @Algoritmo;
END
GO

/*----------------------------------------------------------------------------------------------
  seg.sp_Usuario_Autenticar
  Recibe el hash ya derivado por la aplicación (PBKDF2 con el salt e iteraciones obtenidos
  arriba) y realiza la comparación DENTRO del motor: el PasswordHash nunca viaja a la interfaz.
  Implementa además el bloqueo temporal por intentos fallidos, persistido en la base.

  @Resultado : 0 = autenticado
               1 = credenciales inválidas (o usuario inexistente/inactivo)
               2 = cuenta bloqueada temporalmente
  Devuelve el conjunto con los datos de sesión solo cuando @Resultado = 0.
----------------------------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE seg.sp_Usuario_Autenticar
    @NombreUsuario      NVARCHAR(50),
    @PasswordHash       VARBINARY(64),
    @Resultado          INT             OUTPUT,
    @Mensaje            NVARCHAR(200)   OUTPUT,
    @SegundosBloqueo    INT             OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @MaxIntentos    INT = 5,
            @MinutosBloqueo INT = 5,
            @Ahora          DATETIME2(0) = SYSDATETIME();

    DECLARE @IdUsuario      INT,
            @Estado         BIT,
            @HashReal       VARBINARY(64),
            @Intentos       INT,
            @BloqueadoHasta DATETIME2(0);

    SET @Resultado       = 1;
    SET @Mensaje         = N'Usuario o contraseña incorrectos.';
    SET @SegundosBloqueo = 0;

    SELECT  @IdUsuario      = u.IdUsuario,
            @Estado         = u.Estado,
            @HashReal       = u.PasswordHash,
            @Intentos       = u.IntentosFallidos,
            @BloqueadoHasta = u.BloqueadoHasta
    FROM    seg.Usuario AS u
    WHERE   u.NombreUsuario = @NombreUsuario;

    -- Usuario inexistente o inactivo: mensaje genérico, sin revelar cuál de los dos casos es.
    IF @IdUsuario IS NULL OR @Estado = 0
        RETURN 0;

    IF @BloqueadoHasta IS NOT NULL AND @BloqueadoHasta > @Ahora
    BEGIN
        SET @Resultado       = 2;
        SET @SegundosBloqueo = DATEDIFF(SECOND, @Ahora, @BloqueadoHasta);
        SET @Mensaje         = N'Cuenta bloqueada temporalmente. Intente nuevamente en '
                             + CAST(((@SegundosBloqueo / 60) + 1) AS NVARCHAR(10)) + N' minuto(s).';
        RETURN 0;
    END

    IF @HashReal = @PasswordHash
    BEGIN
        UPDATE  seg.Usuario
        SET     IntentosFallidos = 0,
                BloqueadoHasta   = NULL,
                UltimoAcceso     = @Ahora
        WHERE   IdUsuario = @IdUsuario;

        SET @Resultado = 0;
        SET @Mensaje   = N'Autenticación correcta.';

        SELECT  u.IdUsuario,
                u.NombreUsuario,
                u.NombreCompleto,
                u.Email,
                u.IdRol,
                Rol = r.Nombre,
                u.UltimoAcceso
        FROM    seg.Usuario AS u
                INNER JOIN seg.Rol AS r ON r.IdRol = u.IdRol
        WHERE   u.IdUsuario = @IdUsuario;

        RETURN 0;
    END

    -- Credenciales incorrectas: se incrementa el contador y, al llegar al máximo, se bloquea.
    SET @Intentos = ISNULL(@Intentos, 0) + 1;

    IF @Intentos >= @MaxIntentos
    BEGIN
        SET @BloqueadoHasta  = DATEADD(MINUTE, @MinutosBloqueo, @Ahora);
        SET @Resultado       = 2;
        SET @SegundosBloqueo = @MinutosBloqueo * 60;
        SET @Mensaje         = N'Demasiados intentos fallidos. Cuenta bloqueada por '
                             + CAST(@MinutosBloqueo AS NVARCHAR(10)) + N' minutos.';

        UPDATE  seg.Usuario
        SET     IntentosFallidos = 0,
                BloqueadoHasta   = @BloqueadoHasta
        WHERE   IdUsuario = @IdUsuario;
    END
    ELSE
    BEGIN
        SET @Mensaje = N'Usuario o contraseña incorrectos. Intento '
                     + CAST(@Intentos AS NVARCHAR(10)) + N' de ' + CAST(@MaxIntentos AS NVARCHAR(10)) + N'.';

        UPDATE  seg.Usuario
        SET     IntentosFallidos = @Intentos
        WHERE   IdUsuario = @IdUsuario;
    END

    RETURN 0;
END
GO

/*----------------------------------------------------------------------------------------------
  seg.sp_Usuario_Consultar : lista de usuarios para administración. Nunca proyecta el hash.
----------------------------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE seg.sp_Usuario_Consultar
    @Filtro NVARCHAR(100) = NULL,
    @Estado BIT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  u.IdUsuario,
            u.NombreUsuario,
            u.NombreCompleto,
            u.Email,
            Rol = r.Nombre,
            u.Estado,
            u.UltimoAcceso,
            u.FechaCreacion,
            Bloqueado = CASE WHEN u.BloqueadoHasta IS NOT NULL AND u.BloqueadoHasta > SYSDATETIME()
                             THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
    FROM    seg.Usuario AS u
            INNER JOIN seg.Rol AS r ON r.IdRol = u.IdRol
    WHERE   (@Filtro IS NULL OR u.NombreUsuario LIKE N'%' + @Filtro + N'%'
                             OR u.NombreCompleto LIKE N'%' + @Filtro + N'%')
      AND   (@Estado IS NULL OR u.Estado = @Estado)
    ORDER BY u.NombreUsuario;
END
GO

/*==============================================================================================
  SECCION 10 : PROCEDIMIENTOS DE CATÁLOGOS (Cliente / Salón / Recurso)
  Patrón común: un SP _Guardar que inserta o actualiza según @Id, un _Consultar con filtros
  opcionales combinables (sin concatenar SQL) y un _CambiarEstado para la inactivación lógica.
==============================================================================================*/

CREATE OR ALTER PROCEDURE evt.sp_Cliente_Guardar
    @IdCliente      INT             = NULL,
    @Identificacion NVARCHAR(20),
    @Nombres        NVARCHAR(150),
    @Email          NVARCHAR(150),
    @Telefono       NVARCHAR(20)    = NULL,
    @Estado         BIT             = 1,
    @IdClienteOut   INT             OUTPUT,
    @Mensaje        NVARCHAR(200)   OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Identificacion = LTRIM(RTRIM(@Identificacion));
    SET @Nombres        = LTRIM(RTRIM(@Nombres));
    SET @Email          = LTRIM(RTRIM(@Email));

    IF EXISTS (SELECT 1 FROM evt.Cliente
               WHERE Identificacion = @Identificacion
                 AND (@IdCliente IS NULL OR IdCliente <> @IdCliente))
        THROW 51101, N'Ya existe un cliente registrado con esa identificación.', 1;

    IF @IdCliente IS NULL
    BEGIN
        INSERT INTO evt.Cliente (Identificacion, Nombres, Email, Telefono, Estado)
        VALUES (@Identificacion, @Nombres, @Email, @Telefono, @Estado);

        SET @IdClienteOut = CAST(SCOPE_IDENTITY() AS INT);
        SET @Mensaje      = N'Cliente registrado correctamente.';
    END
    ELSE
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM evt.Cliente WHERE IdCliente = @IdCliente)
            THROW 51102, N'El cliente indicado no existe.', 1;

        UPDATE  evt.Cliente
        SET     Identificacion    = @Identificacion,
                Nombres           = @Nombres,
                Email             = @Email,
                Telefono          = @Telefono,
                Estado            = @Estado,
                FechaModificacion = SYSDATETIME()
        WHERE   IdCliente = @IdCliente;

        SET @IdClienteOut = @IdCliente;
        SET @Mensaje      = N'Cliente actualizado correctamente.';
    END
END
GO

CREATE OR ALTER PROCEDURE evt.sp_Cliente_Consultar
    @IdCliente INT           = NULL,
    @Filtro    NVARCHAR(100) = NULL,
    @Estado    BIT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  c.IdCliente, c.Identificacion, c.Nombres, c.Email, c.Telefono,
            c.Estado, c.FechaCreacion, c.FechaModificacion
    FROM    evt.Cliente AS c
    WHERE   (@IdCliente IS NULL OR c.IdCliente = @IdCliente)
      AND   (@Filtro IS NULL OR c.Nombres LIKE N'%' + @Filtro + N'%'
                             OR c.Identificacion LIKE N'%' + @Filtro + N'%'
                             OR c.Email LIKE N'%' + @Filtro + N'%')
      AND   (@Estado IS NULL OR c.Estado = @Estado)
    ORDER BY c.Nombres;
END
GO

CREATE OR ALTER PROCEDURE evt.sp_Cliente_CambiarEstado
    @IdCliente INT,
    @Estado    BIT,
    @Mensaje   NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM evt.Cliente WHERE IdCliente = @IdCliente)
        THROW 51102, N'El cliente indicado no existe.', 1;

    -- Inactivación lógica: no se elimina información histórica de reservas.
    IF @Estado = 0 AND EXISTS (SELECT 1 FROM evt.Reserva
                               WHERE IdCliente = @IdCliente AND Estado IN ('BORRADOR', 'CONFIRMADA'))
        THROW 51103, N'No se puede inactivar el cliente: tiene reservas en borrador o confirmadas.', 1;

    UPDATE  evt.Cliente
    SET     Estado = @Estado, FechaModificacion = SYSDATETIME()
    WHERE   IdCliente = @IdCliente;

    SET @Mensaje = CASE WHEN @Estado = 1 THEN N'Cliente activado.' ELSE N'Cliente inactivado.' END;
END
GO

CREATE OR ALTER PROCEDURE evt.sp_Salon_Guardar
    @IdSalon    INT             = NULL,
    @Nombre     NVARCHAR(100),
    @Ubicacion  NVARCHAR(150)   = NULL,
    @Capacidad  INT,
    @TarifaBase DECIMAL(12,2),
    @Estado     BIT             = 1,
    @IdSalonOut INT             OUTPUT,
    @Mensaje    NVARCHAR(200)   OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Nombre = LTRIM(RTRIM(@Nombre));

    IF EXISTS (SELECT 1 FROM evt.Salon WHERE Nombre = @Nombre AND (@IdSalon IS NULL OR IdSalon <> @IdSalon))
        THROW 51111, N'Ya existe un salón registrado con ese nombre.', 1;

    IF @IdSalon IS NULL
    BEGIN
        INSERT INTO evt.Salon (Nombre, Ubicacion, Capacidad, TarifaBase, Estado)
        VALUES (@Nombre, @Ubicacion, @Capacidad, @TarifaBase, @Estado);

        SET @IdSalonOut = CAST(SCOPE_IDENTITY() AS INT);
        SET @Mensaje    = N'Salón registrado correctamente.';
    END
    ELSE
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM evt.Salon WHERE IdSalon = @IdSalon)
            THROW 51112, N'El salón indicado no existe.', 1;

        -- No se permite reducir la capacidad por debajo de reservas vigentes ya comprometidas.
        IF EXISTS (SELECT 1 FROM evt.Reserva
                   WHERE IdSalon = @IdSalon
                     AND Estado IN ('BORRADOR', 'CONFIRMADA')
                     AND NumeroInvitados > @Capacidad)
            THROW 51113, N'La nueva capacidad es menor que el número de invitados de reservas vigentes.', 1;

        UPDATE  evt.Salon
        SET     Nombre = @Nombre, Ubicacion = @Ubicacion, Capacidad = @Capacidad,
                TarifaBase = @TarifaBase, Estado = @Estado, FechaModificacion = SYSDATETIME()
        WHERE   IdSalon = @IdSalon;

        SET @IdSalonOut = @IdSalon;
        SET @Mensaje    = N'Salón actualizado correctamente.';
    END
END
GO

CREATE OR ALTER PROCEDURE evt.sp_Salon_Consultar
    @IdSalon         INT           = NULL,
    @Filtro          NVARCHAR(100) = NULL,
    @Estado          BIT           = NULL,
    @CapacidadMinima INT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  s.IdSalon, s.Nombre, s.Ubicacion, s.Capacidad, s.TarifaBase,
            s.Estado, s.FechaCreacion, s.FechaModificacion
    FROM    evt.Salon AS s
    WHERE   (@IdSalon IS NULL OR s.IdSalon = @IdSalon)
      AND   (@Filtro IS NULL OR s.Nombre LIKE N'%' + @Filtro + N'%'
                             OR s.Ubicacion LIKE N'%' + @Filtro + N'%')
      AND   (@Estado IS NULL OR s.Estado = @Estado)
      AND   (@CapacidadMinima IS NULL OR s.Capacidad >= @CapacidadMinima)
    ORDER BY s.Nombre;
END
GO

CREATE OR ALTER PROCEDURE evt.sp_Salon_CambiarEstado
    @IdSalon INT,
    @Estado  BIT,
    @Mensaje NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM evt.Salon WHERE IdSalon = @IdSalon)
        THROW 51112, N'El salón indicado no existe.', 1;

    IF @Estado = 0 AND EXISTS (SELECT 1 FROM evt.Reserva
                               WHERE IdSalon = @IdSalon AND Estado IN ('BORRADOR', 'CONFIRMADA'))
        THROW 51114, N'No se puede inactivar el salón: tiene reservas en borrador o confirmadas.', 1;

    UPDATE  evt.Salon
    SET     Estado = @Estado, FechaModificacion = SYSDATETIME()
    WHERE   IdSalon = @IdSalon;

    SET @Mensaje = CASE WHEN @Estado = 1 THEN N'Salón activado.' ELSE N'Salón inactivado.' END;
END
GO

CREATE OR ALTER PROCEDURE evt.sp_Recurso_Guardar
    @IdRecurso      INT             = NULL,
    @Nombre         NVARCHAR(100),
    @Tipo           VARCHAR(20),
    @StockTotal     INT,
    @PrecioUnitario DECIMAL(12,2),
    @Estado         BIT             = 1,
    @IdRecursoOut   INT             OUTPUT,
    @Mensaje        NVARCHAR(200)   OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Nombre = LTRIM(RTRIM(@Nombre));

    IF EXISTS (SELECT 1 FROM evt.Recurso WHERE Nombre = @Nombre AND (@IdRecurso IS NULL OR IdRecurso <> @IdRecurso))
        THROW 51121, N'Ya existe un recurso registrado con ese nombre.', 1;

    IF @IdRecurso IS NULL
    BEGIN
        INSERT INTO evt.Recurso (Nombre, Tipo, StockTotal, PrecioUnitario, Estado)
        VALUES (@Nombre, @Tipo, @StockTotal, @PrecioUnitario, @Estado);

        SET @IdRecursoOut = CAST(SCOPE_IDENTITY() AS INT);
        SET @Mensaje      = N'Recurso registrado correctamente.';
    END
    ELSE
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM evt.Recurso WHERE IdRecurso = @IdRecurso)
            THROW 51122, N'El recurso indicado no existe.', 1;

        -- El stock no puede quedar por debajo de la mayor cantidad ya comprometida en un momento dado.
        DECLARE @MaximoComprometido INT;

        SELECT  @MaximoComprometido = MAX(x.Comprometido)
        FROM   (SELECT  Comprometido = SUM(d.Cantidad)
                FROM    evt.ReservaDetalle AS d
                        INNER JOIN evt.Reserva AS r ON r.IdReserva = d.IdReserva
                WHERE   d.IdRecurso = @IdRecurso
                  AND   r.Estado IN ('BORRADOR', 'CONFIRMADA')
                GROUP BY r.FechaEvento, r.HoraInicio) AS x;

        IF @MaximoComprometido IS NOT NULL AND @StockTotal < @MaximoComprometido
            THROW 51123, N'El stock indicado es menor que la cantidad ya comprometida en reservas vigentes.', 1;

        UPDATE  evt.Recurso
        SET     Nombre = @Nombre, Tipo = @Tipo, StockTotal = @StockTotal,
                PrecioUnitario = @PrecioUnitario, Estado = @Estado, FechaModificacion = SYSDATETIME()
        WHERE   IdRecurso = @IdRecurso;

        SET @IdRecursoOut = @IdRecurso;
        SET @Mensaje      = N'Recurso actualizado correctamente.';
    END
END
GO

CREATE OR ALTER PROCEDURE evt.sp_Recurso_Consultar
    @IdRecurso INT           = NULL,
    @Filtro    NVARCHAR(100) = NULL,
    @Tipo      VARCHAR(20)   = NULL,
    @Estado    BIT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  r.IdRecurso, r.Nombre, r.Tipo, r.StockTotal, r.PrecioUnitario,
            r.Estado, r.FechaCreacion, r.FechaModificacion
    FROM    evt.Recurso AS r
    WHERE   (@IdRecurso IS NULL OR r.IdRecurso = @IdRecurso)
      AND   (@Filtro IS NULL OR r.Nombre LIKE N'%' + @Filtro + N'%')
      AND   (@Tipo IS NULL OR r.Tipo = @Tipo)
      AND   (@Estado IS NULL OR r.Estado = @Estado)
    ORDER BY r.Tipo, r.Nombre;
END
GO

CREATE OR ALTER PROCEDURE evt.sp_Recurso_CambiarEstado
    @IdRecurso INT,
    @Estado    BIT,
    @Mensaje   NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM evt.Recurso WHERE IdRecurso = @IdRecurso)
        THROW 51122, N'El recurso indicado no existe.', 1;

    IF @Estado = 0 AND EXISTS (SELECT 1 FROM evt.ReservaDetalle AS d
                               INNER JOIN evt.Reserva AS r ON r.IdReserva = d.IdReserva
                               WHERE d.IdRecurso = @IdRecurso AND r.Estado IN ('BORRADOR', 'CONFIRMADA'))
        THROW 51124, N'No se puede inactivar el recurso: está comprometido en reservas vigentes.', 1;

    UPDATE  evt.Recurso
    SET     Estado = @Estado, FechaModificacion = SYSDATETIME()
    WHERE   IdRecurso = @IdRecurso;

    SET @Mensaje = CASE WHEN @Estado = 1 THEN N'Recurso activado.' ELSE N'Recurso inactivado.' END;
END
GO

/*==============================================================================================
  SECCION 11 : DISPONIBILIDAD

  evt.sp_Disponibilidad_Validar
  Única fuente de verdad para las tres validaciones que dependen del estado global de la base:
      1. Cruce de franja horaria del salón (regla: inicioNuevo < finExistente AND finNuevo > inicioExistente)
      2. Capacidad del salón frente al número de invitados
      3. Stock concurrente de cada recurso en la misma fecha y franja horaria

  @IdReserva permite EXCLUIR la reserva que se está editando, de modo que una reserva en
  BORRADOR nunca se detecta a sí misma como conflicto (CA-04).

  @Silencioso = 0 -> además de los OUTPUT devuelve el conjunto de conflictos para mostrarlo
                     en el formulario antes de guardar.
  @Silencioso = 1 -> uso interno desde evt.sp_Reserva_Guardar / evt.sp_Reserva_CambiarEstado:
                     no emite conjuntos de resultados adicionales.
==============================================================================================*/
CREATE OR ALTER PROCEDURE evt.sp_Disponibilidad_Validar
    @IdReserva          INT = NULL,
    @IdSalon            INT,
    @FechaEvento        DATE,
    @HoraInicio         TIME(0),
    @HoraFin            TIME(0),
    @NumeroInvitados    INT,
    @Detalle            evt.ReservaDetalleType READONLY,
    @EsValido           BIT             OUTPUT,
    @Mensaje            NVARCHAR(400)   OUTPUT,
    @Silencioso         BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Conflictos TABLE
    (
        Orden       INT IDENTITY(1,1),
        Tipo        VARCHAR(20)     NOT NULL,
        Referencia  NVARCHAR(100)   NULL,
        Detalle     NVARCHAR(300)   NOT NULL
    );

    DECLARE @Capacidad INT, @SalonNombre NVARCHAR(100), @SalonEstado BIT;

    SELECT  @Capacidad   = s.Capacidad,
            @SalonNombre = s.Nombre,
            @SalonEstado = s.Estado
    FROM    evt.Salon AS s
    WHERE   s.IdSalon = @IdSalon;

    /*-- 1. Salón ------------------------------------------------------------------------------*/
    IF @Capacidad IS NULL
        INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
        VALUES ('SALON', NULL, N'El salón seleccionado no existe.');
    ELSE IF @SalonEstado = 0
        INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
        VALUES ('SALON', @SalonNombre, N'El salón seleccionado está inactivo.');

    /*-- 2. Horario ----------------------------------------------------------------------------*/
    IF @HoraFin <= @HoraInicio
        INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
        VALUES ('HORARIO', NULL, N'La hora de fin debe ser posterior a la hora de inicio.');
    ELSE IF DATEDIFF(MINUTE, @HoraInicio, @HoraFin) < 120
        INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
        VALUES ('HORARIO', NULL, N'La duración mínima de un evento es de 2 horas.');
    ELSE IF DATEDIFF(MINUTE, @HoraInicio, @HoraFin) > 720
        INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
        VALUES ('HORARIO', NULL, N'La duración máxima de un evento es de 12 horas.');

    /*-- 3. Capacidad --------------------------------------------------------------------------*/
    IF @NumeroInvitados IS NULL OR @NumeroInvitados <= 0
        INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
        VALUES ('CAPACIDAD', NULL, N'El número de invitados debe ser mayor que cero.');
    ELSE IF @Capacidad IS NOT NULL AND @NumeroInvitados > @Capacidad
        INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
        VALUES ('CAPACIDAD', @SalonNombre,
                N'El número de invitados (' + CAST(@NumeroInvitados AS NVARCHAR(10))
                + N') supera la capacidad del salón (' + CAST(@Capacidad AS NVARCHAR(10)) + N').');

    /*-- 4. Cruce de franja horaria en el mismo salón y fecha -----------------------------------*/
    INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
    SELECT  'CRUCE',
            r.Codigo,
            N'El salón ya está ocupado por la reserva ' + r.Codigo + N' de '
            + CONVERT(NVARCHAR(5), r.HoraInicio, 108) + N' a ' + CONVERT(NVARCHAR(5), r.HoraFin, 108)
            + N' (' + r.Estado + N').'
    FROM    evt.Reserva AS r
    WHERE   r.IdSalon     = @IdSalon
      AND   r.FechaEvento = @FechaEvento
      AND   r.Estado IN ('BORRADOR', 'CONFIRMADA')
      AND   (@IdReserva IS NULL OR r.IdReserva <> @IdReserva)   -- CA-04: se excluye a sí misma
      AND   @HoraInicio < r.HoraFin
      AND   @HoraFin    > r.HoraInicio;

    /*-- 5. Detalle: existencia, duplicados, cantidades y precios -------------------------------*/
    IF NOT EXISTS (SELECT 1 FROM @Detalle)
        INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
        VALUES ('DETALLE', NULL, N'La reserva debe tener al menos un recurso o servicio.');

    INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
    SELECT  'DETALLE', CAST(d.IdRecurso AS NVARCHAR(20)),
            N'El recurso aparece más de una vez en el detalle de la reserva.'
    FROM    @Detalle AS d
    GROUP BY d.IdRecurso
    HAVING  COUNT(*) > 1;

    INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
    SELECT  'RECURSO', CAST(d.IdRecurso AS NVARCHAR(20)),
            N'El recurso indicado no existe o está inactivo.'
    FROM   (SELECT DISTINCT IdRecurso FROM @Detalle) AS d
    WHERE  NOT EXISTS (SELECT 1 FROM evt.Recurso AS rc WHERE rc.IdRecurso = d.IdRecurso AND rc.Estado = 1);

    INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
    SELECT  'CANTIDAD', rc.Nombre, N'La cantidad del recurso ' + rc.Nombre + N' debe ser mayor que cero.'
    FROM    @Detalle AS d
            INNER JOIN evt.Recurso AS rc ON rc.IdRecurso = d.IdRecurso
    WHERE   d.Cantidad <= 0;

    INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
    SELECT  'PRECIO', rc.Nombre, N'El precio unitario del recurso ' + rc.Nombre + N' no puede ser negativo.'
    FROM    @Detalle AS d
            INNER JOIN evt.Recurso AS rc ON rc.IdRecurso = d.IdRecurso
    WHERE   d.PrecioUnitario < 0;

    INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
    SELECT  'DESCUENTO', rc.Nombre,
            N'El descuento de línea del recurso ' + rc.Nombre + N' debe estar entre 0 y 20 por ciento.'
    FROM    @Detalle AS d
            INNER JOIN evt.Recurso AS rc ON rc.IdRecurso = d.IdRecurso
    WHERE   d.PorcentajeDescuento < 0 OR d.PorcentajeDescuento > 20;

    /*-- 6. Stock concurrente -------------------------------------------------------------------
        Comprometido = suma de cantidades de OTRAS reservas vigentes (BORRADOR/CONFIRMADA) del
        mismo recurso, en la misma fecha, cuya franja se cruza con la solicitada.               */
    INSERT INTO @Conflictos (Tipo, Referencia, Detalle)
    SELECT  'STOCK', rc.Nombre,
            N'Stock insuficiente de ' + rc.Nombre + N': solicita ' + CAST(sol.Solicitado AS NVARCHAR(10))
            + N', comprometido ' + CAST(ISNULL(ocp.Comprometido, 0) AS NVARCHAR(10))
            + N', disponible ' + CAST(rc.StockTotal - ISNULL(ocp.Comprometido, 0) AS NVARCHAR(10))
            + N' de ' + CAST(rc.StockTotal AS NVARCHAR(10)) + N'.'
    FROM   (SELECT IdRecurso, Solicitado = SUM(Cantidad) FROM @Detalle GROUP BY IdRecurso) AS sol
            INNER JOIN evt.Recurso AS rc ON rc.IdRecurso = sol.IdRecurso
            OUTER APPLY
            (
                SELECT  Comprometido = SUM(dd.Cantidad)
                FROM    evt.ReservaDetalle AS dd
                        INNER JOIN evt.Reserva AS rr ON rr.IdReserva = dd.IdReserva
                WHERE   dd.IdRecurso  = sol.IdRecurso
                  AND   rr.FechaEvento = @FechaEvento
                  AND   rr.Estado IN ('BORRADOR', 'CONFIRMADA')
                  AND   (@IdReserva IS NULL OR rr.IdReserva <> @IdReserva)
                  AND   @HoraInicio < rr.HoraFin
                  AND   @HoraFin    > rr.HoraInicio
            ) AS ocp
    WHERE   sol.Solicitado + ISNULL(ocp.Comprometido, 0) > rc.StockTotal;

    /*-- Resultado ------------------------------------------------------------------------------*/
    IF EXISTS (SELECT 1 FROM @Conflictos)
    BEGIN
        SET @EsValido = 0;
        SELECT  @Mensaje = LEFT(STRING_AGG(c.Detalle, N' ') WITHIN GROUP (ORDER BY c.Orden), 400)
        FROM    @Conflictos AS c;
    END
    ELSE
    BEGIN
        SET @EsValido = 1;
        SET @Mensaje  = N'Salón y recursos disponibles para la fecha y horario indicados.';
    END

    IF @Silencioso = 0
        SELECT Tipo, Referencia, Detalle FROM @Conflictos ORDER BY Orden;
END
GO

/*==============================================================================================
  SECCION 12 : RESERVAS

  evt.sp_Reserva_Guardar
  ------------------------------------------------------------------------------------------
  TRANSACCIÓN ATÓMICA cabecera + detalle. Todo el detalle llega en UNA sola llamada mediante
  el parámetro tipo tabla evt.ReservaDetalleType; no existe un INSERT por fila desde el
  formulario. Si cualquier línea falla (recurso inexistente, cantidad inválida, violación de
  CHECK/FK, stock insuficiente) se ejecuta ROLLBACK completo: no quedan cabeceras huérfanas ni
  detalles parciales (CA-02).

  Los importes NO se aceptan desde la aplicación: se recalculan aquí y se persisten.
      Subtotal  = TarifaBase del salón + SUM(SubtotalLinea)   -- SubtotalLinea es columna calculada
      BaseNeta  = Subtotal - Descuento global
      Impuesto  = 15% de BaseNeta
      Total     = BaseNeta + Impuesto
==============================================================================================*/
CREATE OR ALTER PROCEDURE evt.sp_Reserva_Guardar
    @IdReserva          INT             = NULL,
    @IdCliente          INT,
    @IdSalon            INT,
    @FechaEvento        DATE,
    @HoraInicio         TIME(0),
    @HoraFin            TIME(0),
    @NumeroInvitados    INT,
    @Descuento          DECIMAL(12,2)   = 0,
    @Observacion        NVARCHAR(500)   = NULL,
    @IdUsuario          INT,
    @Detalle            evt.ReservaDetalleType READONLY,
    @IdReservaOut       INT             OUTPUT,
    @CodigoOut          NVARCHAR(20)    OUTPUT,
    @Mensaje            NVARCHAR(400)   OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @TasaImpuesto   DECIMAL(6,4) = 0.1500;   -- 15% exigido por la regla de negocio
    DECLARE @Rol            NVARCHAR(30),
            @EstadoActual   VARCHAR(12),
            @TarifaBase     DECIMAL(12,2),
            @Subtotal       DECIMAL(12,2),
            @BaseNeta       DECIMAL(12,2),
            @Impuesto       DECIMAL(12,2),
            @Total          DECIMAL(12,2),
            @EsValido       BIT,
            @MensajeDisp    NVARCHAR(400),
            @EsNueva        BIT = CASE WHEN @IdReserva IS NULL THEN 1 ELSE 0 END;

    /*-- Validaciones previas a la transacción --------------------------------------------------*/
    SELECT  @Rol = r.Nombre
    FROM    seg.Usuario AS u
            INNER JOIN seg.Rol AS r ON r.IdRol = u.IdRol
    WHERE   u.IdUsuario = @IdUsuario AND u.Estado = 1;

    IF @Rol IS NULL
        THROW 51200, N'La sesión del usuario no es válida. Vuelva a iniciar sesión.', 1;

    IF NOT EXISTS (SELECT 1 FROM evt.Cliente WHERE IdCliente = @IdCliente AND Estado = 1)
        THROW 51202, N'El cliente seleccionado no existe o está inactivo.', 1;

    -- Solo ADMINISTRADOR puede autorizar descuentos de línea superiores al 10 por ciento.
    IF @Rol <> N'ADMINISTRADOR' AND EXISTS (SELECT 1 FROM @Detalle WHERE PorcentajeDescuento > 10)
        THROW 51203, N'Solo un ADMINISTRADOR puede aplicar descuentos de línea superiores al 10 por ciento.', 1;

    IF @Descuento IS NULL OR @Descuento < 0
        THROW 51204, N'El descuento global no puede ser negativo.', 1;

    IF @EsNueva = 0
    BEGIN
        SELECT @EstadoActual = Estado FROM evt.Reserva WHERE IdReserva = @IdReserva;

        IF @EstadoActual IS NULL
            THROW 51205, N'La reserva que intenta editar no existe.', 1;

        -- Una reserva CONFIRMADA/FINALIZADA/CANCELADA no puede modificar cliente, salón,
        -- fecha, horario ni detalles. Solo puede cambiar de estado.
        IF @EstadoActual <> 'BORRADOR'
            THROW 51206, N'Solo las reservas en estado BORRADOR pueden modificarse. Esta reserva ya fue confirmada, finalizada o cancelada.', 1;
    END

    /*-- Transacción atómica --------------------------------------------------------------------*/
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Disponibilidad evaluada DENTRO de la transacción: aunque la UI omita la validación
        -- visual, el rechazo se produce igualmente desde SQL Server (CA-03 / CA-05).
        EXEC evt.sp_Disponibilidad_Validar
                @IdReserva       = @IdReserva,
                @IdSalon         = @IdSalon,
                @FechaEvento     = @FechaEvento,
                @HoraInicio      = @HoraInicio,
                @HoraFin         = @HoraFin,
                @NumeroInvitados = @NumeroInvitados,
                @Detalle         = @Detalle,
                @EsValido        = @EsValido    OUTPUT,
                @Mensaje         = @MensajeDisp OUTPUT,
                @Silencioso      = 1;

        IF @EsValido = 0
            THROW 51300, @MensajeDisp, 1;

        SELECT @TarifaBase = TarifaBase FROM evt.Salon WHERE IdSalon = @IdSalon;

        /*-- Cabecera ---------------------------------------------------------------------------*/
        IF @EsNueva = 1
        BEGIN
            SET @CodigoOut = N'RSV-' + CAST(YEAR(@FechaEvento) AS NVARCHAR(4)) + N'-'
                           + RIGHT(N'000000' + CAST(NEXT VALUE FOR evt.sq_Reserva AS NVARCHAR(10)), 6);

            INSERT INTO evt.Reserva
                (Codigo, IdCliente, IdSalon, FechaEvento, HoraInicio, HoraFin, NumeroInvitados,
                 Estado, Subtotal, Descuento, Impuesto, Total, Observacion, IdUsuarioCreacion)
            VALUES
                (@CodigoOut, @IdCliente, @IdSalon, @FechaEvento, @HoraInicio, @HoraFin, @NumeroInvitados,
                 'BORRADOR', 0, 0, 0, 0, @Observacion, @IdUsuario);

            SET @IdReservaOut = CAST(SCOPE_IDENTITY() AS INT);

            INSERT INTO evt.ReservaAuditoria (IdReserva, EstadoAnterior, EstadoNuevo, Motivo, IdUsuario)
            VALUES (@IdReservaOut, NULL, 'BORRADOR', N'Creación de la reserva.', @IdUsuario);
        END
        ELSE
        BEGIN
            SET @IdReservaOut = @IdReserva;

            UPDATE  evt.Reserva
            SET     IdCliente             = @IdCliente,
                    IdSalon               = @IdSalon,
                    FechaEvento           = @FechaEvento,
                    HoraInicio            = @HoraInicio,
                    HoraFin               = @HoraFin,
                    NumeroInvitados       = @NumeroInvitados,
                    Observacion           = @Observacion,
                    IdUsuarioModificacion = @IdUsuario,
                    FechaModificacion     = SYSDATETIME()
            WHERE   IdReserva = @IdReserva
              AND   Estado    = 'BORRADOR';   -- concurrencia: si otro usuario la confirmó, no afecta filas

            IF @@ROWCOUNT = 0
                THROW 51207, N'La reserva fue modificada por otro usuario. Vuelva a cargarla antes de guardar.', 1;

            SELECT @CodigoOut = Codigo FROM evt.Reserva WHERE IdReserva = @IdReserva;
        END

        /*-- Detalle: sincronización completa dentro de la MISMA transacción ---------------------*/
        DELETE FROM evt.ReservaDetalle WHERE IdReserva = @IdReservaOut;

        INSERT INTO evt.ReservaDetalle (IdReserva, IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento)
        SELECT  @IdReservaOut, d.IdRecurso, d.Cantidad, d.PrecioUnitario, d.PorcentajeDescuento
        FROM    @Detalle AS d;

        IF @@ROWCOUNT = 0
            THROW 51208, N'La reserva debe tener al menos un recurso o servicio.', 1;

        /*-- Recálculo de importes: SQL Server es la fuente de verdad ----------------------------*/
        SELECT  @Subtotal = @TarifaBase + ISNULL(SUM(d.SubtotalLinea), 0)
        FROM    evt.ReservaDetalle AS d
        WHERE   d.IdReserva = @IdReservaOut;

        IF @Descuento > @Subtotal
            THROW 51209, N'El descuento global no puede superar el subtotal de la reserva.', 1;

        SET @BaseNeta = CAST(@Subtotal - @Descuento AS DECIMAL(12,2));
        SET @Impuesto = CAST(ROUND(@BaseNeta * @TasaImpuesto, 2) AS DECIMAL(12,2));
        SET @Total    = CAST(@BaseNeta + @Impuesto AS DECIMAL(12,2));

        UPDATE  evt.Reserva
        SET     Subtotal  = @Subtotal,
                Descuento = @Descuento,
                Impuesto  = @Impuesto,
                Total     = @Total
        WHERE   IdReserva = @IdReservaOut;

        COMMIT TRANSACTION;

        SET @Mensaje = CASE WHEN @EsNueva = 1
                            THEN N'Reserva ' + @CodigoOut + N' creada correctamente. Total: ' + CAST(@Total AS NVARCHAR(20)) + N'.'
                            ELSE N'Reserva ' + @CodigoOut + N' actualizada correctamente. Total: ' + CAST(@Total AS NVARCHAR(20)) + N'.'
                       END;
    END TRY
    BEGIN CATCH
        -- Cualquier fallo (validación de negocio, CHECK, FK o error del motor) revierte TODO.
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        SET @IdReservaOut = NULL;
        SET @CodigoOut    = NULL;
        THROW;   -- se conserva número y mensaje originales para que la capa de aplicación decida
    END CATCH
END
GO

/*----------------------------------------------------------------------------------------------
  evt.sp_Reserva_Consultar
  Filtros opcionales combinables sin concatenar SQL: cada predicado se anula con IS NULL.
  Incluye paginación por OFFSET/FETCH y devuelve el total de registros para la UI.
----------------------------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE evt.sp_Reserva_Consultar
    @Codigo         NVARCHAR(20)  = NULL,
    @IdCliente      INT           = NULL,
    @TextoCliente   NVARCHAR(100) = NULL,
    @FechaDesde     DATE          = NULL,
    @FechaHasta     DATE          = NULL,
    @IdSalon        INT           = NULL,
    @Estado         VARCHAR(12)   = NULL,
    @Pagina         INT           = 1,
    @TamanoPagina   INT           = 50,
    @TotalRegistros INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina IS NULL OR @Pagina < 1              SET @Pagina = 1;
    IF @TamanoPagina IS NULL OR @TamanoPagina < 1  SET @TamanoPagina = 50;
    IF @TamanoPagina > 500                         SET @TamanoPagina = 500;

    SELECT  @TotalRegistros = COUNT(*)
    FROM    evt.Reserva AS r
            INNER JOIN evt.Cliente AS c ON c.IdCliente = r.IdCliente
    WHERE   (@Codigo       IS NULL OR r.Codigo LIKE N'%' + @Codigo + N'%')
      AND   (@IdCliente    IS NULL OR r.IdCliente = @IdCliente)
      AND   (@TextoCliente IS NULL OR c.Nombres LIKE N'%' + @TextoCliente + N'%'
                                   OR c.Identificacion LIKE N'%' + @TextoCliente + N'%')
      AND   (@FechaDesde   IS NULL OR r.FechaEvento >= @FechaDesde)
      AND   (@FechaHasta   IS NULL OR r.FechaEvento <= @FechaHasta)
      AND   (@IdSalon      IS NULL OR r.IdSalon = @IdSalon)
      AND   (@Estado       IS NULL OR r.Estado = @Estado);

    SELECT  r.IdReserva,
            r.Codigo,
            r.IdCliente,
            Cliente         = c.Nombres,
            Identificacion  = c.Identificacion,
            EmailCliente    = c.Email,
            r.IdSalon,
            Salon           = s.Nombre,
            r.FechaEvento,
            r.HoraInicio,
            r.HoraFin,
            r.NumeroInvitados,
            r.Estado,
            r.Subtotal,
            r.Descuento,
            r.Impuesto,
            r.Total,
            r.Observacion,
            TotalDetalles   = (SELECT COUNT(*) FROM evt.ReservaDetalle AS d WHERE d.IdReserva = r.IdReserva),
            UltimoAnalisis  = (SELECT MAX(a.Fecha) FROM evt.AnalisisIA AS a WHERE a.IdReserva = r.IdReserva AND a.Exitoso = 1),
            UltimoCorreo    = (SELECT MAX(m.FechaIntento) FROM com.CorreoEnviado AS m WHERE m.IdReserva = r.IdReserva AND m.Estado = 'ENVIADO'),
            r.FechaCreacion,
            UsuarioCreacion = u.NombreUsuario
    FROM    evt.Reserva AS r
            INNER JOIN evt.Cliente AS c ON c.IdCliente = r.IdCliente
            INNER JOIN evt.Salon   AS s ON s.IdSalon   = r.IdSalon
            INNER JOIN seg.Usuario AS u ON u.IdUsuario = r.IdUsuarioCreacion
    WHERE   (@Codigo       IS NULL OR r.Codigo LIKE N'%' + @Codigo + N'%')
      AND   (@IdCliente    IS NULL OR r.IdCliente = @IdCliente)
      AND   (@TextoCliente IS NULL OR c.Nombres LIKE N'%' + @TextoCliente + N'%'
                                   OR c.Identificacion LIKE N'%' + @TextoCliente + N'%')
      AND   (@FechaDesde   IS NULL OR r.FechaEvento >= @FechaDesde)
      AND   (@FechaHasta   IS NULL OR r.FechaEvento <= @FechaHasta)
      AND   (@IdSalon      IS NULL OR r.IdSalon = @IdSalon)
      AND   (@Estado       IS NULL OR r.Estado = @Estado)
    ORDER BY r.FechaEvento DESC, r.HoraInicio DESC, r.IdReserva DESC
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS FETCH NEXT @TamanoPagina ROWS ONLY
    OPTION (RECOMPILE);   -- filtros opcionales: se evita reutilizar un plan poco selectivo
END
GO

/*----------------------------------------------------------------------------------------------
  evt.sp_Reserva_ObtenerPorId : devuelve DOS conjuntos de resultados (cabecera y detalle).
----------------------------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE evt.sp_Reserva_ObtenerPorId
    @IdReserva INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Conjunto 1: cabecera
    SELECT  r.IdReserva,
            r.Codigo,
            r.IdCliente,
            Cliente         = c.Nombres,
            Identificacion  = c.Identificacion,
            EmailCliente    = c.Email,
            TelefonoCliente = c.Telefono,
            r.IdSalon,
            Salon           = s.Nombre,
            CapacidadSalon  = s.Capacidad,
            TarifaBaseSalon = s.TarifaBase,
            r.FechaEvento,
            r.HoraInicio,
            r.HoraFin,
            r.NumeroInvitados,
            r.Estado,
            r.Subtotal,
            r.Descuento,
            r.Impuesto,
            r.Total,
            r.Observacion,
            r.MotivoCancelacion,
            r.JustificacionContingencia,
            r.IdUsuarioCreacion,
            UsuarioCreacion = u.NombreUsuario,
            r.FechaCreacion,
            r.IdUsuarioModificacion,
            r.FechaModificacion
    FROM    evt.Reserva AS r
            INNER JOIN evt.Cliente AS c ON c.IdCliente = r.IdCliente
            INNER JOIN evt.Salon   AS s ON s.IdSalon   = r.IdSalon
            INNER JOIN seg.Usuario AS u ON u.IdUsuario = r.IdUsuarioCreacion
    WHERE   r.IdReserva = @IdReserva;

    -- Conjunto 2: detalle completo
    SELECT  d.IdDetalle,
            d.IdReserva,
            d.IdRecurso,
            Recurso         = rc.Nombre,
            TipoRecurso     = rc.Tipo,
            StockTotal      = rc.StockTotal,
            d.Cantidad,
            d.PrecioUnitario,
            d.PorcentajeDescuento,
            d.SubtotalLinea
    FROM    evt.ReservaDetalle AS d
            INNER JOIN evt.Recurso AS rc ON rc.IdRecurso = d.IdRecurso
    WHERE   d.IdReserva = @IdReserva
    ORDER BY d.IdDetalle;
END
GO

/*----------------------------------------------------------------------------------------------
  evt.sp_Reserva_CambiarEstado
  Valida la transición contra evt.TransicionEstado, exige motivo cuando corresponde, revalida
  las precondiciones de negocio para CONFIRMADA y registra la auditoría. El UPDATE incluye el
  estado esperado en el WHERE: si otro usuario ya cambió la reserva, no se afecta ninguna fila
  y el cambio se rechaza. Así una reserva cambia de estado UNA SOLA VEZ (CA-06 / CA-07).
----------------------------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE evt.sp_Reserva_CambiarEstado
    @IdReserva                  INT,
    @EstadoNuevo                VARCHAR(12),
    @Motivo                     NVARCHAR(500)   = NULL,
    @JustificacionContingencia  NVARCHAR(500)   = NULL,
    @IdUsuario                  INT,
    @Mensaje                    NVARCHAR(400)   OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @EstadoActual   VARCHAR(12),
            @Codigo         NVARCHAR(20),
            @IdSalon        INT,
            @FechaEvento    DATE,
            @HoraInicio     TIME(0),
            @HoraFin        TIME(0),
            @Invitados      INT,
            @EmailCliente   NVARCHAR(150),
            @ClienteActivo  BIT,
            @RequiereMotivo BIT,
            @EsValido       BIT,
            @MensajeDisp    NVARCHAR(400);

    SET @Motivo = NULLIF(LTRIM(RTRIM(ISNULL(@Motivo, N''))), N'');
    SET @JustificacionContingencia = NULLIF(LTRIM(RTRIM(ISNULL(@JustificacionContingencia, N''))), N'');

    IF NOT EXISTS (SELECT 1 FROM seg.Usuario WHERE IdUsuario = @IdUsuario AND Estado = 1)
        THROW 51200, N'La sesión del usuario no es válida. Vuelva a iniciar sesión.', 1;

    SELECT  @EstadoActual  = r.Estado,
            @Codigo        = r.Codigo,
            @IdSalon       = r.IdSalon,
            @FechaEvento   = r.FechaEvento,
            @HoraInicio    = r.HoraInicio,
            @HoraFin       = r.HoraFin,
            @Invitados     = r.NumeroInvitados,
            @EmailCliente  = c.Email,
            @ClienteActivo = c.Estado
    FROM    evt.Reserva AS r
            INNER JOIN evt.Cliente AS c ON c.IdCliente = r.IdCliente
    WHERE   r.IdReserva = @IdReserva;

    IF @EstadoActual IS NULL
        THROW 51205, N'La reserva indicada no existe.', 1;

    IF @EstadoActual = @EstadoNuevo
        THROW 51400, N'La reserva ya se encuentra en el estado solicitado. No se aplicó ningún cambio.', 1;

    SELECT  @RequiereMotivo = t.RequiereMotivo
    FROM    evt.TransicionEstado AS t
    WHERE   t.EstadoOrigen = @EstadoActual AND t.EstadoDestino = @EstadoNuevo;

    IF @RequiereMotivo IS NULL
    BEGIN
        DECLARE @MsgTransicion NVARCHAR(400) =
            N'Transición no permitida: una reserva en estado ' + @EstadoActual
            + N' no puede pasar a ' + @EstadoNuevo + N'.'
            + CASE WHEN @EstadoActual IN ('FINALIZADA', 'CANCELADA')
                   THEN N' ' + @EstadoActual + N' es un estado terminal.' ELSE N'' END;
        THROW 51401, @MsgTransicion, 1;
    END

    IF @RequiereMotivo = 1 AND (@Motivo IS NULL OR LEN(@Motivo) < 20)
        THROW 51402, N'Debe indicar un motivo de al menos 20 caracteres para cancelar la reserva.', 1;

    /*-- Precondiciones para CONFIRMAR -----------------------------------------------------------*/
    IF @EstadoNuevo = 'CONFIRMADA'
    BEGIN
        IF @ClienteActivo = 0
            THROW 51403, N'No se puede confirmar: el cliente de la reserva está inactivo.', 1;

        IF @EmailCliente IS NULL OR @EmailCliente NOT LIKE N'%_@__%.__%' OR @EmailCliente LIKE N'% %'
            THROW 51403, N'No se puede confirmar: el cliente no tiene un correo electrónico válido.', 1;

        IF NOT EXISTS (SELECT 1 FROM evt.ReservaDetalle WHERE IdReserva = @IdReserva)
            THROW 51404, N'No se puede confirmar: la reserva no tiene detalles registrados.', 1;

        -- Disponibilidad vigente al momento de confirmar (pudo cambiar desde el borrador).
        DECLARE @Det evt.ReservaDetalleType;

        INSERT INTO @Det (IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento)
        SELECT  IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento
        FROM    evt.ReservaDetalle
        WHERE   IdReserva = @IdReserva;

        EXEC evt.sp_Disponibilidad_Validar
                @IdReserva       = @IdReserva,
                @IdSalon         = @IdSalon,
                @FechaEvento     = @FechaEvento,
                @HoraInicio      = @HoraInicio,
                @HoraFin         = @HoraFin,
                @NumeroInvitados = @Invitados,
                @Detalle         = @Det,
                @EsValido        = @EsValido    OUTPUT,
                @Mensaje         = @MensajeDisp OUTPUT,
                @Silencioso      = 1;

        IF @EsValido = 0
            THROW 51300, @MensajeDisp, 1;

        -- Análisis IA exitoso o justificación manual de contingencia auditada.
        IF NOT EXISTS (SELECT 1 FROM evt.AnalisisIA WHERE IdReserva = @IdReserva AND Exitoso = 1)
           AND (@JustificacionContingencia IS NULL OR LEN(@JustificacionContingencia) < 20)
            THROW 51405, N'No se puede confirmar: se requiere un análisis de IA exitoso o una justificación de contingencia de al menos 20 caracteres.', 1;
    END

    /*-- Cambio de estado + auditoría en una sola transacción -------------------------------------*/
    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE  evt.Reserva
        SET     Estado                    = @EstadoNuevo,
                MotivoCancelacion         = CASE WHEN @EstadoNuevo = 'CANCELADA' THEN @Motivo ELSE MotivoCancelacion END,
                JustificacionContingencia = CASE WHEN @EstadoNuevo = 'CONFIRMADA' AND @JustificacionContingencia IS NOT NULL
                                                 THEN @JustificacionContingencia ELSE JustificacionContingencia END,
                IdUsuarioModificacion     = @IdUsuario,
                FechaModificacion         = SYSDATETIME()
        WHERE   IdReserva = @IdReserva
          AND   Estado    = @EstadoActual;   -- concurrencia optimista: una sola transición efectiva

        IF @@ROWCOUNT = 0
            THROW 51406, N'La reserva fue modificada por otro usuario. Vuelva a cargarla e intente nuevamente.', 1;

        INSERT INTO evt.ReservaAuditoria (IdReserva, EstadoAnterior, EstadoNuevo, Motivo, IdUsuario)
        VALUES (@IdReserva, @EstadoActual, @EstadoNuevo,
                COALESCE(@Motivo, @JustificacionContingencia, N'Cambio de estado desde la aplicación.'), @IdUsuario);

        COMMIT TRANSACTION;

        SET @Mensaje = N'La reserva ' + @Codigo + N' pasó de ' + @EstadoActual + N' a ' + @EstadoNuevo + N'.';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/*----------------------------------------------------------------------------------------------
  evt.sp_Reserva_Auditoria_Consultar : historial de transiciones de una reserva.
----------------------------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE evt.sp_Reserva_Auditoria_Consultar
    @IdReserva INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  a.IdAuditoria, a.IdReserva, a.EstadoAnterior, a.EstadoNuevo, a.Motivo,
            Usuario = u.NombreUsuario, a.Fecha
    FROM    evt.ReservaAuditoria AS a
            INNER JOIN seg.Usuario AS u ON u.IdUsuario = a.IdUsuario
    WHERE   a.IdReserva = @IdReserva
    ORDER BY a.Fecha DESC, a.IdAuditoria DESC;
END
GO

/*==============================================================================================
  SECCION 13 : INTEGRACIONES (auditoría de correo y de análisis de IA)

  Estos procedimientos son invocados por la capa de integraciones DESPUÉS de intentar la
  operación externa. Registran siempre el intento, tanto si terminó bien como si falló, para
  que FrmAuditoriaIntegraciones pueda mostrar el histórico y permitir el reenvío auditable.
  Ninguno recibe ni almacena credenciales SMTP ni la API key del modelo.
==============================================================================================*/

CREATE OR ALTER PROCEDURE com.sp_Correo_Registrar
    @IdReserva          INT,
    @TipoNotificacion   VARCHAR(20),
    @Destinatario       NVARCHAR(150),
    @Asunto             NVARCHAR(200),
    @Estado             VARCHAR(10),
    @Error              NVARCHAR(500)   = NULL,
    @IdUsuario          INT             = NULL,
    @IdCorreoOut        INT             OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM evt.Reserva WHERE IdReserva = @IdReserva)
        THROW 51205, N'La reserva indicada no existe.', 1;

    -- Se recorta el error técnico para que quepa en la columna sin abortar el registro:
    -- el objetivo es dejar rastro del intento, nunca perder la auditoría por un mensaje largo.
    SET @Error = LEFT(NULLIF(LTRIM(RTRIM(ISNULL(@Error, N''))), N''), 500);

    INSERT INTO com.CorreoEnviado (IdReserva, TipoNotificacion, Destinatario, Asunto, Estado, Error, IdUsuario)
    VALUES (@IdReserva, @TipoNotificacion, @Destinatario, @Asunto, @Estado, @Error, @IdUsuario);

    SET @IdCorreoOut = CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE com.sp_Correo_Consultar
    @IdReserva  INT           = NULL,
    @Codigo     NVARCHAR(20)  = NULL,
    @Estado     VARCHAR(10)   = NULL,
    @FechaDesde DATE          = NULL,
    @FechaHasta DATE          = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  m.IdCorreo,
            m.IdReserva,
            CodigoReserva = r.Codigo,
            Cliente       = c.Nombres,
            m.TipoNotificacion,
            m.Destinatario,
            m.Asunto,
            m.FechaIntento,
            m.Estado,
            m.Error,
            Usuario       = u.NombreUsuario
    FROM    com.CorreoEnviado AS m
            INNER JOIN evt.Reserva AS r ON r.IdReserva = m.IdReserva
            INNER JOIN evt.Cliente AS c ON c.IdCliente = r.IdCliente
            LEFT  JOIN seg.Usuario AS u ON u.IdUsuario = m.IdUsuario
    WHERE   (@IdReserva  IS NULL OR m.IdReserva = @IdReserva)
      AND   (@Codigo     IS NULL OR r.Codigo LIKE N'%' + @Codigo + N'%')
      AND   (@Estado     IS NULL OR m.Estado = @Estado)
      AND   (@FechaDesde IS NULL OR CAST(m.FechaIntento AS DATE) >= @FechaDesde)
      AND   (@FechaHasta IS NULL OR CAST(m.FechaIntento AS DATE) <= @FechaHasta)
    ORDER BY m.FechaIntento DESC, m.IdCorreo DESC
    OPTION (RECOMPILE);
END
GO

CREATE OR ALTER PROCEDURE evt.sp_AnalisisIA_Registrar
    @IdReserva      INT,
    @Modelo         NVARCHAR(100),
    @PromptVersion  NVARCHAR(20),
    @RespuestaJson  NVARCHAR(MAX)   = NULL,
    @NivelRiesgo    VARCHAR(5)      = NULL,
    @TokensEntrada  INT             = NULL,
    @TokensSalida   INT             = NULL,
    @Exitoso        BIT,
    @Error          NVARCHAR(500)   = NULL,
    @IdUsuario      INT             = NULL,
    @IdAnalisisOut  INT             OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM evt.Reserva WHERE IdReserva = @IdReserva)
        THROW 51205, N'La reserva indicada no existe.', 1;

    SET @Error = LEFT(NULLIF(LTRIM(RTRIM(ISNULL(@Error, N''))), N''), 500);

    -- Defensa en profundidad: si el proveedor devolviera algo que no es JSON válido, se registra
    -- como intento fallido en lugar de romper el CHECK y perder la auditoría.
    IF @Exitoso = 1 AND (@RespuestaJson IS NULL OR ISJSON(@RespuestaJson) = 0)
    BEGIN
        SET @Exitoso       = 0;
        SET @RespuestaJson = NULL;
        SET @NivelRiesgo   = NULL;
        SET @Error         = ISNULL(@Error, N'La respuesta del modelo no es un JSON válido.');
    END

    IF @Exitoso = 0 AND @Error IS NULL
        SET @Error = N'Error no especificado durante el análisis con IA.';

    INSERT INTO evt.AnalisisIA
        (IdReserva, Modelo, PromptVersion, RespuestaJson, NivelRiesgo,
         TokensEntrada, TokensSalida, Exitoso, Error, IdUsuario)
    VALUES
        (@IdReserva, @Modelo, @PromptVersion, @RespuestaJson, @NivelRiesgo,
         @TokensEntrada, @TokensSalida, @Exitoso, @Error, @IdUsuario);

    SET @IdAnalisisOut = CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE evt.sp_AnalisisIA_Consultar
    @IdReserva   INT           = NULL,
    @Codigo      NVARCHAR(20)  = NULL,
    @Exitoso     BIT           = NULL,
    @NivelRiesgo VARCHAR(5)    = NULL,
    @FechaDesde  DATE          = NULL,
    @FechaHasta  DATE          = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  a.IdAnalisis,
            a.IdReserva,
            CodigoReserva = r.Codigo,
            Cliente       = c.Nombres,
            a.Modelo,
            a.PromptVersion,
            a.NivelRiesgo,
            a.TokensEntrada,
            a.TokensSalida,
            a.Fecha,
            a.Exitoso,
            a.Error,
            a.RespuestaJson,
            Usuario       = u.NombreUsuario
    FROM    evt.AnalisisIA AS a
            INNER JOIN evt.Reserva AS r ON r.IdReserva = a.IdReserva
            INNER JOIN evt.Cliente AS c ON c.IdCliente = r.IdCliente
            LEFT  JOIN seg.Usuario AS u ON u.IdUsuario = a.IdUsuario
    WHERE   (@IdReserva   IS NULL OR a.IdReserva = @IdReserva)
      AND   (@Codigo      IS NULL OR r.Codigo LIKE N'%' + @Codigo + N'%')
      AND   (@Exitoso     IS NULL OR a.Exitoso = @Exitoso)
      AND   (@NivelRiesgo IS NULL OR a.NivelRiesgo = @NivelRiesgo)
      AND   (@FechaDesde  IS NULL OR CAST(a.Fecha AS DATE) >= @FechaDesde)
      AND   (@FechaHasta  IS NULL OR CAST(a.Fecha AS DATE) <= @FechaHasta)
    ORDER BY a.Fecha DESC, a.IdAnalisis DESC
    OPTION (RECOMPILE);
END
GO

CREATE OR ALTER PROCEDURE evt.sp_AnalisisIA_ObtenerUltimo
    @IdReserva INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  TOP (1)
            a.IdAnalisis, a.IdReserva, a.Modelo, a.PromptVersion, a.RespuestaJson,
            a.NivelRiesgo, a.TokensEntrada, a.TokensSalida, a.Fecha, a.Exitoso, a.Error
    FROM    evt.AnalisisIA AS a
    WHERE   a.IdReserva = @IdReserva
      AND   a.Exitoso   = 1
    ORDER BY a.Fecha DESC, a.IdAnalisis DESC;
END
GO

/*==============================================================================================
  SECCION 14 : RESERVA DE DEMOSTRACIÓN Y VERIFICACIÓN FINAL

  Se crea UNA reserva de ejemplo invocando el propio procedimiento transaccional con el TVP.
  Cumple dos objetivos: deja datos listos para probar CA-03 (cruce de horario) y CA-04 (edición
  sin autoconflicto) inmediatamente después de instalar, y demuestra que el SP funciona dentro
  del mismo script.
==============================================================================================*/
DECLARE @IdUsuarioAdmin INT   = (SELECT IdUsuario FROM seg.Usuario WHERE NombreUsuario = N'admin');
DECLARE @IdClienteDemo  INT   = (SELECT IdCliente FROM evt.Cliente WHERE Identificacion = N'0102030405');
DECLARE @IdSalonDemo    INT   = (SELECT IdSalon   FROM evt.Salon   WHERE Nombre = N'Salon Esmeralda');
DECLARE @FechaDemo      DATE  = DATEADD(DAY, 30, CAST(SYSDATETIME() AS DATE));

DECLARE @DetalleDemo evt.ReservaDetalleType;

INSERT INTO @DetalleDemo (IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento)
SELECT IdRecurso, 2,  PrecioUnitario, 0  FROM evt.Recurso WHERE Nombre = N'Proyector Full HD'
UNION ALL
SELECT IdRecurso, 80, PrecioUnitario, 5  FROM evt.Recurso WHERE Nombre = N'Silla plegable acolchada'
UNION ALL
SELECT IdRecurso, 80, PrecioUnitario, 0  FROM evt.Recurso WHERE Nombre = N'Servicio de coffee break';

DECLARE @IdReservaDemo INT, @CodigoDemo NVARCHAR(20), @MensajeDemo NVARCHAR(400);

EXEC evt.sp_Reserva_Guardar
        @IdReserva       = NULL,
        @IdCliente       = @IdClienteDemo,
        @IdSalon         = @IdSalonDemo,
        @FechaEvento     = @FechaDemo,
        @HoraInicio      = '10:00',
        @HoraFin         = '15:00',
        @NumeroInvitados = 80,
        @Descuento       = 0,
        @Observacion     = N'Reserva de demostración creada por el script de instalación.',
        @IdUsuario       = @IdUsuarioAdmin,
        @Detalle         = @DetalleDemo,
        @IdReservaOut    = @IdReservaDemo OUTPUT,
        @CodigoOut       = @CodigoDemo    OUTPUT,
        @Mensaje         = @MensajeDemo   OUTPUT;

PRINT '>> Reserva de demostración: ' + ISNULL(@MensajeDemo, N'(sin mensaje)');
GO

/*-- Verificación final: inventario de objetos creados ------------------------------------------*/
PRINT '';
PRINT '==============================================================';
PRINT ' SmartEvent AI - Instalación completada';
PRINT '==============================================================';
GO

SELECT  Objeto = 'Tablas',                  Cantidad = COUNT(*) FROM sys.tables WHERE SCHEMA_NAME(schema_id) IN ('seg','evt','com')
UNION ALL
SELECT  'Procedimientos almacenados',       COUNT(*) FROM sys.procedures WHERE SCHEMA_NAME(schema_id) IN ('seg','evt','com')
UNION ALL
SELECT  'Tipos tabla (TVP)',                COUNT(*) FROM sys.table_types WHERE SCHEMA_NAME(schema_id) IN ('seg','evt','com')
UNION ALL
SELECT  'Índices no clustered',             COUNT(*) FROM sys.indexes i INNER JOIN sys.tables t ON t.object_id = i.object_id
                                            WHERE i.type_desc = 'NONCLUSTERED' AND SCHEMA_NAME(t.schema_id) IN ('seg','evt','com')
UNION ALL
SELECT  'Restricciones CHECK',              COUNT(*) FROM sys.check_constraints
UNION ALL
SELECT  'Claves foráneas',                  COUNT(*) FROM sys.foreign_keys
UNION ALL
SELECT  'Usuarios semilla',                 COUNT(*) FROM seg.Usuario
UNION ALL
SELECT  'Clientes semilla',                 COUNT(*) FROM evt.Cliente
UNION ALL
SELECT  'Salones semilla',                  COUNT(*) FROM evt.Salon
UNION ALL
SELECT  'Recursos semilla',                 COUNT(*) FROM evt.Recurso
UNION ALL
SELECT  'Reservas de demostración',         COUNT(*) FROM evt.Reserva
UNION ALL
SELECT  'Detalles de demostración',         COUNT(*) FROM evt.ReservaDetalle;
GO

-- Comprobación de la reserva de demostración: cabecera + totales calculados por el motor.
SELECT  r.Codigo, s.Nombre AS Salon, c.Nombres AS Cliente, r.FechaEvento, r.HoraInicio, r.HoraFin,
        r.NumeroInvitados, r.Estado, r.Subtotal, r.Descuento, r.Impuesto, r.Total,
        Detalles = (SELECT COUNT(*) FROM evt.ReservaDetalle d WHERE d.IdReserva = r.IdReserva)
FROM    evt.Reserva AS r
        INNER JOIN evt.Salon   AS s ON s.IdSalon   = r.IdSalon
        INNER JOIN evt.Cliente AS c ON c.IdCliente = r.IdCliente;
GO

PRINT '';
PRINT ' Credenciales semilla (solo laboratorio, cambiar en uso real):';
PRINT '   admin       / Admin123*   -> ADMINISTRADOR';
PRINT '   coordinador / Coord123*   -> COORDINADOR';
PRINT '';
PRINT ' Siguiente paso: configurar la cadena de conexión y las variables de entorno';
PRINT ' descritas en el README.md antes de ejecutar SmartEvent.UI.';
PRINT '==============================================================';
GO

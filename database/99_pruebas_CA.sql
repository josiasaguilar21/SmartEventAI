/*==============================================================================================
  PROYECTO   : SmartEvent AI
  ARCHIVO    : /database/99_pruebas_CA.sql
  PROPOSITO  : Banco de pruebas ejecutable de las reglas de negocio que viven en SQL Server.
               Demuestra CA-01 a CA-06 a nivel de motor, sin pasar por la interfaz, para probar
               que el rechazo ocurre en la base aunque se omita la validacion visual.

  USO        : Ejecutar DESPUES de 00_SmartEventAI.sql.
                   sqlcmd -S .\INSTANCIA -E -C -i 99_pruebas_CA.sql -W
               Cada prueba imprime OK o FALLA.

  DISENO     : El banco es AUTOCONTENIDO. Crea sus propias reservas ancla y no depende de la
               reserva de demostracion ni de la fecha en que se instalo la base. Al terminar
               elimina todo lo que creo, por lo que puede ejecutarse cuantas veces se quiera.
==============================================================================================*/
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE SmartEventAI;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

PRINT '==============================================================';
PRINT ' BANCO DE PRUEBAS - CASOS DE ACEPTACION (capa SQL Server)';
PRINT '==============================================================';
GO

/*----------------------------------------------------------------------------------------------
  Contexto comun de las pruebas.
  Todas las reservas de prueba se crean en el Salon Zafiro (capacidad 60, tarifa 280.00) en una
  fecha futura fija respecto del reloj del servidor, para no interferir con datos existentes.
----------------------------------------------------------------------------------------------*/
DECLARE @IdAdmin   INT  = (SELECT IdUsuario FROM seg.Usuario WHERE NombreUsuario = N'admin');
DECLARE @IdCoord   INT  = (SELECT IdUsuario FROM seg.Usuario WHERE NombreUsuario = N'coordinador');
DECLARE @IdCliente INT  = (SELECT IdCliente FROM evt.Cliente WHERE Identificacion = N'0987654321');
DECLARE @IdSalon   INT  = (SELECT IdSalon   FROM evt.Salon   WHERE Nombre = N'Salon Zafiro');
DECLARE @Fecha     DATE = DATEADD(DAY, 45, CAST(SYSDATETIME() AS DATE));

DECLARE @IdProyector INT = (SELECT IdRecurso FROM evt.Recurso WHERE Nombre = N'Proyector Full HD');
DECLARE @IdSilla     INT = (SELECT IdRecurso FROM evt.Recurso WHERE Nombre = N'Silla plegable acolchada');
DECLARE @IdCoffee    INT = (SELECT IdRecurso FROM evt.Recurso WHERE Nombre = N'Servicio de coffee break');
DECLARE @IdMicro     INT = (SELECT IdRecurso FROM evt.Recurso WHERE Nombre = N'Microfono inalambrico');

DECLARE @IdRes INT, @Cod NVARCHAR(20), @Msg NVARCHAR(400);
DECLARE @Det evt.ReservaDetalleType;
DECLARE @ReservasAntes INT, @DetallesAntes INT, @Conteo INT;
DECLARE @IdCA01 INT, @Sub DECIMAL(12,2), @Imp DECIMAL(12,2), @Tot DECIMAL(12,2);

-- Punto de partida limpio por si una corrida anterior se interrumpio.
DELETE FROM evt.Reserva WHERE Observacion LIKE N'Prueba CA-%';

/*----------------------------------------------------------------------------------------------
  CA-01 : Guardar una reserva valida con TRES detalles y recuperarla integra.
----------------------------------------------------------------------------------------------*/
PRINT '';
PRINT '--- CA-01 : alta de reserva con 3 detalles ------------------------------------';

DELETE FROM @Det;
INSERT INTO @Det (IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento) VALUES
    (@IdProyector, 1, 35.00, 0),
    (@IdMicro,     4, 12.50, 10),
    (@IdCoffee,   50,  4.50, 0);

EXEC evt.sp_Reserva_Guardar
        @IdCliente = @IdCliente, @IdSalon = @IdSalon, @FechaEvento = @Fecha,
        @HoraInicio = '09:00', @HoraFin = '13:00', @NumeroInvitados = 50,
        @Descuento = 0, @Observacion = N'Prueba CA-01', @IdUsuario = @IdAdmin,
        @Detalle = @Det, @IdReservaOut = @IdRes OUTPUT, @CodigoOut = @Cod OUTPUT, @Mensaje = @Msg OUTPUT;

SET @IdCA01 = @IdRes;
SELECT @Conteo = COUNT(*) FROM evt.ReservaDetalle WHERE IdReserva = @IdCA01;

IF @IdCA01 IS NOT NULL AND @Conteo = 3
    PRINT 'OK    | Reserva ' + @Cod + ' creada y recuperada con sus 3 detalles.';
ELSE
    PRINT 'FALLA | No se recuperaron los 3 detalles.';

-- Totales que debe calcular el motor:
--   Zafiro 280.00 + (1*35.00) + (4*12.50*0.90 = 45.00) + (50*4.50 = 225.00) = 585.00
--   Impuesto 15 por ciento = 87.75 ; Total = 672.75
SELECT @Sub = Subtotal, @Imp = Impuesto, @Tot = Total FROM evt.Reserva WHERE IdReserva = @IdCA01;

IF @Sub = 585.00 AND @Imp = 87.75 AND @Tot = 672.75
    PRINT 'OK    | Totales recalculados en SQL: Subtotal=585.00 Impuesto=87.75 Total=672.75';
ELSE
    PRINT 'FALLA | Totales incorrectos: Subtotal=' + CAST(@Sub AS NVARCHAR(20))
          + ' Impuesto=' + CAST(@Imp AS NVARCHAR(20)) + ' Total=' + CAST(@Tot AS NVARCHAR(20));

/*----------------------------------------------------------------------------------------------
  CA-02 : Error forzado en un detalle -> ROLLBACK completo, sin cabecera ni detalles parciales.
----------------------------------------------------------------------------------------------*/
PRINT '';
PRINT '--- CA-02 : rollback ante detalle invalido ------------------------------------';

SELECT @ReservasAntes = COUNT(*) FROM evt.Reserva;
SELECT @DetallesAntes = COUNT(*) FROM evt.ReservaDetalle;

DELETE FROM @Det;
INSERT INTO @Det (IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento) VALUES
    (@IdProyector, 1, 35.00, 0),
    (@IdCoffee,   20,  4.50, 0),
    (999999,       1, 10.00, 0);   -- recurso inexistente: la tercera linea rompe la operacion

BEGIN TRY
    EXEC evt.sp_Reserva_Guardar
            @IdCliente = @IdCliente, @IdSalon = @IdSalon, @FechaEvento = @Fecha,
            @HoraInicio = '19:00', @HoraFin = '22:00', @NumeroInvitados = 30,
            @Descuento = 0, @Observacion = N'Prueba CA-02', @IdUsuario = @IdAdmin,
            @Detalle = @Det, @IdReservaOut = @IdRes OUTPUT, @CodigoOut = @Cod OUTPUT, @Mensaje = @Msg OUTPUT;
    PRINT 'FALLA | El procedimiento no rechazo el detalle invalido.';
END TRY
BEGIN CATCH
    PRINT 'OK    | Rechazado (' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + '): ' + ERROR_MESSAGE();
END CATCH

IF (SELECT COUNT(*) FROM evt.Reserva) = @ReservasAntes
   AND (SELECT COUNT(*) FROM evt.ReservaDetalle) = @DetallesAntes
    PRINT 'OK    | Sin cabeceras huerfanas ni detalles parciales tras el rollback.';
ELSE
    PRINT 'FALLA | Quedaron datos parciales despues del error.';

/*----------------------------------------------------------------------------------------------
  CA-03 : Cruce parcial de franja horaria en el mismo salon y fecha.
          Ancla: la reserva de CA-01 ocupa el Salon Zafiro de 09:00 a 13:00.
          Se intenta 12:00 - 16:00 (se solapa una hora) y luego 13:00 - 17:00 (adyacente).
----------------------------------------------------------------------------------------------*/
PRINT '';
PRINT '--- CA-03 : cruce de franja horaria --------------------------------------------';

DELETE FROM @Det;
INSERT INTO @Det (IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento) VALUES (@IdProyector, 1, 35.00, 0);

BEGIN TRY
    EXEC evt.sp_Reserva_Guardar
            @IdCliente = @IdCliente, @IdSalon = @IdSalon, @FechaEvento = @Fecha,
            @HoraInicio = '12:00', @HoraFin = '16:00', @NumeroInvitados = 40,
            @Descuento = 0, @Observacion = N'Prueba CA-03 solapada', @IdUsuario = @IdAdmin,
            @Detalle = @Det, @IdReservaOut = @IdRes OUTPUT, @CodigoOut = @Cod OUTPUT, @Mensaje = @Msg OUTPUT;
    PRINT 'FALLA | Se acepto una reserva con cruce de horario.';
END TRY
BEGIN CATCH
    PRINT 'OK    | Rechazado (' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + '): ' + ERROR_MESSAGE();
END CATCH

-- Franja adyacente: fin existente = inicio nuevo. La regla inicioNuevo < finExistente AND
-- finNuevo > inicioExistente NO debe considerarlo cruce.
BEGIN TRY
    EXEC evt.sp_Reserva_Guardar
            @IdCliente = @IdCliente, @IdSalon = @IdSalon, @FechaEvento = @Fecha,
            @HoraInicio = '13:00', @HoraFin = '17:00', @NumeroInvitados = 40,
            @Descuento = 0, @Observacion = N'Prueba CA-03 adyacente', @IdUsuario = @IdAdmin,
            @Detalle = @Det, @IdReservaOut = @IdRes OUTPUT, @CodigoOut = @Cod OUTPUT, @Mensaje = @Msg OUTPUT;
    PRINT 'OK    | Franja adyacente 13:00-17:00 aceptada (' + @Cod + '): no hay falso positivo.';
END TRY
BEGIN CATCH
    PRINT 'FALLA | Se rechazo una franja adyacente valida: ' + ERROR_MESSAGE();
END CATCH

/*----------------------------------------------------------------------------------------------
  CA-04 : Editar una reserva BORRADOR sin que se detecte a si misma como conflicto.
          Se reedita la reserva de CA-01 conservando exactamente su mismo horario.
----------------------------------------------------------------------------------------------*/
PRINT '';
PRINT '--- CA-04 : edicion sin autoconflicto ------------------------------------------';

DELETE FROM @Det;
INSERT INTO @Det (IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento)
SELECT IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento
FROM   evt.ReservaDetalle WHERE IdReserva = @IdCA01;

BEGIN TRY
    EXEC evt.sp_Reserva_Guardar
            @IdReserva = @IdCA01,
            @IdCliente = @IdCliente, @IdSalon = @IdSalon, @FechaEvento = @Fecha,
            @HoraInicio = '09:00', @HoraFin = '13:00', @NumeroInvitados = 55,
            @Descuento = 0, @Observacion = N'Prueba CA-01 editada en CA-04', @IdUsuario = @IdAdmin,
            @Detalle = @Det, @IdReservaOut = @IdRes OUTPUT, @CodigoOut = @Cod OUTPUT, @Mensaje = @Msg OUTPUT;
    PRINT 'OK    | Reserva ' + @Cod + ' editada con su mismo horario, sin autoconflicto.';
END TRY
BEGIN CATCH
    PRINT 'FALLA | La reserva se detecto a si misma como conflicto: ' + ERROR_MESSAGE();
END CATCH

/*----------------------------------------------------------------------------------------------
  CA-05 : Rechazo desde SQL por capacidad, por stock concurrente y por descuento no autorizado.
----------------------------------------------------------------------------------------------*/
PRINT '';
PRINT '--- CA-05a : capacidad del salon ----------------------------------------------';

DELETE FROM @Det;
INSERT INTO @Det (IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento) VALUES (@IdProyector, 1, 35.00, 0);

BEGIN TRY
    EXEC evt.sp_Reserva_Guardar
            @IdCliente = @IdCliente, @IdSalon = @IdSalon, @FechaEvento = @Fecha,
            @HoraInicio = '19:00', @HoraFin = '22:00', @NumeroInvitados = 500,  -- Zafiro admite 60
            @Descuento = 0, @Observacion = N'Prueba CA-05a', @IdUsuario = @IdAdmin,
            @Detalle = @Det, @IdReservaOut = @IdRes OUTPUT, @CodigoOut = @Cod OUTPUT, @Mensaje = @Msg OUTPUT;
    PRINT 'FALLA | Se acepto una reserva por encima de la capacidad del salon.';
END TRY
BEGIN CATCH
    PRINT 'OK    | Rechazado (' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + '): ' + ERROR_MESSAGE();
END CATCH

PRINT '--- CA-05b : stock concurrente del recurso ------------------------------------';

DELETE FROM @Det;
INSERT INTO @Det (IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento) VALUES (@IdSilla, 500, 1.75, 0);  -- stock 400

BEGIN TRY
    EXEC evt.sp_Reserva_Guardar
            @IdCliente = @IdCliente, @IdSalon = @IdSalon, @FechaEvento = @Fecha,
            @HoraInicio = '19:00', @HoraFin = '22:00', @NumeroInvitados = 50,
            @Descuento = 0, @Observacion = N'Prueba CA-05b', @IdUsuario = @IdAdmin,
            @Detalle = @Det, @IdReservaOut = @IdRes OUTPUT, @CodigoOut = @Cod OUTPUT, @Mensaje = @Msg OUTPUT;
    PRINT 'FALLA | Se acepto una cantidad superior al stock disponible.';
END TRY
BEGIN CATCH
    PRINT 'OK    | Rechazado (' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + '): ' + ERROR_MESSAGE();
END CATCH

PRINT '--- CA-05c : descuento de linea mayor al 10 por ciento con rol COORDINADOR ----';

DELETE FROM @Det;
INSERT INTO @Det (IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento) VALUES (@IdProyector, 1, 35.00, 18);

BEGIN TRY
    EXEC evt.sp_Reserva_Guardar
            @IdCliente = @IdCliente, @IdSalon = @IdSalon, @FechaEvento = @Fecha,
            @HoraInicio = '19:00', @HoraFin = '22:00', @NumeroInvitados = 30,
            @Descuento = 0, @Observacion = N'Prueba CA-05c', @IdUsuario = @IdCoord,
            @Detalle = @Det, @IdReservaOut = @IdRes OUTPUT, @CodigoOut = @Cod OUTPUT, @Mensaje = @Msg OUTPUT;
    PRINT 'FALLA | Un COORDINADOR aplico un descuento superior al permitido.';
END TRY
BEGIN CATCH
    PRINT 'OK    | Rechazado (' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + '): ' + ERROR_MESSAGE();
END CATCH

/*----------------------------------------------------------------------------------------------
  CA-06 : Transiciones de estado, contingencia, idempotencia, bloqueo de edicion y cancelacion.
----------------------------------------------------------------------------------------------*/
PRINT '';
PRINT '--- CA-06 : flujo de estados ---------------------------------------------------';

BEGIN TRY
    EXEC evt.sp_Reserva_CambiarEstado @IdReserva = @IdCA01, @EstadoNuevo = 'FINALIZADA',
                                      @IdUsuario = @IdAdmin, @Mensaje = @Msg OUTPUT;
    PRINT 'FALLA | Se permitio BORRADOR -> FINALIZADA.';
END TRY
BEGIN CATCH
    PRINT 'OK    | Transicion invalida rechazada: ' + ERROR_MESSAGE();
END CATCH

BEGIN TRY
    EXEC evt.sp_Reserva_CambiarEstado @IdReserva = @IdCA01, @EstadoNuevo = 'CONFIRMADA',
                                      @IdUsuario = @IdAdmin, @Mensaje = @Msg OUTPUT;
    PRINT 'FALLA | Se confirmo sin analisis de IA ni justificacion de contingencia.';
END TRY
BEGIN CATCH
    PRINT 'OK    | Confirmacion bloqueada: ' + ERROR_MESSAGE();
END CATCH

-- Contingencia manual auditada (>= 20 caracteres): permite confirmar si la IA no esta disponible.
BEGIN TRY
    EXEC evt.sp_Reserva_CambiarEstado
            @IdReserva = @IdCA01, @EstadoNuevo = 'CONFIRMADA',
            @JustificacionContingencia = N'Servicio de IA no disponible, validado manualmente por el coordinador.',
            @IdUsuario = @IdAdmin, @Mensaje = @Msg OUTPUT;
    PRINT 'OK    | ' + @Msg;
END TRY
BEGIN CATCH
    PRINT 'FALLA | No se pudo confirmar con contingencia: ' + ERROR_MESSAGE();
END CATCH

-- Idempotencia: repetir la misma transicion no vuelve a aplicarse (CA-06 / CA-07).
BEGIN TRY
    EXEC evt.sp_Reserva_CambiarEstado @IdReserva = @IdCA01, @EstadoNuevo = 'CONFIRMADA',
                                      @IdUsuario = @IdAdmin, @Mensaje = @Msg OUTPUT;
    PRINT 'FALLA | Se aplico dos veces la misma transicion.';
END TRY
BEGIN CATCH
    PRINT 'OK    | Segunda confirmacion rechazada: ' + ERROR_MESSAGE();
END CATCH

-- Una reserva CONFIRMADA no puede editar cliente, salon, fecha, horario ni detalles.
DELETE FROM @Det;
INSERT INTO @Det (IdRecurso, Cantidad, PrecioUnitario, PorcentajeDescuento) VALUES (@IdProyector, 1, 35.00, 0);

BEGIN TRY
    EXEC evt.sp_Reserva_Guardar
            @IdReserva = @IdCA01, @IdCliente = @IdCliente, @IdSalon = @IdSalon, @FechaEvento = @Fecha,
            @HoraInicio = '09:00', @HoraFin = '13:00', @NumeroInvitados = 55,
            @Descuento = 0, @Observacion = N'Prueba CA-01 edicion indebida', @IdUsuario = @IdAdmin,
            @Detalle = @Det, @IdReservaOut = @IdRes OUTPUT, @CodigoOut = @Cod OUTPUT, @Mensaje = @Msg OUTPUT;
    PRINT 'FALLA | Se edito una reserva CONFIRMADA.';
END TRY
BEGIN CATCH
    PRINT 'OK    | Edicion bloqueada: ' + ERROR_MESSAGE();
END CATCH

BEGIN TRY
    EXEC evt.sp_Reserva_CambiarEstado @IdReserva = @IdCA01, @EstadoNuevo = 'CANCELADA',
                                      @Motivo = N'Se cancelo', @IdUsuario = @IdAdmin, @Mensaje = @Msg OUTPUT;
    PRINT 'FALLA | Se acepto un motivo de cancelacion menor a 20 caracteres.';
END TRY
BEGIN CATCH
    PRINT 'OK    | Motivo insuficiente rechazado: ' + ERROR_MESSAGE();
END CATCH

BEGIN TRY
    EXEC evt.sp_Reserva_CambiarEstado
            @IdReserva = @IdCA01, @EstadoNuevo = 'CANCELADA',
            @Motivo = N'El cliente reprogramo el evento corporativo para el proximo trimestre.',
            @IdUsuario = @IdAdmin, @Mensaje = @Msg OUTPUT;
    PRINT 'OK    | ' + @Msg;
END TRY
BEGIN CATCH
    PRINT 'FALLA | No se pudo cancelar: ' + ERROR_MESSAGE();
END CATCH

BEGIN TRY
    EXEC evt.sp_Reserva_CambiarEstado @IdReserva = @IdCA01, @EstadoNuevo = 'FINALIZADA',
                                      @IdUsuario = @IdAdmin, @Mensaje = @Msg OUTPUT;
    PRINT 'FALLA | Se permitio salir de un estado terminal.';
END TRY
BEGIN CATCH
    PRINT 'OK    | Estado terminal respetado: ' + ERROR_MESSAGE();
END CATCH

SELECT @Conteo = COUNT(*) FROM evt.ReservaAuditoria WHERE IdReserva = @IdCA01;
IF @Conteo >= 3
    PRINT 'OK    | Auditoria de transiciones registrada: ' + CAST(@Conteo AS NVARCHAR(10)) + ' movimientos.';
ELSE
    PRINT 'FALLA | Auditoria incompleta: ' + CAST(@Conteo AS NVARCHAR(10)) + ' movimientos.';
GO

/*----------------------------------------------------------------------------------------------
  SEGURIDAD : autenticacion por hash, credenciales invalidas y bloqueo temporal.
  Los hashes usados son los mismos que genera la aplicacion con PBKDF2-HMAC-SHA256
  (120000 iteraciones) sobre el salt almacenado en la fila de cada usuario.
----------------------------------------------------------------------------------------------*/
PRINT '';
PRINT '--- SEGURIDAD : autenticacion y bloqueo ----------------------------------------';

DECLARE @Res INT, @MsgAuth NVARCHAR(200), @Seg INT, @i INT = 1;

EXEC seg.sp_Usuario_Autenticar
        @NombreUsuario = N'admin',
        @PasswordHash  = 0xD69ABAF7F22BF4FF747958A2CA8107A8102755A1C10BE7299F992406BC860539,
        @Resultado = @Res OUTPUT, @Mensaje = @MsgAuth OUTPUT, @SegundosBloqueo = @Seg OUTPUT;

IF @Res = 0 PRINT 'OK    | Credenciales correctas aceptadas: ' + @MsgAuth;
ELSE        PRINT 'FALLA | No se autentico al usuario admin: ' + @MsgAuth;

EXEC seg.sp_Usuario_Autenticar
        @NombreUsuario = N'admin', @PasswordHash = 0x00,
        @Resultado = @Res OUTPUT, @Mensaje = @MsgAuth OUTPUT, @SegundosBloqueo = @Seg OUTPUT;

IF @Res = 1 PRINT 'OK    | Credenciales incorrectas rechazadas: ' + @MsgAuth;
ELSE        PRINT 'FALLA | Resultado inesperado con hash invalido.';

-- Usuario inexistente: mismo mensaje generico, sin revelar si la cuenta existe.
EXEC seg.sp_Usuario_Autenticar
        @NombreUsuario = N'usuario_que_no_existe', @PasswordHash = 0x00,
        @Resultado = @Res OUTPUT, @Mensaje = @MsgAuth OUTPUT, @SegundosBloqueo = @Seg OUTPUT;

IF @Res = 1 PRINT 'OK    | Usuario inexistente tratado como credencial invalida (sin enumeracion).';
ELSE        PRINT 'FALLA | Se filtro informacion sobre la existencia del usuario.';

WHILE @i <= 4
BEGIN
    EXEC seg.sp_Usuario_Autenticar
            @NombreUsuario = N'coordinador', @PasswordHash = 0x00,
            @Resultado = @Res OUTPUT, @Mensaje = @MsgAuth OUTPUT, @SegundosBloqueo = @Seg OUTPUT;
    SET @i += 1;
END

EXEC seg.sp_Usuario_Autenticar
        @NombreUsuario = N'coordinador', @PasswordHash = 0x00,
        @Resultado = @Res OUTPUT, @Mensaje = @MsgAuth OUTPUT, @SegundosBloqueo = @Seg OUTPUT;

IF @Res = 2 PRINT 'OK    | Bloqueo temporal activado tras 5 intentos: ' + @MsgAuth;
ELSE        PRINT 'FALLA | No se activo el bloqueo temporal.';

-- Se libera el bloqueo para no afectar las pruebas manuales posteriores desde la aplicacion.
UPDATE seg.Usuario SET IntentosFallidos = 0, BloqueadoHasta = NULL WHERE NombreUsuario = N'coordinador';
PRINT 'INFO  | Bloqueo del usuario coordinador liberado.';
GO

/*----------------------------------------------------------------------------------------------
  LIMPIEZA : se eliminan unicamente las reservas creadas por este banco de pruebas.
  La eliminacion en cascada retira detalles, auditoria, analisis IA y correos asociados.
----------------------------------------------------------------------------------------------*/
PRINT '';
PRINT '--- LIMPIEZA -------------------------------------------------------------------';

DELETE FROM evt.Reserva WHERE Observacion LIKE N'Prueba CA-%';

DECLARE @Restantes INT = (SELECT COUNT(*) FROM evt.Reserva);
PRINT 'INFO  | Reservas de prueba eliminadas. Reservas restantes en la base: ' + CAST(@Restantes AS NVARCHAR(10));
PRINT '';
PRINT '==============================================================';
PRINT ' FIN DEL BANCO DE PRUEBAS';
PRINT '==============================================================';
GO

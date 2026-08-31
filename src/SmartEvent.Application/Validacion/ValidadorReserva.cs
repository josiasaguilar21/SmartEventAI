using SmartEvent.Application.Calculo;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;

namespace SmartEvent.Application.Validacion;

/// <summary>
/// Reglas de negocio de la reserva evaluadas en el cliente, antes de ir al servidor.
///
/// Es el espejo de lo que impone SQL Server. Se duplica a proposito por dos motivos:
///   1. Experiencia de uso: el usuario recibe el error al instante y con todos los problemas
///      juntos, sin esperar una ida y vuelta a la base.
///   2. Defensa en profundidad: si alguien elimina esta clase, las reglas se siguen cumpliendo;
///      si alguien elimina las de SQL, no. Por eso el orden de importancia es el inverso al de
///      ejecucion, y por eso nunca se relaja una regla aqui "porque ya la valida la base".
///
/// Lo que esta clase NO puede validar, y por eso queda en manos del motor:
///   - el cruce de franja horaria con otras reservas,
///   - el stock concurrente de un recurso en la misma fecha y horario.
/// Ambos dependen del estado global de la base en el instante del guardado.
/// </summary>
public static class ValidadorReserva
{
    public const int DuracionMinimaHoras = 2;
    public const int DuracionMaximaHoras = 12;
    public const decimal DescuentoLineaMaximo = 20m;
    public const decimal DescuentoLineaMaximoSinPermiso = 10m;

    public static ResultadoValidacion Validar(
        ReservaGuardarDto reserva,
        Salon? salon,
        IReadOnlyDictionary<int, Recurso> recursosPorId,
        SesionUsuario sesion)
    {
        ArgumentNullException.ThrowIfNull(reserva);
        ArgumentNullException.ThrowIfNull(recursosPorId);
        ArgumentNullException.ThrowIfNull(sesion);

        var resultado = new ResultadoValidacion();

        ValidarCabecera(reserva, salon, resultado);
        ValidarDetalles(reserva, recursosPorId, sesion, resultado);
        ValidarDescuentoGlobal(reserva, salon, resultado);

        return resultado;
    }

    private static void ValidarCabecera(ReservaGuardarDto reserva, Salon? salon, ResultadoValidacion resultado)
    {
        resultado.Exigir(reserva.IdCliente > 0, nameof(reserva.IdCliente),
            "Seleccione el cliente de la reserva.");

        resultado.Exigir(reserva.IdSalon > 0, nameof(reserva.IdSalon),
            "Seleccione el salon del evento.");

        if (salon is not null)
        {
            resultado.Exigir(salon.Estado, nameof(reserva.IdSalon),
                $"El salon '{salon.Nombre}' esta inactivo y no admite nuevas reservas.");
        }

        // La fecha pasada solo se bloquea al CREAR. Una reserva ya existente se puede seguir
        // editando o cerrando aunque su fecha haya quedado atras.
        if (reserva.IdReserva is null)
        {
            resultado.Exigir(reserva.FechaEvento >= DateOnly.FromDateTime(DateTime.Today),
                nameof(reserva.FechaEvento),
                "La fecha del evento no puede ser anterior al dia de hoy.");
        }

        if (reserva.HoraFin <= reserva.HoraInicio)
        {
            resultado.Agregar(nameof(reserva.HoraFin),
                "La hora de fin debe ser posterior a la hora de inicio.");
        }
        else
        {
            var horas = (reserva.HoraFin.ToTimeSpan() - reserva.HoraInicio.ToTimeSpan()).TotalHours;

            resultado.Exigir(horas >= DuracionMinimaHoras, nameof(reserva.HoraFin),
                $"La duracion minima de un evento es de {DuracionMinimaHoras} horas (indicada: {horas:0.#}).");

            resultado.Exigir(horas <= DuracionMaximaHoras, nameof(reserva.HoraFin),
                $"La duracion maxima de un evento es de {DuracionMaximaHoras} horas (indicada: {horas:0.#}).");
        }

        resultado.Exigir(reserva.NumeroInvitados > 0, nameof(reserva.NumeroInvitados),
            "El numero de invitados debe ser mayor que cero.");

        if (salon is not null && reserva.NumeroInvitados > salon.Capacidad)
        {
            resultado.Agregar(nameof(reserva.NumeroInvitados),
                $"El numero de invitados ({reserva.NumeroInvitados}) supera la capacidad del salon " +
                $"'{salon.Nombre}' ({salon.Capacidad}).");
        }
    }

    private static void ValidarDetalles(ReservaGuardarDto reserva, IReadOnlyDictionary<int, Recurso> recursosPorId,
                                        SesionUsuario sesion, ResultadoValidacion resultado)
    {
        if (reserva.Detalles.Count == 0)
        {
            resultado.Agregar(nameof(reserva.Detalles),
                "La reserva debe incluir al menos un recurso o servicio.");
            return;
        }

        var repetidos = reserva.Detalles
            .GroupBy(d => d.IdRecurso)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var idRepetido in repetidos)
        {
            var nombre = recursosPorId.TryGetValue(idRepetido, out var recursoRepetido)
                ? recursoRepetido.Nombre
                : $"recurso {idRepetido}";

            resultado.Agregar(nameof(reserva.Detalles),
                $"El recurso '{nombre}' aparece mas de una vez. Agrupe las cantidades en una sola linea.");
        }

        foreach (var detalle in reserva.Detalles)
        {
            if (!recursosPorId.TryGetValue(detalle.IdRecurso, out var recurso))
            {
                resultado.Agregar(nameof(reserva.Detalles),
                    "Hay una linea con un recurso que ya no existe o fue inactivado. Eliminela y vuelva a agregarla.");
                continue;
            }

            if (!recurso.Estado)
            {
                resultado.Agregar(nameof(reserva.Detalles),
                    $"El recurso '{recurso.Nombre}' esta inactivo y no puede reservarse.");
            }

            if (detalle.Cantidad <= 0)
            {
                resultado.Agregar(nameof(detalle.Cantidad),
                    $"La cantidad de '{recurso.Nombre}' debe ser mayor que cero.");
            }
            else if (detalle.Cantidad > recurso.StockTotal)
            {
                // Comprobacion contra el inventario total. La disponibilidad real considerando
                // otras reservas del mismo dia y franja la calcula evt.sp_Disponibilidad_Validar.
                resultado.Agregar(nameof(detalle.Cantidad),
                    $"La cantidad de '{recurso.Nombre}' ({detalle.Cantidad}) supera el inventario total " +
                    $"registrado ({recurso.StockTotal}).");
            }

            if (detalle.PrecioUnitario < 0)
            {
                resultado.Agregar(nameof(detalle.PrecioUnitario),
                    $"El precio unitario de '{recurso.Nombre}' no puede ser negativo.");
            }

            ValidarDescuentoLinea(detalle, recurso, sesion, resultado);
        }
    }

    private static void ValidarDescuentoLinea(ReservaDetalleGuardarDto detalle, Recurso recurso,
                                              SesionUsuario sesion, ResultadoValidacion resultado)
    {
        if (detalle.PorcentajeDescuento < 0 || detalle.PorcentajeDescuento > DescuentoLineaMaximo)
        {
            resultado.Agregar(nameof(detalle.PorcentajeDescuento),
                $"El descuento de '{recurso.Nombre}' debe estar entre 0 y {DescuentoLineaMaximo:0} por ciento.");
            return;
        }

        // Autorizacion por rol. Se comprueba tambien aqui para que el coordinador reciba un
        // mensaje claro antes de guardar, pero quien realmente lo impide es el procedimiento
        // almacenado, que recibe el usuario y consulta su rol en la base.
        if (detalle.PorcentajeDescuento > DescuentoLineaMaximoSinPermiso && !sesion.PuedeAplicarDescuentoAlto)
        {
            resultado.Agregar(nameof(detalle.PorcentajeDescuento),
                $"El descuento de '{recurso.Nombre}' ({detalle.PorcentajeDescuento:0.##} por ciento) supera el " +
                $"{DescuentoLineaMaximoSinPermiso:0} por ciento permitido para su rol. " +
                "Solicite la autorizacion de un administrador.");
        }
    }

    private static void ValidarDescuentoGlobal(ReservaGuardarDto reserva, Salon? salon, ResultadoValidacion resultado)
    {
        if (reserva.Descuento < 0)
        {
            resultado.Agregar(nameof(reserva.Descuento), "El descuento global no puede ser negativo.");
            return;
        }

        if (salon is null || reserva.Descuento == 0m)
        {
            return;
        }

        var totales = CalculadoraTotales.Calcular(salon.TarifaBase, reserva.Detalles, 0m);

        if (reserva.Descuento > totales.Subtotal)
        {
            resultado.Agregar(nameof(reserva.Descuento),
                $"El descuento global ({reserva.Descuento:N2}) no puede superar el subtotal de la reserva " +
                $"({totales.Subtotal:N2}).");
        }
    }
}

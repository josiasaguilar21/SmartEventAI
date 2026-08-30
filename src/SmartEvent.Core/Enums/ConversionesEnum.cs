using SmartEvent.Core.Exceptions;

namespace SmartEvent.Core.Enums;

/// <summary>
/// Traduccion entre los enums del dominio y los literales que persiste SQL Server.
/// Se centraliza aqui para que ni la capa de datos ni la de presentacion escriban cadenas
/// magicas como "CONFIRMADA" repartidas por el codigo: un cambio de valor se hace en un solo sitio.
/// </summary>
public static class ConversionesEnum
{
    // ---------------------------------------------------------------------------- EstadoReserva
    public static string ASql(this EstadoReserva estado) => estado switch
    {
        EstadoReserva.Borrador => "BORRADOR",
        EstadoReserva.Confirmada => "CONFIRMADA",
        EstadoReserva.Finalizada => "FINALIZADA",
        EstadoReserva.Cancelada => "CANCELADA",
        _ => throw new ErrorTecnicoException($"Estado de reserva no soportado: {estado}.")
    };

    public static EstadoReserva AEstadoReserva(string valor) => valor?.Trim().ToUpperInvariant() switch
    {
        "BORRADOR" => EstadoReserva.Borrador,
        "CONFIRMADA" => EstadoReserva.Confirmada,
        "FINALIZADA" => EstadoReserva.Finalizada,
        "CANCELADA" => EstadoReserva.Cancelada,
        _ => throw new ErrorTecnicoException($"Estado de reserva desconocido recibido de la base de datos: '{valor}'.")
    };

    /// <summary>Texto legible para mostrar en la interfaz.</summary>
    public static string ATextoUsuario(this EstadoReserva estado) => estado switch
    {
        EstadoReserva.Borrador => "Borrador",
        EstadoReserva.Confirmada => "Confirmada",
        EstadoReserva.Finalizada => "Finalizada",
        EstadoReserva.Cancelada => "Cancelada",
        _ => estado.ToString()
    };

    /// <summary>
    /// Estados terminales: no admiten ninguna transicion posterior. La regla real la aplica
    /// SQL Server; esta propiedad solo permite que la interfaz deshabilite botones sin ir al servidor.
    /// </summary>
    public static bool EsTerminal(this EstadoReserva estado) =>
        estado is EstadoReserva.Finalizada or EstadoReserva.Cancelada;

    /// <summary>Solo una reserva en BORRADOR admite edicion de cabecera y detalles.</summary>
    public static bool PermiteEdicion(this EstadoReserva estado) => estado == EstadoReserva.Borrador;

    // ------------------------------------------------------------------------------ TipoRecurso
    public static string ASql(this TipoRecurso tipo) => tipo switch
    {
        TipoRecurso.Equipo => "EQUIPO",
        TipoRecurso.Mobiliario => "MOBILIARIO",
        TipoRecurso.Servicio => "SERVICIO",
        TipoRecurso.Catering => "CATERING",
        _ => throw new ErrorTecnicoException($"Tipo de recurso no soportado: {tipo}.")
    };

    public static TipoRecurso ATipoRecurso(string valor) => valor?.Trim().ToUpperInvariant() switch
    {
        "EQUIPO" => TipoRecurso.Equipo,
        "MOBILIARIO" => TipoRecurso.Mobiliario,
        "SERVICIO" => TipoRecurso.Servicio,
        "CATERING" => TipoRecurso.Catering,
        _ => throw new ErrorTecnicoException($"Tipo de recurso desconocido recibido de la base de datos: '{valor}'.")
    };

    // ----------------------------------------------------------------------------- EstadoCorreo
    public static string ASql(this EstadoCorreo estado) =>
        estado == EstadoCorreo.Enviado ? "ENVIADO" : "ERROR";

    public static EstadoCorreo AEstadoCorreo(string valor) =>
        string.Equals(valor?.Trim(), "ENVIADO", StringComparison.OrdinalIgnoreCase)
            ? EstadoCorreo.Enviado
            : EstadoCorreo.Error;

    // ------------------------------------------------------------------------- TipoNotificacion
    public static string ASql(this TipoNotificacion tipo) => tipo switch
    {
        TipoNotificacion.Confirmacion => "CONFIRMACION",
        TipoNotificacion.Cancelacion => "CANCELACION",
        TipoNotificacion.Reenvio => "REENVIO",
        _ => throw new ErrorTecnicoException($"Tipo de notificacion no soportado: {tipo}.")
    };

    public static TipoNotificacion ATipoNotificacion(string valor) => valor?.Trim().ToUpperInvariant() switch
    {
        "CONFIRMACION" => TipoNotificacion.Confirmacion,
        "CANCELACION" => TipoNotificacion.Cancelacion,
        "REENVIO" => TipoNotificacion.Reenvio,
        _ => TipoNotificacion.Confirmacion
    };

    // ------------------------------------------------------------------------------ NivelRiesgo
    public static string ASql(this NivelRiesgo nivel) => nivel switch
    {
        NivelRiesgo.Bajo => "BAJO",
        NivelRiesgo.Medio => "MEDIO",
        NivelRiesgo.Alto => "ALTO",
        _ => throw new ErrorTecnicoException($"Nivel de riesgo no soportado: {nivel}.")
    };

    /// <summary>
    /// Conversion tolerante usada al deserializar la respuesta del modelo: si el proveedor
    /// devolviera un valor fuera del contrato, se trata como respuesta invalida y no revienta.
    /// </summary>
    public static bool TryANivelRiesgo(string? valor, out NivelRiesgo nivel)
    {
        switch (valor?.Trim().ToUpperInvariant())
        {
            case "BAJO": nivel = NivelRiesgo.Bajo; return true;
            case "MEDIO": nivel = NivelRiesgo.Medio; return true;
            case "ALTO": nivel = NivelRiesgo.Alto; return true;
            default: nivel = NivelRiesgo.Bajo; return false;
        }
    }
}

using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;

namespace SmartEvent.Core.Abstractions;

/// <summary>
/// Persistencia de la auditoria de integraciones: intentos de correo y analisis de IA.
/// Se registra SIEMPRE, tanto el exito como el fallo. Ninguna operacion recibe ni guarda
/// credenciales SMTP ni claves de API.
/// </summary>
public interface IAuditoriaIntegracionesRepositorio
{
    /// <summary>Registra un intento de envio de correo y devuelve el identificador generado.</summary>
    Task<int> RegistrarCorreoAsync(int idReserva, ResultadoCorreo resultado, Enums.TipoNotificacion tipo,
                                   int? idUsuario, CancellationToken cancelacion);

    Task<IReadOnlyList<CorreoEnviado>> ConsultarCorreosAsync(FiltroCorreoDto filtro, CancellationToken cancelacion);

    /// <summary>Registra el resultado de un analisis de IA (exitoso o fallido) y devuelve su identificador.</summary>
    Task<int> RegistrarAnalisisAsync(int idReserva, AnalisisIAResultado resultado, int? idUsuario, CancellationToken cancelacion);

    Task<IReadOnlyList<AnalisisIA>> ConsultarAnalisisAsync(FiltroAnalisisDto filtro, CancellationToken cancelacion);

    /// <summary>Ultimo analisis EXITOSO de una reserva; es el que habilita la confirmacion.</summary>
    Task<AnalisisIA?> ObtenerUltimoAnalisisAsync(int idReserva, CancellationToken cancelacion);
}

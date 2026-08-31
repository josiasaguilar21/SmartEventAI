using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using SmartEvent.Application.Sesion;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Exceptions;

namespace SmartEvent.Application.Servicios;

public interface IServicioAuditoria
{
    Task<IReadOnlyList<CorreoEnviado>> ConsultarCorreosAsync(FiltroCorreoDto filtro, CancellationToken cancelacion);
    Task<IReadOnlyList<AnalisisIA>> ConsultarAnalisisAsync(FiltroAnalisisDto filtro, CancellationToken cancelacion);
    Task<AnalisisIA?> ObtenerUltimoAnalisisAsync(int idReserva, CancellationToken cancelacion);

    /// <summary>Deserializa el JSON guardado de un analisis para mostrarlo con formato.</summary>
    AnalisisIARespuesta? InterpretarRespuesta(string? respuestaJson);

    /// <summary>Reformatea el JSON con sangria para mostrarlo legible en la pantalla de auditoria.</summary>
    string FormatearJson(string? respuestaJson);
}

/// <summary>
/// Consulta de la auditoria de integraciones que alimenta FrmAuditoriaIntegraciones.
///
/// Los mensajes de error tecnicos SI se muestran en esta pantalla: es su proposito, permitir
/// diagnosticar. Lo que nunca contienen es un secreto, porque quien los genera (el servicio de
/// correo y el de IA) redacta mensajes controlados que jamas incluyen credenciales, claves ni
/// cadenas de conexion.
/// </summary>
public sealed class ServicioAuditoria : IServicioAuditoria
{
    private static readonly JsonSerializerOptions OpcionesLectura = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions OpcionesFormato = new()
    {
        WriteIndented = true,

        // Sin esto, el serializador escapa todo lo que no sea ASCII y la pantalla de auditoria
        // mostraria la secuencia de escape (una barra invertida seguida de u y cuatro digitos)
        // en lugar de la letra acentuada. El escape existe para evitar
        // inyeccion al incrustar JSON dentro de HTML o JavaScript; aqui el texto se vuelca en un
        // cuadro de texto de Windows Forms, que no interpreta marcado, asi que no aplica.
        // El JSON que se PERSISTE no se toca: esto solo afecta a como se muestra.
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement)
    };

    private readonly IAuditoriaIntegracionesRepositorio _auditoria;
    private readonly IContextoSesion _contexto;
    private readonly IRegistroEventos _registro;

    public ServicioAuditoria(IAuditoriaIntegracionesRepositorio auditoria, IContextoSesion contexto,
                             IRegistroEventos registro)
    {
        _auditoria = auditoria;
        _contexto = contexto;
        _registro = registro;
    }

    public Task<IReadOnlyList<CorreoEnviado>> ConsultarCorreosAsync(FiltroCorreoDto filtro,
                                                                    CancellationToken cancelacion)
    {
        ExigirPermiso();
        return _auditoria.ConsultarCorreosAsync(filtro, cancelacion);
    }

    public Task<IReadOnlyList<AnalisisIA>> ConsultarAnalisisAsync(FiltroAnalisisDto filtro,
                                                                  CancellationToken cancelacion)
    {
        ExigirPermiso();
        return _auditoria.ConsultarAnalisisAsync(filtro, cancelacion);
    }

    public Task<AnalisisIA?> ObtenerUltimoAnalisisAsync(int idReserva, CancellationToken cancelacion) =>
        _auditoria.ObtenerUltimoAnalisisAsync(idReserva, cancelacion);

    public AnalisisIARespuesta? InterpretarRespuesta(string? respuestaJson)
    {
        if (string.IsNullOrWhiteSpace(respuestaJson))
        {
            return null;
        }

        try
        {
            var respuesta = JsonSerializer.Deserialize<AnalisisIARespuesta>(respuestaJson, OpcionesLectura);

            // Se revalida el contrato al leerlo: un registro antiguo o manipulado no debe
            // mostrarse como si fuera valido.
            if (respuesta is not null && respuesta.Validar(out _))
            {
                return respuesta;
            }

            return respuesta;
        }
        catch (JsonException ex)
        {
            _registro.Advertencia("El JSON de un analisis almacenado no pudo interpretarse.", ex);
            return null;
        }
    }

    public string FormatearJson(string? respuestaJson)
    {
        if (string.IsNullOrWhiteSpace(respuestaJson))
        {
            return string.Empty;
        }

        try
        {
            using var documento = JsonDocument.Parse(respuestaJson);
            return JsonSerializer.Serialize(documento.RootElement, OpcionesFormato);
        }
        catch (JsonException)
        {
            // Si no es JSON valido se devuelve el texto tal cual: en auditoria interesa ver
            // exactamente lo que se guardo, no una version maquillada.
            return respuestaJson;
        }
    }

    private void ExigirPermiso()
    {
        var sesion = _contexto.Requerida;

        if (!sesion.PuedeVerAuditoriaIntegraciones)
        {
            throw new ReglaNegocioException(
                $"Su rol ({sesion.Rol}) no tiene acceso a la auditoria de integraciones.");
        }
    }
}

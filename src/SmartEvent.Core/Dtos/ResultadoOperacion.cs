namespace SmartEvent.Core.Dtos;

/// <summary>
/// Resultado uniforme de las operaciones de catalogo: el identificador afectado y el mensaje
/// que el procedimiento almacenado redacto para el usuario.
/// </summary>
public sealed class ResultadoOperacion
{
    public required int Id { get; init; }
    public required string Mensaje { get; init; }
}

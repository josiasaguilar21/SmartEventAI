using System.Text;

namespace SmartEvent.Application.Validacion;

/// <summary>Un problema concreto detectado en la validacion, asociado al campo que lo origina.</summary>
public sealed class ErrorValidacion
{
    public required string Campo { get; init; }
    public required string Mensaje { get; init; }
}

/// <summary>
/// Resultado de una validacion de negocio ejecutada en el cliente.
///
/// Se acumulan TODOS los errores en lugar de abortar en el primero: el usuario corrige la
/// pantalla completa de una vez en lugar de descubrir los problemas de uno en uno.
///
/// Esta validacion es de conveniencia. Ninguna regla depende de ella para cumplirse: las
/// mismas comprobaciones existen en SQL Server, que es quien puede evaluarlas contra el estado
/// real y concurrente de la base. Saltarse esta clase no permite guardar una reserva invalida.
/// </summary>
public sealed class ResultadoValidacion
{
    private readonly List<ErrorValidacion> _errores = new();

    public IReadOnlyList<ErrorValidacion> Errores => _errores;

    public bool EsValido => _errores.Count == 0;

    public void Agregar(string campo, string mensaje) =>
        _errores.Add(new ErrorValidacion { Campo = campo, Mensaje = mensaje });

    /// <summary>Agrega el error solo si la condicion indicada NO se cumple.</summary>
    public void Exigir(bool condicion, string campo, string mensaje)
    {
        if (!condicion)
        {
            Agregar(campo, mensaje);
        }
    }

    /// <summary>Nombre del primer campo con error, para devolver el foco en el formulario.</summary>
    public string? PrimerCampoConError => _errores.Count > 0 ? _errores[0].Campo : null;

    /// <summary>Texto listo para un MessageBox: un problema por linea.</summary>
    public string MensajeCompleto()
    {
        if (EsValido)
        {
            return string.Empty;
        }

        var texto = new StringBuilder();

        if (_errores.Count == 1)
        {
            return _errores[0].Mensaje;
        }

        texto.AppendLine("Corrija los siguientes puntos antes de continuar:");
        texto.AppendLine();

        foreach (var error in _errores)
        {
            texto.Append("  - ").AppendLine(error.Mensaje);
        }

        return texto.ToString().TrimEnd();
    }
}

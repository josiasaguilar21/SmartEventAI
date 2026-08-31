namespace SmartEvent.UI.Comun;

/// <summary>
/// Utilidades para ejecutar operaciones asincronicas desde un formulario sin congelar la
/// interfaz y sin repetir en cada boton el mismo try/catch/finally.
///
/// PATRON QUE SE SIGUE EN TODA LA APLICACION:
///   - Los manejadores de eventos son 'async void' (es el unico caso legitimo de async void:
///     un manejador de eventos no tiene a quien devolverle la tarea).
///   - Toda excepcion se captura DENTRO del manejador; una excepcion que escape de un
///     async void terminaria el proceso.
///   - Mientras la operacion esta en curso se deshabilitan los controles implicados y se
///     muestra el cursor de espera, para que el usuario no dispare la misma accion dos veces.
///   - Jamas se usa .Result, .Wait() ni Thread.Sleep: eso bloquearia el hilo de la interfaz,
///     que es exactamente lo que produce la ventana "no responde".
/// </summary>
public static class EjecucionUi
{
    /// <summary>Ejecuta una operacion sin resultado, gestionando cursor, controles y errores.</summary>
    public static async Task<bool> EjecutarAsync(
        Control propietario,
        Func<CancellationToken, Task> operacion,
        CancellationToken cancelacion,
        string? contexto = null,
        params Control[] controlesADeshabilitar)
    {
        ArgumentNullException.ThrowIfNull(propietario);
        ArgumentNullException.ThrowIfNull(operacion);

        var formulario = propietario as Form ?? propietario.FindForm();
        var estadoPrevio = Deshabilitar(controlesADeshabilitar);

        var cursorPrevio = formulario?.Cursor ?? Cursors.Default;

        if (formulario is not null)
        {
            formulario.Cursor = Cursors.WaitCursor;
        }

        try
        {
            await operacion(cancelacion).ConfigureAwait(true);
            return true;
        }
        catch (OperationCanceledException)
        {
            // El usuario cancelo o se cerro la pantalla: no hay nada que informar.
            return false;
        }
        catch (Exception ex)
        {
            ManejadorErrores.Mostrar(formulario, ex, contexto);
            return false;
        }
        finally
        {
            // Restaurar SIEMPRE, tambien si hubo error: de lo contrario la pantalla quedaria
            // bloqueada con el cursor de espera puesto.
            if (formulario is not null && !formulario.IsDisposed)
            {
                formulario.Cursor = cursorPrevio;
            }

            Restaurar(estadoPrevio);
        }
    }

    /// <summary>Version con resultado. Devuelve el valor por defecto si la operacion falla o se cancela.</summary>
    public static async Task<T?> EjecutarAsync<T>(
        Control propietario,
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancelacion,
        string? contexto = null,
        params Control[] controlesADeshabilitar)
    {
        ArgumentNullException.ThrowIfNull(propietario);
        ArgumentNullException.ThrowIfNull(operacion);

        var formulario = propietario as Form ?? propietario.FindForm();
        var estadoPrevio = Deshabilitar(controlesADeshabilitar);
        var cursorPrevio = formulario?.Cursor ?? Cursors.Default;

        if (formulario is not null)
        {
            formulario.Cursor = Cursors.WaitCursor;
        }

        try
        {
            return await operacion(cancelacion).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            return default;
        }
        catch (Exception ex)
        {
            ManejadorErrores.Mostrar(formulario, ex, contexto);
            return default;
        }
        finally
        {
            if (formulario is not null && !formulario.IsDisposed)
            {
                formulario.Cursor = cursorPrevio;
            }

            Restaurar(estadoPrevio);
        }
    }

    private static List<(Control Control, bool Habilitado)> Deshabilitar(Control[] controles)
    {
        var estado = new List<(Control, bool)>();

        foreach (var control in controles)
        {
            if (control is null || control.IsDisposed)
            {
                continue;
            }

            estado.Add((control, control.Enabled));
            control.Enabled = false;
        }

        return estado;
    }

    private static void Restaurar(List<(Control Control, bool Habilitado)> estado)
    {
        foreach (var (control, habilitado) in estado)
        {
            if (!control.IsDisposed)
            {
                control.Enabled = habilitado;
            }
        }
    }
}

/// <summary>
/// Gestiona el token de cancelacion de una pantalla que relanza la misma consulta muchas veces
/// (por ejemplo, al escribir en un filtro).
///
/// Cada nueva llamada CANCELA la anterior: sin esto, una busqueda lenta podria terminar
/// despues de otra mas reciente y pintar en la grilla resultados que ya no corresponden a lo
/// que el usuario tiene escrito.
/// </summary>
public sealed class OperacionCancelable : IDisposable
{
    private CancellationTokenSource? _origen;
    private readonly CancellationToken _tokenDelFormulario;

    public OperacionCancelable(CancellationToken tokenDelFormulario)
    {
        _tokenDelFormulario = tokenDelFormulario;
    }

    /// <summary>Cancela la operacion en curso, si la hay, y devuelve un token para la nueva.</summary>
    public CancellationToken Reiniciar()
    {
        Cancelar();

        // El token resultante se cancela tanto si llega otra peticion como si se cierra el
        // formulario: asi ninguna tarea sobrevive a la pantalla que la lanzo.
        _origen = CancellationTokenSource.CreateLinkedTokenSource(_tokenDelFormulario);

        return _origen.Token;
    }

    public void Cancelar()
    {
        if (_origen is null)
        {
            return;
        }

        try
        {
            if (!_origen.IsCancellationRequested)
            {
                _origen.Cancel();
            }
        }
        catch (ObjectDisposedException)
        {
            // Ya estaba liberado; no hay nada que cancelar.
        }
        finally
        {
            _origen.Dispose();
            _origen = null;
        }
    }

    public void Dispose() => Cancelar();
}

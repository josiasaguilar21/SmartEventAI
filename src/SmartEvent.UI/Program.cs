using SmartEvent.Core.Exceptions;
using SmartEvent.UI.Comun;
using SmartEvent.UI.Composicion;
using SmartEvent.UI.Configuracion;
using SmartEvent.UI.Formularios;

// La capa de negocio se llama SmartEvent.Application y Windows Forms expone una clase estatica
// llamada Application. Dentro del espacio de nombres SmartEvent.UI, el compilador resuelve
// 'Application' hacia el espacio de nombres propio antes que hacia la clase de Windows Forms.
// El alias elimina la ambiguedad de forma explicita, sin renombrar la capa ni escribir el
// nombre completo en cada uso.
using AplicacionWinForms = System.Windows.Forms.Application;

namespace SmartEvent.UI;

/// <summary>
/// Punto de entrada de la aplicacion.
///
/// Secuencia de arranque:
///   1. Se resuelve la configuracion (variables de entorno, User Secrets, appsettings.json).
///   2. Se construye el grafo de dependencias una sola vez, en ContenedorServicios.
///   3. Se instala la red de seguridad para excepciones no controladas.
///   4. Se muestra el inicio de sesion. Solo si la autenticacion es correcta se abre el MDI.
///
/// Si la configuracion falla, la aplicacion no arranca a medias: muestra un mensaje que
/// explica exactamente que falta y termina de forma ordenada.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        ConfiguracionAplicacion configuracion;

        try
        {
            configuracion = CargadorConfiguracion.Cargar();
        }
        catch (ConfiguracionException ex)
        {
            MessageBox.Show(ex.Message, "SmartEvent AI - Configuracion",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "No fue posible leer la configuracion de la aplicacion.\n\n" +
                "Verifique que el archivo appsettings.json tenga un formato JSON valido.\n\n" +
                $"Detalle: {ex.Message}",
                "SmartEvent AI - Configuracion", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var servicios = new ContenedorServicios(configuracion);

        ManejadorErrores.Inicializar(servicios.Registro);
        InstalarRedDeSeguridad(servicios);

        // Inicio de sesion. Si el usuario cierra la ventana sin autenticarse, la aplicacion
        // termina sin llegar a construir el formulario principal.
        using (var login = new FrmLogin(servicios))
        {
            if (login.ShowDialog() != DialogResult.OK)
            {
                return;
            }
        }

        AplicacionWinForms.Run(new FrmPrincipal(servicios));
    }

    /// <summary>
    /// Red de seguridad para excepciones que escapen de cualquier manejador.
    ///
    /// Sin esto, una excepcion no controlada cierra la aplicacion de golpe mostrando un
    /// dialogo del sistema con el stack trace completo, que es justamente lo que no debe
    /// verse. Con esto, el fallo se registra y el usuario recibe un mensaje neutro.
    /// </summary>
    private static void InstalarRedDeSeguridad(ContenedorServicios servicios)
    {
        AplicacionWinForms.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        AplicacionWinForms.ThreadException += (_, argumentos) =>
        {
            ManejadorErrores.Mostrar(null, argumentos.Exception, "hilo de la interfaz");
        };

        AppDomain.CurrentDomain.UnhandledException += (_, argumentos) =>
        {
            if (argumentos.ExceptionObject is Exception excepcion)
            {
                servicios.Registro.Error("Excepcion no controlada fuera del hilo de la interfaz.", excepcion);
            }
        };

        // Tareas cuyas excepciones nunca se observaron. Se registran para poder diagnosticarlas
        // y se marcan como observadas para que no terminen el proceso.
        TaskScheduler.UnobservedTaskException += (_, argumentos) =>
        {
            servicios.Registro.Error("Excepcion no observada en una tarea en segundo plano.", argumentos.Exception);
            argumentos.SetObserved();
        };
    }
}

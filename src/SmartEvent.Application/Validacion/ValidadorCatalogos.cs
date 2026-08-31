using System.Net.Mail;
using SmartEvent.Core.Entities;

namespace SmartEvent.Application.Validacion;

/// <summary>
/// Validaciones de los catalogos antes de enviarlos a la base.
/// La unicidad (identificacion de cliente, nombre de salon o de recurso) NO se valida aqui:
/// comprobarla con un SELECT previo seria una condicion de carrera. La impone la restriccion
/// UNIQUE de la tabla y la comprueba el procedimiento almacenado dentro de la misma operacion.
/// </summary>
public static class ValidadorCatalogos
{
    public static ResultadoValidacion ValidarCliente(Cliente cliente)
    {
        ArgumentNullException.ThrowIfNull(cliente);

        var resultado = new ResultadoValidacion();

        var identificacion = cliente.Identificacion?.Trim() ?? string.Empty;
        var nombres = cliente.Nombres?.Trim() ?? string.Empty;
        var email = cliente.Email?.Trim() ?? string.Empty;

        resultado.Exigir(identificacion.Length >= 5, nameof(cliente.Identificacion),
            "La identificacion debe tener al menos 5 caracteres.");

        resultado.Exigir(identificacion.Length <= 20, nameof(cliente.Identificacion),
            "La identificacion no puede superar los 20 caracteres.");

        resultado.Exigir(identificacion.All(c => char.IsLetterOrDigit(c) || c == '-'),
            nameof(cliente.Identificacion),
            "La identificacion solo admite letras, numeros y guiones.");

        resultado.Exigir(nombres.Length >= 3, nameof(cliente.Nombres),
            "Escriba el nombre o razon social del cliente (minimo 3 caracteres).");

        resultado.Exigir(nombres.Length <= 150, nameof(cliente.Nombres),
            "El nombre no puede superar los 150 caracteres.");

        // El correo es obligatorio: sin un correo valido la reserva no se podra confirmar.
        if (string.IsNullOrWhiteSpace(email))
        {
            resultado.Agregar(nameof(cliente.Email),
                "El correo electronico es obligatorio: sin el no sera posible confirmar reservas de este cliente.");
        }
        else
        {
            resultado.Exigir(EsCorreoValido(email), nameof(cliente.Email),
                $"El correo '{email}' no tiene un formato valido.");
        }

        if (!string.IsNullOrWhiteSpace(cliente.Telefono))
        {
            var telefono = cliente.Telefono.Trim();

            resultado.Exigir(telefono.Length <= 20, nameof(cliente.Telefono),
                "El telefono no puede superar los 20 caracteres.");

            resultado.Exigir(telefono.All(c => char.IsDigit(c) || c is '+' or '-' or ' ' or '(' or ')'),
                nameof(cliente.Telefono),
                "El telefono solo admite numeros y los simbolos + - ( ) y espacios.");
        }

        return resultado;
    }

    public static ResultadoValidacion ValidarSalon(Salon salon)
    {
        ArgumentNullException.ThrowIfNull(salon);

        var resultado = new ResultadoValidacion();
        var nombre = salon.Nombre?.Trim() ?? string.Empty;

        resultado.Exigir(nombre.Length >= 3, nameof(salon.Nombre),
            "El nombre del salon debe tener al menos 3 caracteres.");

        resultado.Exigir(nombre.Length <= 100, nameof(salon.Nombre),
            "El nombre del salon no puede superar los 100 caracteres.");

        resultado.Exigir(salon.Capacidad > 0, nameof(salon.Capacidad),
            "La capacidad debe ser mayor que cero.");

        resultado.Exigir(salon.Capacidad <= 100_000, nameof(salon.Capacidad),
            "La capacidad indicada no es razonable. Revise el valor.");

        resultado.Exigir(salon.TarifaBase >= 0, nameof(salon.TarifaBase),
            "La tarifa base no puede ser negativa.");

        if (!string.IsNullOrWhiteSpace(salon.Ubicacion))
        {
            resultado.Exigir(salon.Ubicacion.Trim().Length <= 150, nameof(salon.Ubicacion),
                "La ubicacion no puede superar los 150 caracteres.");
        }

        return resultado;
    }

    public static ResultadoValidacion ValidarRecurso(Recurso recurso)
    {
        ArgumentNullException.ThrowIfNull(recurso);

        var resultado = new ResultadoValidacion();
        var nombre = recurso.Nombre?.Trim() ?? string.Empty;

        resultado.Exigir(nombre.Length >= 3, nameof(recurso.Nombre),
            "El nombre del recurso debe tener al menos 3 caracteres.");

        resultado.Exigir(nombre.Length <= 100, nameof(recurso.Nombre),
            "El nombre del recurso no puede superar los 100 caracteres.");

        resultado.Exigir(recurso.StockTotal >= 0, nameof(recurso.StockTotal),
            "El stock no puede ser negativo.");

        resultado.Exigir(recurso.PrecioUnitario >= 0, nameof(recurso.PrecioUnitario),
            "El precio unitario no puede ser negativo.");

        return resultado;
    }

    /// <summary>
    /// Comprobacion estructural del correo, alineada con el CHECK de la tabla evt.Cliente:
    /// texto, arroba, dominio, punto y extension, sin espacios. Se usa MailAddress en lugar de
    /// una expresion regular propia porque una expresion regular "completa" para correo es una
    /// fuente clasica de falsos negativos.
    /// </summary>
    public static bool EsCorreoValido(string? correo)
    {
        if (string.IsNullOrWhiteSpace(correo) || correo.Contains(' '))
        {
            return false;
        }

        if (!MailAddress.TryCreate(correo.Trim(), out var direccion) || direccion is null)
        {
            return false;
        }

        var partes = direccion.Host.Split('.', StringSplitOptions.RemoveEmptyEntries);

        // Se exige dominio con al menos un punto y una extension de dos o mas caracteres,
        // que es justo lo que expresa el patron LIKE del CHECK en la base.
        return partes.Length >= 2 && partes[^1].Length >= 2 && direccion.User.Length >= 1;
    }
}

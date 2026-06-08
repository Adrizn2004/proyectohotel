using System.Text.Json.Serialization;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();
app.UseCors();

// Configuración de base de datos — cambie usuario/contraseña según su instalación de MySQL
const string ConnectionString = "Server=localhost;User ID=root;Password=root;Database=hotel_db";

async Task<MySqlConnection> AbrirConexionAsync()
{
    var conexion = new MySqlConnection(ConnectionString);
    await conexion.OpenAsync();
    return conexion;
}

app.MapPost("/api/login", async (LoginRequest datos) =>
{
    await using var conexion = await AbrirConexionAsync();
    await using var cmd = new MySqlCommand(
        "SELECT id_usuario, correo, nombre, rol FROM usuarios WHERE correo = @correo AND contrasena = @contrasena", conexion);
    cmd.Parameters.AddWithValue("@correo", datos.Correo);
    cmd.Parameters.AddWithValue("@contrasena", datos.Contrasena);

    await using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        var rol = reader.GetString("rol");
        var redireccion = rol == "administrador" ? "admin/admin.html" : "cliente/cliente.html";
        return Results.Ok(new
        {
            estado = "exito",
            redireccion,
            usuario = new
            {
                id_usuario = reader.GetInt32("id_usuario"),
                correo = reader.GetString("correo"),
                nombre = reader.GetString("nombre"),
                rol
            }
        });
    }

    return Results.Json(new { estado = "error", mensaje = "Credenciales inválidas" }, statusCode: 401);
});

app.MapGet("/api/habitaciones", async () =>
{
    await using var conexion = await AbrirConexionAsync();
    const string consulta = """
        SELECT id_habitacion, numero, tipo, nombre, tamano, cama, precio, amenidades, imagen_clase,
               incluye_desayuno, tiene_jacuzzi, tiene_wifi
        FROM habitaciones
        ORDER BY numero
        """;
    await using var cmd = new MySqlCommand(consulta, conexion);
    await using var reader = await cmd.ExecuteReaderAsync();

    var habitaciones = new List<object>();
    while (await reader.ReadAsync())
    {
        habitaciones.Add(new
        {
            id_habitacion = reader.GetInt32("id_habitacion"),
            numero = reader.GetString("numero"),
            tipo = reader.GetString("tipo"),
            nombre = reader.GetString("nombre"),
            tamano = reader.GetString("tamano"),
            cama = reader.GetString("cama"),
            precio = reader.GetDecimal("precio"),
            amenidades = reader.GetString("amenidades").Split('|', StringSplitOptions.RemoveEmptyEntries),
            imagen_clase = reader.GetString("imagen_clase"),
            servicios = new
            {
                desayuno = reader.GetBoolean("incluye_desayuno"),
                jacuzzi = reader.GetBoolean("tiene_jacuzzi"),
                wifi = reader.GetBoolean("tiene_wifi")
            }
        });
    }

    return Results.Ok(habitaciones);
});

app.MapPost("/api/registro", async (RegistroRequest datos) =>
{
    if (string.IsNullOrWhiteSpace(datos.Correo) || string.IsNullOrWhiteSpace(datos.Contrasena))
    {
        return Results.Json(new { estado = "error", mensaje = "Correo y contraseña son obligatorios" }, statusCode: 400);
    }

    await using var conexion = await AbrirConexionAsync();

    await using (var verificar = new MySqlCommand("SELECT id_usuario FROM usuarios WHERE correo = @correo", conexion))
    {
        verificar.Parameters.AddWithValue("@correo", datos.Correo);
        var existente = await verificar.ExecuteScalarAsync();
        if (existente is not null)
        {
            return Results.Json(new { estado = "error", mensaje = "Ya existe una cuenta con ese correo" }, statusCode: 409);
        }
    }

    await using var insertar = new MySqlCommand(
        "INSERT INTO usuarios (correo, nombre, contrasena, rol) VALUES (@correo, @nombre, @contrasena, 'cliente')", conexion);
    insertar.Parameters.AddWithValue("@correo", datos.Correo);
    insertar.Parameters.AddWithValue("@nombre", datos.Nombre ?? "");
    insertar.Parameters.AddWithValue("@contrasena", datos.Contrasena);
    await insertar.ExecuteNonQueryAsync();

    return Results.Ok(new { estado = "exito", mensaje = "Cuenta creada correctamente" });
});

app.MapPut("/api/usuario/perfil", async (ActualizarPerfilRequest datos) =>
{
    if (string.IsNullOrWhiteSpace(datos.Correo) || string.IsNullOrWhiteSpace(datos.Nombre))
    {
        return Results.Json(new { estado = "error", mensaje = "Correo y nombre son obligatorios" }, statusCode: 400);
    }

    await using var conexion = await AbrirConexionAsync();
    await using var actualizar = new MySqlCommand(
        "UPDATE usuarios SET nombre = @nombre WHERE correo = @correo", conexion);
    actualizar.Parameters.AddWithValue("@nombre", datos.Nombre.Trim());
    actualizar.Parameters.AddWithValue("@correo", datos.Correo);
    var filas = await actualizar.ExecuteNonQueryAsync();

    if (filas == 0)
    {
        return Results.Json(new { estado = "error", mensaje = "Usuario no encontrado" }, statusCode: 404);
    }

    return Results.Ok(new { estado = "exito", mensaje = "Nombre actualizado correctamente", nombre = datos.Nombre.Trim() });
});

app.MapGet("/api/admin/reservas", async () =>
{
    await using var conexion = await AbrirConexionAsync();
    const string consulta = """
        SELECT r.id_reserva, r.fecha_registro, u.correo, d.habitacion, d.fecha_ingreso, d.fecha_salida, d.precio_unitario
        FROM reservas r
        INNER JOIN detalle_reservas d ON r.id_reserva = d.id_reserva
        INNER JOIN usuarios u ON r.id_usuario = u.id_usuario
        """;
    await using var cmd = new MySqlCommand(consulta, conexion);
    await using var reader = await cmd.ExecuteReaderAsync();

    var reservas = new List<object>();
    while (await reader.ReadAsync())
    {
        reservas.Add(new
        {
            id_reserva = reader.GetInt32("id_reserva"),
            fecha_registro = reader.GetDateTime("fecha_registro").ToString("yyyy-MM-dd HH:mm"),
            correo = reader.GetString("correo"),
            habitacion = reader.GetString("habitacion"),
            fecha_ingreso = reader.GetDateTime("fecha_ingreso").ToString("yyyy-MM-dd"),
            fecha_salida = reader.GetDateTime("fecha_salida").ToString("yyyy-MM-dd"),
            precio_unitario = reader.GetDecimal("precio_unitario")
        });
    }

    return Results.Ok(reservas);
});

app.MapPost("/api/soporte", async (SoporteRequest datos) =>
{
    if (string.IsNullOrWhiteSpace(datos.Correo) || string.IsNullOrWhiteSpace(datos.Tipo) ||
        string.IsNullOrWhiteSpace(datos.Asunto) || string.IsNullOrWhiteSpace(datos.Mensaje))
    {
        return Results.Json(new { estado = "error", mensaje = "Faltan datos para registrar la solicitud" }, statusCode: 400);
    }

    await using var conexion = await AbrirConexionAsync();

    int idUsuario;
    await using (var buscarUsuario = new MySqlCommand("SELECT id_usuario FROM usuarios WHERE correo = @correo", conexion))
    {
        buscarUsuario.Parameters.AddWithValue("@correo", datos.Correo);
        var resultado = await buscarUsuario.ExecuteScalarAsync();
        if (resultado is null)
        {
            return Results.Json(new { estado = "error", mensaje = "Usuario no encontrado" }, statusCode: 404);
        }
        idUsuario = Convert.ToInt32(resultado);
    }

    await using var insertar = new MySqlCommand(
        "INSERT INTO soporte (id_usuario, tipo, asunto, mensaje) VALUES (@id_usuario, @tipo, @asunto, @mensaje)", conexion);
    insertar.Parameters.AddWithValue("@id_usuario", idUsuario);
    insertar.Parameters.AddWithValue("@tipo", datos.Tipo);
    insertar.Parameters.AddWithValue("@asunto", datos.Asunto);
    insertar.Parameters.AddWithValue("@mensaje", datos.Mensaje);
    await insertar.ExecuteNonQueryAsync();

    var idSolicitud = insertar.LastInsertedId;
    var folio = $"SOP-{idSolicitud:D6}";

    return Results.Ok(new { estado = "exito", mensaje = "Solicitud registrada correctamente", folio });
});

app.MapPut("/api/admin/reservas/{id:int}", async (int id, EditarReservaRequest datos) =>
{
    if (string.IsNullOrWhiteSpace(datos.Habitacion) || string.IsNullOrWhiteSpace(datos.FechaIngreso) ||
        string.IsNullOrWhiteSpace(datos.FechaSalida))
    {
        return Results.Json(new { estado = "error", mensaje = "Faltan datos para actualizar la reserva" }, statusCode: 400);
    }

    await using var conexion = await AbrirConexionAsync();
    await using var actualizar = new MySqlCommand(
        "UPDATE detalle_reservas SET habitacion = @habitacion, fecha_ingreso = @fecha_ingreso, fecha_salida = @fecha_salida WHERE id_reserva = @id_reserva",
        conexion);
    actualizar.Parameters.AddWithValue("@habitacion", datos.Habitacion);
    actualizar.Parameters.AddWithValue("@fecha_ingreso", datos.FechaIngreso);
    actualizar.Parameters.AddWithValue("@fecha_salida", datos.FechaSalida);
    actualizar.Parameters.AddWithValue("@id_reserva", id);
    var filas = await actualizar.ExecuteNonQueryAsync();

    if (filas == 0)
    {
        return Results.Json(new { estado = "error", mensaje = "Reserva no encontrada" }, statusCode: 404);
    }

    return Results.Ok(new { estado = "exito", mensaje = "Reserva actualizada correctamente" });
});

app.MapDelete("/api/admin/reservas/{id:int}", async (int id) =>
{
    await using var conexion = await AbrirConexionAsync();
    await using var eliminar = new MySqlCommand("DELETE FROM reservas WHERE id_reserva = @id_reserva", conexion);
    eliminar.Parameters.AddWithValue("@id_reserva", id);
    var filas = await eliminar.ExecuteNonQueryAsync();

    if (filas == 0)
    {
        return Results.Json(new { estado = "error", mensaje = "Reserva no encontrada" }, statusCode: 404);
    }

    return Results.Ok(new { estado = "exito", mensaje = "Reserva cancelada correctamente" });
});

app.MapPost("/api/reservas", async (ReservaRequest datos) =>
{
    if (string.IsNullOrWhiteSpace(datos.Correo) || string.IsNullOrWhiteSpace(datos.Habitacion) ||
        string.IsNullOrWhiteSpace(datos.FechaIngreso) || string.IsNullOrWhiteSpace(datos.FechaSalida) ||
        datos.PrecioUnitario <= 0)
    {
        return Results.Json(new { estado = "error", mensaje = "Faltan datos para crear la reserva" }, statusCode: 400);
    }

    await using var conexion = await AbrirConexionAsync();

    int idUsuario;
    await using (var buscarUsuario = new MySqlCommand("SELECT id_usuario FROM usuarios WHERE correo = @correo", conexion))
    {
        buscarUsuario.Parameters.AddWithValue("@correo", datos.Correo);
        var resultado = await buscarUsuario.ExecuteScalarAsync();
        if (resultado is null)
        {
            return Results.Json(new { estado = "error", mensaje = "Usuario no encontrado" }, statusCode: 404);
        }
        idUsuario = Convert.ToInt32(resultado);
    }

    using var transaccion = conexion.BeginTransaction();

    long idReserva;
    await using (var crearReserva = new MySqlCommand("INSERT INTO reservas (id_usuario) VALUES (@id_usuario)", conexion, transaccion))
    {
        crearReserva.Parameters.AddWithValue("@id_usuario", idUsuario);
        await crearReserva.ExecuteNonQueryAsync();
        idReserva = crearReserva.LastInsertedId;
    }

    await using (var crearDetalle = new MySqlCommand(
        "INSERT INTO detalle_reservas (id_reserva, habitacion, fecha_ingreso, fecha_salida, precio_unitario) VALUES (@id_reserva, @habitacion, @fecha_ingreso, @fecha_salida, @precio_unitario)",
        conexion, transaccion))
    {
        crearDetalle.Parameters.AddWithValue("@id_reserva", idReserva);
        crearDetalle.Parameters.AddWithValue("@habitacion", datos.Habitacion);
        crearDetalle.Parameters.AddWithValue("@fecha_ingreso", datos.FechaIngreso);
        crearDetalle.Parameters.AddWithValue("@fecha_salida", datos.FechaSalida);
        crearDetalle.Parameters.AddWithValue("@precio_unitario", datos.PrecioUnitario);
        await crearDetalle.ExecuteNonQueryAsync();
    }

    await transaccion.CommitAsync();

    return Results.Ok(new { estado = "exito", mensaje = "Reserva creada correctamente", id_reserva = idReserva });
});

app.MapGet("/api/reservas/mias", async (string? correo) =>
{
    if (string.IsNullOrWhiteSpace(correo))
    {
        return Results.Json(new { estado = "error", mensaje = "Correo requerido" }, statusCode: 400);
    }

    await using var conexion = await AbrirConexionAsync();
    const string consulta = """
        SELECT r.id_reserva, d.habitacion, d.fecha_ingreso, d.fecha_salida, d.precio_unitario, r.fecha_registro
        FROM reservas r
        INNER JOIN detalle_reservas d ON r.id_reserva = d.id_reserva
        INNER JOIN usuarios u ON r.id_usuario = u.id_usuario
        WHERE u.correo = @correo
        ORDER BY r.id_reserva DESC
        """;
    await using var cmd = new MySqlCommand(consulta, conexion);
    cmd.Parameters.AddWithValue("@correo", correo);
    await using var reader = await cmd.ExecuteReaderAsync();

    var reservas = new List<object>();
    while (await reader.ReadAsync())
    {
        reservas.Add(new
        {
            id_reserva = reader.GetInt32("id_reserva"),
            habitacion = reader.GetString("habitacion"),
            fecha_ingreso = reader.GetDateTime("fecha_ingreso").ToString("yyyy-MM-dd"),
            fecha_salida = reader.GetDateTime("fecha_salida").ToString("yyyy-MM-dd"),
            precio_unitario = reader.GetDecimal("precio_unitario"),
            fecha_registro = reader.GetDateTime("fecha_registro")
        });
    }

    return Results.Ok(reservas);
});

app.Run("http://localhost:5000");

record LoginRequest(string Correo, string Contrasena);

record RegistroRequest(string Correo, string Contrasena, string? Nombre);

record ActualizarPerfilRequest(string Correo, string Nombre);

record SoporteRequest(string Correo, string Tipo, string Asunto, string Mensaje);

record EditarReservaRequest(
    string Habitacion,
    [property: JsonPropertyName("fecha_ingreso")] string FechaIngreso,
    [property: JsonPropertyName("fecha_salida")] string FechaSalida
);

record ReservaRequest(
    string Correo,
    string Habitacion,
    [property: JsonPropertyName("fecha_ingreso")] string FechaIngreso,
    [property: JsonPropertyName("fecha_salida")] string FechaSalida,
    [property: JsonPropertyName("precio_unitario")] decimal PrecioUnitario
);

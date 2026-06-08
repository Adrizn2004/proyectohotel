Guía de Ejecución: Plataforma Hotelera (MVP)
Esta guía detalla los pasos técnicos necesarios para levantar la arquitectura del sistema (Base de Datos, Backend y Frontend) en un entorno de desarrollo local.

1. Preparar la Base de Datos (MySQL)
El backend requiere una conexión activa a la base de datos para autenticar usuarios y gestionar reservas.

Inicie su servidor MySQL local (mediante XAMPP, WAMP o servicio nativo).

Abra su cliente de administración de bases de datos (ej. MySQL Workbench o consola).

Ejecute el script alojado en database/init.sql para crear la base de datos hotel_db, sus tablas y los registros de prueba.

Abra el archivo backend/Program.cs en su editor de código.

Modifique la constante ConnectionString (usuario y contraseña) para que coincida con su configuración local de MySQL.

2. Levantar el Servidor Backend (C# / ASP.NET Core)
El servidor proveerá la API de autenticación y los datos estructurados en formato JSON. Requiere tener instalado el SDK de .NET 8 (https://dotnet.microsoft.com/download).

Abra una terminal de comandos.

Navegue al directorio del backend: cd ruta/hacia/plataforma-hotel/backend

Restaure las dependencias del proyecto ejecutando: dotnet restore

Inicie el servidor ejecutando: dotnet run

Verifique que la terminal indique que el servicio está activo en el puerto 5000 (ej. http://localhost:5000). No cierre esta terminal.

3. Levantar la Interfaz Frontend (HTML/JS)
La interfaz gráfica debe servirse a través de un protocolo HTTP local para evitar bloqueos por políticas de seguridad (CORS).

Abra una nueva pestaña o ventana de terminal.

Navegue al directorio del frontend: cd ruta/hacia/plataforma-hotel/fronted

Inicie el servidor web de desarrollo ejecutando: python -m http.server 8000

(Nota: Si utiliza un editor de código moderno, puede optar por abrir index.html mediante una extensión como Live Server).

4. Pruebas de Sistema
Con las capas de datos, servidor y cliente en ejecución, la plataforma está lista para pruebas funcionales.

Abra su navegador web y diríjase a: http://localhost:8000

Prueba de Administrador: Ingrese con el correo admin@hotel.com y la contraseña 123456. El sistema debe redirigir a admin/admin.html y cargar el reporte de reservas.

Prueba de Cliente: Retorne al login e ingrese con el correo cliente@hotel.com y la contraseña 123456. El sistema debe redirigir a cliente/cliente.html mostrando el catálogo de habitaciones. Desde ahí puede navegar a Mi Cuenta, Mis Reservas, Soporte, Servicios y crear una reserva real (queda guardada en la base de datos).
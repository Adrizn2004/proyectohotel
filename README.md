# Hotel Mour — Plataforma de Gestión Hotelera

Sistema web para la gestión de un hotel: catálogo de habitaciones, reservas, panel de administración, soporte/PQR y cuentas de cliente/administrador.

**Arquitectura:**
- **Backend**: C# / ASP.NET Core (Minimal API) + MySQL — carpeta [`backend/`](backend/)
- **Base de datos**: MySQL — script de creación en [`database/init.sql`](database/init.sql)
- **Frontend**: HTML, CSS y JavaScript puro (sin frameworks) — carpeta [`fronted/`](fronted/)

---

## 1. Requisitos previos (instalar una sola vez)

| Herramienta | Para qué | Descarga |
|---|---|---|
| **Git** | Clonar el repositorio | https://git-scm.com/download/win |
| **.NET SDK 10** | Compilar y correr el backend | https://dotnet.microsoft.com/download/dotnet/10.0 |
| **MySQL Server** | Base de datos | https://dev.mysql.com/downloads/installer/ |
| **MySQL Workbench** (opcional) | Ver y editar la base de datos con interfaz gráfica | incluido en el instalador de MySQL |

Durante la instalación de MySQL, **anota el usuario y la contraseña de root** — los necesitarás más adelante.

Verifica que todo quedó instalado abriendo una terminal (PowerShell) y corriendo:
```powershell
git --version
dotnet --version
mysql --version
```

---

## 2. Clonar el proyecto

```powershell
git clone <URL-de-tu-repositorio-en-GitHub>
cd proyectohotel
```

---

## 3. Crear la base de datos

Con el servicio de MySQL corriendo, ejecuta el script [`database/init.sql`](database/init.sql). Desde la terminal:

```powershell
Get-Content database\init.sql | mysql -u root -p
```
> **Nota**: en PowerShell el operador `<` no sirve para redirigir archivos (a diferencia de Bash/CMD) — por eso se usa `Get-Content ... | mysql ...` para enviarle el script a MySQL.

(te pedirá la contraseña de root). Esto crea la base `hotel_db`, todas las tablas (`usuarios`, `habitaciones`, `reservas`, `detalle_reservas`, `soporte`) y la cuenta de administrador semilla.

Si prefieres usar una interfaz gráfica: abre **MySQL Workbench**, conéctate a tu servidor local, abre el archivo `database/init.sql` y ejecútalo con el botón ⚡ (rayo).

---

## 4. Configurar la conexión del backend

Abre [`backend/Program.cs`](backend/Program.cs), línea 15, y ajusta el usuario/contraseña si son distintos a `root`/`root`:

```csharp
const string ConnectionString = "Server=localhost;User ID=root;Password=root;Database=hotel_db";
```

---

## 5. Levantar el backend

```powershell
cd backend
dotnet restore
dotnet run
```
Debe quedar escuchando en `http://localhost:5000` — **deja esta terminal abierta** mientras uses el sistema.

---

## 6. Levantar el frontend

Abre **otra terminal** (no cierres la del backend):

```powershell
cd fronted
python -m http.server 8000
```
(Si no tienes Python, instala la extensión **Live Server** en VS Code y haz clic derecho sobre `fronted/index.html` → "Open with Live Server".)

---

## 7. Probar que todo funciona

Abre el navegador en `http://localhost:8000` e inicia sesión con la cuenta de administrador semilla:
- **Correo**: `admin@hotel.com`
- **Contraseña**: `123456`

Desde ahí puedes crear cuentas de cliente nuevas, reservar habitaciones, etc. — todo se guarda en tu base de datos local en tiempo real.

---

## 8. Cómo hacer modificaciones comunes

### a) Cambiar el precio de una habitación (o cualquier otro dato del catálogo)

Las habitaciones viven en la tabla `habitaciones` de MySQL. Hay dos formas de cambiarlas:

**Opción 1 — Editar datos ya existentes (recomendado para cambios puntuales de precio):**
Abre MySQL Workbench (o la consola de `mysql`) y ejecuta una sentencia `UPDATE`. Por ejemplo, para subir el precio de la habitación 101 a $95:
```sql
UPDATE habitaciones SET precio = 95.00 WHERE numero = '101';
```
O para cambiar el precio de **todas** las habitaciones VIP:
```sql
UPDATE habitaciones SET precio = 200.00 WHERE tipo = 'vip';
```
El cambio aparece de inmediato en el catálogo del cliente (`/api/habitaciones`) sin reiniciar nada — el backend siempre lee directamente de la base de datos.

**Opción 2 — Cambiar los valores semilla del proyecto (para que el precio nuevo quede así desde una instalación limpia):**
Edita las sentencias `INSERT INTO habitaciones` en [`database/init.sql`](database/init.sql:61) (alrededor de la línea 61) y ajusta el valor del precio (columna `precio`) en cada fila. Esto solo afecta a instalaciones futuras que ejecuten el script desde cero — **no modifica una base de datos que ya existe** (para eso usa la Opción 1).

### b) Agregar, quitar o modificar habitaciones

Para **agregar** una habitación nueva, inserta una fila en la tabla `habitaciones`:
```sql
INSERT INTO habitaciones (numero, tipo, nombre, tamano, cama, precio, amenidades, imagen_clase, incluye_desayuno, tiene_jacuzzi, tiene_wifi)
VALUES ('104', 'estandar', 'Habitación Estándar', '28 m²', 'Cama Queen', 85.00,
        'WiFi gratis|TV LCD|Aire acondicionado', 'img-standard', FALSE, FALSE, TRUE);
```
- `tipo` debe ser uno de: `estandar`, `vip`, `suite`.
- `amenidades` es una lista separada por el carácter `|` (así es como el backend la divide para mostrarla en tarjetas individuales).
- `imagen_clase` controla qué estilo visual usa la tarjeta — usa `img-standard`, `img-vip` o `img-suite` para mantener la consistencia con el diseño existente.

Para **eliminar** una habitación (solo si no tiene reservas activas referenciándola):
```sql
DELETE FROM habitaciones WHERE numero = '104';
```

### c) Modificar la base de datos (estructura: tablas, columnas, relaciones)

- La estructura completa (tablas, columnas, llaves foráneas) está definida en [`database/init.sql`](database/init.sql). Es el "plano" de la base de datos.
- Si necesitas agregar una columna o tabla nueva a una base de datos **que ya existe y tiene datos**, usa sentencias `ALTER TABLE` directamente en MySQL Workbench, por ejemplo:
  ```sql
  ALTER TABLE usuarios ADD COLUMN telefono VARCHAR(20);
  ```
- Después, **actualiza también `init.sql`** para que una instalación nueva desde cero quede igual que la tuya (agrega la columna en el `CREATE TABLE` correspondiente). Así mantienes ambos sincronizados.
- Cualquier columna/tabla nueva normalmente requiere también:
  1. Actualizar las consultas SQL en [`backend/Program.cs`](backend/Program.cs) (los `SELECT`/`INSERT`/`UPDATE` que usan esa tabla).
  2. Actualizar los `record` de C# que mapean los datos JSON (al final de `Program.cs`).
  3. Actualizar el HTML/JS en `fronted/` que muestra o envía esos datos.

### d) Modificar o agregar funciones (endpoints del backend)

Todos los endpoints de la API viven en [`backend/Program.cs`](backend/Program.cs), definidos con `app.MapGet`, `app.MapPost`, `app.MapPut` o `app.MapDelete`. Por ejemplo:

```csharp
app.MapGet("/api/habitaciones", async () => { ... });
```

Para **modificar** uno existente, busca su ruta (ej. `/api/habitaciones`) y edita la consulta SQL o la lógica dentro del bloque.

Para **agregar uno nuevo**, copia la estructura de uno parecido y cambia la ruta, el verbo HTTP y la lógica. Recuerda:
- Usar siempre `cmd.Parameters.AddWithValue(...)` para los valores que vienen del usuario (evita inyección SQL).
- Si el endpoint recibe datos en el cuerpo (POST/PUT), define un `record` al final del archivo con los campos esperados.

Después de cualquier cambio en `Program.cs`, **detén el backend** (`Ctrl+C` en su terminal) y vuelve a correr `dotnet run` para que tome los cambios.

### e) Modificar el frontend (textos, estilos, comportamiento de botones)

Cada página vive en `fronted/` (cliente en `fronted/cliente/`, administrador en `fronted/admin/`, login y registro en la raíz de `fronted/`). Cada archivo `.html` contiene su propio `<style>` (CSS) y `<script>` (JavaScript) — no hay un build ni compilación, así que **cualquier cambio que guardes se ve recargando la página en el navegador**.

- Para cambiar textos o estilos: edita directamente el HTML/CSS de la página correspondiente.
- Para cambiar el comportamiento (qué pasa al hacer clic en un botón, qué datos se piden al backend, etc.): edita el `<script>` de esa página — busca la función relacionada (suelen tener nombres descriptivos en español, ej. `cargarReservas`, `confirmarCancelacionReserva`).
- Las llamadas al backend usan siempre `fetch('http://localhost:5000/api/...')`. Si cambias la ruta o el puerto del backend, deberás actualizar esas URLs.

---

## 9. Estructura del proyecto (resumen)

```
proyectohotel/
├── backend/
│   ├── Program.cs            ← toda la API (endpoints, lógica, conexión a MySQL)
│   └── HotelMourApi.csproj   ← configuración del proyecto .NET
├── database/
│   └── init.sql              ← script que crea la base de datos y sus tablas
└── fronted/
    ├── index.html            ← login
    ├── registro.html         ← creación de cuenta
    ├── cliente/              ← páginas del cliente (catálogo, reservas, cuenta, soporte...)
    └── admin/                ← páginas del administrador (panel de reservas, cuenta...)
```

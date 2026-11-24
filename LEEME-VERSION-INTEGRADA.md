# Librería El Saber – Versión integrada

Incluye: main (Productos, Categorías, Compras, Proveedores) + Ventas (PR #3) + Login (PR #4),
con los arreglos de la revisión.

## Cómo ejecutar
1. Abrir `WebApplication1.slnx` en Visual Studio (la primera vez descarga los paquetes NuGet, incluido Bogus).
2. Clic derecho en la solución -> Configurar proyectos de inicio -> Varios proyectos:
   WebApplication1 = Iniciar, Web = Iniciar.
3. Ejecutar. Al arrancar, la API **crea sola la base de datos** y **siembra los datos**
   (la primera vez tarda unos segundos).
4. Entrar con **admin / 123456**.

La cadena de conexión usa `(localdb)\MSSQLLocalDB` (viene con Visual Studio) y la base `LibreriaElSaber`.
Si usas SQL Server Express u otro servidor, cambia `ConexionDB` en `WebApplication1/appsettings.json`
SOLO en tu PC y no la subas al repositorio.

## Datos de prueba (WebApplication1/Factories/SemillasFactory.cs)
Mínimo **100 registros por tabla**, generados con Bogus:
- 100 categorías, 100 productos, 100 proveedores (85% activos, NIT únicos).
- 100 compras con su detalle (solo a proveedores activos; **suben** el stock).
- 100 ventas con su detalle y su factura (**bajan** el stock, nunca queda negativo).
- 100 usuarios (10% administradores). Todos con contraseña **123456**, guardada **encriptada**.
Si una tabla ya tiene datos, solo completa lo que falta para llegar a 100.

## Arreglos incluidos
- **Menú**: diseño azul del Login con TODOS los módulos (Ventas, Compras, Proveedores, Usuarios).
- **Login**: contraseñas encriptadas con PasswordHasher (los usuarios antiguos sin encriptar
  pueden entrar y su contraseña se encripta sola). Protección de páginas en Web/Program.cs:
  sin sesión -> Login; Compras y Usuarios -> solo Administrador.
- **Productos**: botones Editar (modal) y Eliminar; precio en Bs; stock bajo en rojo.
  La API no deja eliminar un producto que ya tiene compras o ventas.
- **Compras**: arreglado el bug de borrar una fila del medio (se perdían productos);
  moneda en Bs; solo se listan proveedores ACTIVOS.
- **Categorías, Productos y Compras**: mensaje claro si la API no está corriendo (antes error 500).

## Seguridad de la API con JWT (como en clase)
- `POST /api/Auth/login` devuelve un **token** (dura 8 horas) con el nombre y el **rol** del usuario.
- Todos los controllers llevan `[Authorize]`: sin token la API responde **401**.
  - Compras y la lista/eliminación de Usuarios: `[Authorize(Roles = "Administrador")]`.
  - Login y Registrar son públicos. Si alguien se registra sin ser admin, queda como "Usuario"
    (nadie puede hacerse Administrador desde Swagger).
- **Swagger**: ahora sale el botón 🔒 **Authorize**. Para probar: haz login en
  `POST /api/Auth/login` (admin / 123456), copia el `token`, aprieta Authorize y pégalo (sin escribir "Bearer").
- **Web**: al iniciar sesión guarda el token en la sesión y `Web/Servicios/TokenHandler.cs` lo pega
  solo en cada pedido a la API. Si el token vence, te manda al Login.
- Paquete nuevo en la API: `Microsoft.AspNetCore.Authentication.JwtBearer`. La clave está en
  `appsettings.json` -> `Jwt:Clave`.

## Proveedores: al menos un contacto
- Al registrar o editar, es obligatorio **teléfono o correo** (al menos uno). La regla está en
  `Contratos/ProveedorDto.cs` (IValidatableObject), así se revisa en la Web y en la API.
- Un proveedor antiguo sin contacto no se puede activar/desactivar hasta agregarle uno con Editar.

## Permisos del rol Usuario (solo puede ver)
- **Proveedores**: solo Administrador (oculto en el menú, la página redirige al Inicio y la API responde 403).
- **Productos**: el Usuario solo ve la lista (sin Nuevo, Editar ni Eliminar). En la API, POST/PUT/DELETE son solo para Administrador.
- **Categorías**: el Usuario solo ve la lista (sin Nueva categoría). En la API, POST es solo para Administrador.
- **Ventas** (nueva venta, historial y facturas): solo Administrador. El Usuario no lo ve y la API le responde 403.

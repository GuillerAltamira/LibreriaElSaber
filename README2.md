# Librería El Saber – Sistema de Gestión

Proyecto ASP.NET Core Web API con Entity Framework Core para la Librería y Papelería "El Saber". Reemplaza el control manual de ventas e inventario en cuadernos y hojas de cálculo.

## Requisitos

- .NET 10 SDK
- Visual Studio 2022 o superior (con la carga de trabajo ASP.NET y desarrollo web)
- SQL Server LocalDB (se instala junto con Visual Studio)

## Tecnologías y versiones

| Componente | Versión |
| --- | --- |
| .NET | 10.0 |
| Entity Framework Core | 10.0.15 |
| Proveedor de base de datos | SQL Server (LocalDB) |
| Documentación de la API | Swagger (Swashbuckle) |

## Configurar la conexión

La cadena de conexión está en `WebApplication1/appsettings.json`:

```json
"ConnectionStrings": {
  "ConexionDB": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LibreriaElSaber;Integrated Security=True;Trust Server Certificate=True"
}
```

Usa autenticación de Windows, por lo que no requiere usuario ni contraseña. Si se usa otro servidor SQL Server, solo hay que cambiar el valor de `Data Source`.

## Crear o actualizar la base de datos

1. Abrir la solución `WebApplication1.slnx` en Visual Studio.
2. Abrir **Herramientas → Administrador de paquetes NuGet → Consola del Administrador de paquetes**.
3. Ejecutar:

```
Update-Database
```

Esto crea la base `LibreriaElSaber` con todas las tablas a partir de las migraciones.

Para agregar cambios al modelo:

```
Add-Migration NombreDeLaMigracion
Update-Database
```

## Ejecutar el proyecto

- En Visual Studio: presionar **F5** o el botón **https**.
- Por consola, dentro de la carpeta `WebApplication1`:

```
dotnet run
```

Luego abrir `https://localhost:<puerto>/swagger` para probar la API.

## Módulos

### Gestión de Productos – Grupo Zuñigas
Registrar, modificar, eliminar productos y consultar stock.
Endpoints: `/api/Producto`

### Gestión de Proveedores – Grupo Nucitas
Registrar proveedores, actualizar su información y consultar su historial de compras.
Endpoints: `/api/Proveedor`
Tablas: `proveedores`, `compras`, `detalle_compras`
DTOs en Contratos: `ProveedorDto`, `HistorialCompraDto`, `ResumenProveedorDto`
Endpoint extra: `GET /api/Proveedor/resumen-compras` (compras y total por proveedor)
Páginas Web: `/Proveedores` (tarjetas, gráficos Top 5 y estado, buscador, filtro, orden, paginación, exportar a Excel, registrar/editar en modal, activar/desactivar) y `/Proveedores/Historial/{id}` (gráfico por mes, detalle de facturas, imprimir/PDF)

### Gestión de Ventas – Grupo GarantesTatianaMarset
_(Completar por el grupo)_

### Gestión de Compras
_(Completar por el grupo asignado)_

### Gestión de Inventario
_(Completar por el grupo asignado)_

## Trabajo colaborativo

Antes de modificar el proyecto, hacer **Git → Extraer (Pull)** para tener la última versión. Antes de crear una migración, verificar que no haya migraciones nuevas de otros grupos sin descargar.

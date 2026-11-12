# Diseño de Arquitecturas Distribuidas — Comercio y Distribución Multibodega (AD01)

* **Institución:** Instituto Superior Tecnológico Cordillera
* **Carrera:** Desarrollo de Software
* **Asignatura:** Aplicaciones Distribuidas (`DISTR`)
* **Nivel / Paralelo:** Cuarto A Matutino (`4AM`) — Bimestre 1 (`B1`)
* **Actividad:** Actividad Docente 01 (`AD01`)
* **Estudiante:** Franklin Rivadeneira

---

## 1. Arquitectura de Microservicios

El sistema implementa **3 microservicios REST independientes** desarrollados con **ASP.NET Core Web API** y **SQL Server** (`(localdb)\MSSQLLocalDB`), siguiendo el principio *Database per Service*:

| Microservicio | Puerto Local | Base de Datos | Entidades Dominio |
| :--- | :--- | :--- | :--- |
| **`Catalogo.Api`** | `http://localhost:5101` | `CatalogoDB` | `Categoria`, `Marca`, `Producto` |
| **`Inventario.Api`** | `http://localhost:5102` | `InventarioDB` | `Bodega`, `Existencia`, `MovimientoInventario`, `ReservaStock` |
| **`Pedidos.Api`** | `http://localhost:5103` | `PedidosDB` | `Cliente`, `Pedido`, `DetallePedido` |

---

## 2. Reglas de Llaves Foráneas (Físicas vs. Lógicas)

1. **Llaves Foráneas Físicas (dentro de la misma base de datos):**
   * En **`CatalogoDB`**: `Producto.IdCategoria -> Categoria.IdCategoria` y `Producto.IdMarca -> Marca.IdMarca`.
   * En **`InventarioDB`**: `Existencia.IdBodega`, `MovimientoInventario.IdBodega` y `ReservaStock.IdBodega` referencian físicamente a `Bodega.IdBodega`.
   * En **`PedidosDB`**: `Pedido.IdCliente -> Cliente.IdCliente` y `DetallePedido.IdPedido -> Pedido.IdPedido`.
2. **Llaves Foráneas Lógicas (entre microservicios independientes vía HTTP):**
   * `IdProducto` en `InventarioDB` (`Existencia`, `MovimientoInventario`, `ReservaStock`) y en `PedidosDB` (`DetallePedido`) se valida mediante llamadas HTTP (`HttpClient`) hacia `Catalogo.Api` (`GET /api/productos/{id}`), garantizando bajo acoplamiento a nivel de base de datos.
   * `Pedidos.Api` además consulta la disponibilidad de stock en `Inventario.Api` (`GET /api/existencias/producto/{idProducto}/stock-total`) antes de confirmar un pedido.

---

## 3. Estructura del Repositorio

* `Catalogo.Api/` — Microservicio de catálogo comercial (`CatalogoDB`).
* `Inventario.Api/` — Microservicio de bodegas, existencias, movimientos y reservas (`InventarioDB`).
* `Pedidos.Api/` — Microservicio de clientes, órdenes de compra y detalles (`PedidosDB`).
* `SQL/` — Scripts SQL DDL y datos semilla para `CatalogoDB`, `InventarioDB` y `PedidosDB`.
* `Postman/` — Colección de Postman (`.postman_collection.json`) con casos de prueba `200 OK`, `201 Created`, `400 Bad Request` y `404 Not Found`.
* `INICIAR_PROYECTO_AD01.bat` — Script ejecutable para levantar los 3 microservicios simultáneamente.

---

## 4. Ejecución Rápida

1. Ejecutar `INICIAR_PROYECTO_AD01.bat` (o iniciar cada proyecto con `dotnet run`). Al arrancar, cada microservicio crea y puebla automáticamente su base de datos en `(localdb)\MSSQLLocalDB`.
2. Importar el archivo `Postman/AD01_ComercioMultibodega_Microservicios.postman_collection.json` en Postman y ejecutar las peticiones de prueba.

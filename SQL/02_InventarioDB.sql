-- =========================================================================
-- MICROSERVICIO 2: Inventario.Api
-- Base de Datos: InventarioDB
-- Responsabilidad: Gestionar el almacenamiento y movimientos físicos.
-- Nota de Arquitectura:
--   * IdBodega es FK FÍSICA hacia dbo.Bodega (misma base de datos).
--   * IdProducto es FK LÓGICA hacia Catalogo.Api (CatalogoDB), validada vía HTTP.
-- =========================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'InventarioDB')
BEGIN
    CREATE DATABASE InventarioDB;
END
GO

USE InventarioDB;
GO

IF OBJECT_ID('dbo.ReservaStock', 'U') IS NOT NULL DROP TABLE dbo.ReservaStock;
IF OBJECT_ID('dbo.MovimientoInventario', 'U') IS NOT NULL DROP TABLE dbo.MovimientoInventario;
IF OBJECT_ID('dbo.Existencia', 'U') IS NOT NULL DROP TABLE dbo.Existencia;
IF OBJECT_ID('dbo.Bodega', 'U') IS NOT NULL DROP TABLE dbo.Bodega;
GO

CREATE TABLE dbo.Bodega (
    IdBodega INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Ciudad NVARCHAR(100) NOT NULL,
    Direccion NVARCHAR(200) NOT NULL
);
GO

CREATE TABLE dbo.Existencia (
    IdExistencia INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdProducto INT NOT NULL, -- FK LÓGICA (referencia a Producto en Catalogo.Api)
    IdBodega INT NOT NULL,   -- FK FÍSICA (referencia a Bodega en InventarioDB)
    Stock INT NOT NULL CHECK (Stock >= 0),
    CONSTRAINT FK_Existencia_Bodega FOREIGN KEY (IdBodega)
        REFERENCES dbo.Bodega (IdBodega) ON DELETE NO ACTION,
    CONSTRAINT UQ_Existencia_Producto_Bodega UNIQUE (IdProducto, IdBodega)
);
GO

CREATE TABLE dbo.MovimientoInventario (
    IdMovimiento INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdProducto INT NOT NULL, -- FK LÓGICA (referencia a Producto en Catalogo.Api)
    IdBodega INT NOT NULL,   -- FK FÍSICA (referencia a Bodega en InventarioDB)
    Tipo NVARCHAR(30) NOT NULL, -- ENTRADA | SALIDA | AJUSTE
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    Fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_MovimientoInventario_Bodega FOREIGN KEY (IdBodega)
        REFERENCES dbo.Bodega (IdBodega) ON DELETE NO ACTION
);
GO

CREATE TABLE dbo.ReservaStock (
    IdReserva INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdProducto INT NOT NULL, -- FK LÓGICA (referencia a Producto en Catalogo.Api)
    IdBodega INT NOT NULL,   -- FK FÍSICA (referencia a Bodega en InventarioDB)
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    Estado NVARCHAR(30) NOT NULL DEFAULT 'Reservado', -- Reservado | Confirmado | Cancelado
    CONSTRAINT FK_ReservaStock_Bodega FOREIGN KEY (IdBodega)
        REFERENCES dbo.Bodega (IdBodega) ON DELETE NO ACTION
);
GO

-- Datos semilla iniciales
INSERT INTO dbo.Bodega (Nombre, Ciudad, Direccion) VALUES
('Bodega Central Norte', 'Quito', 'Av. Galo Plaza Lasso N52-100 y Capitán Ramón Borja'),
('Bodega Distribución Sur', 'Guayaquil', 'Km 14.5 Vía Daule, Parque Industrial Pascuales');

INSERT INTO dbo.Existencia (IdProducto, IdBodega, Stock) VALUES
(1, 1, 25),
(2, 1, 40),
(3, 1, 100),
(1, 2, 15),
(2, 2, 30);

INSERT INTO dbo.MovimientoInventario (IdProducto, IdBodega, Tipo, Cantidad, Fecha) VALUES
(1, 1, 'ENTRADA', 25, SYSDATETIME()),
(2, 1, 'ENTRADA', 40, SYSDATETIME()),
(3, 1, 'ENTRADA', 100, SYSDATETIME());

INSERT INTO dbo.ReservaStock (IdProducto, IdBodega, Cantidad, Estado) VALUES
(1, 1, 2, 'Confirmado');
GO

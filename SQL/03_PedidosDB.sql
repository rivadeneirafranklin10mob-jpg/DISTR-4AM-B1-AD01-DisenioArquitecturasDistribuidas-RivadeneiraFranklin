-- =========================================================================
-- MICROSERVICIO 3: Pedidos.Api
-- Base de Datos: PedidosDB
-- Responsabilidad: Registrar y administrar las órdenes de compra de clientes.
-- Nota de Arquitectura:
--   * Pedido.IdCliente es FK FÍSICA hacia dbo.Cliente.
--   * DetallePedido.IdPedido es FK FÍSICA hacia dbo.Pedido.
--   * DetallePedido.IdProducto es FK LÓGICA hacia Catalogo.Api (CatalogoDB).
-- =========================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'PedidosDB')
BEGIN
    CREATE DATABASE PedidosDB;
END
GO

USE PedidosDB;
GO

IF OBJECT_ID('dbo.DetallePedido', 'U') IS NOT NULL DROP TABLE dbo.DetallePedido;
IF OBJECT_ID('dbo.Pedido', 'U') IS NOT NULL DROP TABLE dbo.Pedido;
IF OBJECT_ID('dbo.Cliente', 'U') IS NOT NULL DROP TABLE dbo.Cliente;
GO

CREATE TABLE dbo.Cliente (
    IdCliente INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombres NVARCHAR(150) NOT NULL,
    Cedula NVARCHAR(20) NOT NULL UNIQUE,
    Correo NVARCHAR(150) NOT NULL
);
GO

CREATE TABLE dbo.Pedido (
    IdPedido INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdCliente INT NOT NULL, -- FK FÍSICA hacia dbo.Cliente
    Fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Total DECIMAL(18,2) NOT NULL CHECK (Total >= 0),
    Estado NVARCHAR(30) NOT NULL DEFAULT 'Confirmado',
    CONSTRAINT FK_Pedido_Cliente FOREIGN KEY (IdCliente)
        REFERENCES dbo.Cliente (IdCliente) ON DELETE NO ACTION
);
GO

CREATE TABLE dbo.DetallePedido (
    IdDetalle INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdPedido INT NOT NULL,   -- FK FÍSICA hacia dbo.Pedido
    IdProducto INT NOT NULL, -- FK LÓGICA hacia Catalogo.Api (Producto)
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    PrecioUnitario DECIMAL(18,2) NOT NULL CHECK (PrecioUnitario > 0),
    CONSTRAINT FK_DetallePedido_Pedido FOREIGN KEY (IdPedido)
        REFERENCES dbo.Pedido (IdPedido) ON DELETE CASCADE
);
GO

-- Datos semilla iniciales
INSERT INTO dbo.Cliente (Nombres, Cedula, Correo) VALUES
('Franklin Rivadeneira', '1723456789', 'franklin.rivadeneira@cordillera.edu.ec'),
('María Fernanda Gómez', '0912345678', 'maria.gomez@empresa.com.ec');

INSERT INTO dbo.Pedido (IdCliente, Fecha, Total, Estado) VALUES
(1, SYSDATETIME(), 2500.00, 'Confirmado');

INSERT INTO dbo.DetallePedido (IdPedido, IdProducto, Cantidad, PrecioUnitario) VALUES
(1, 1, 2, 1250.00);
GO

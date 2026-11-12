-- =========================================================================
-- MICROSERVICIO 1: Catalogo.Api
-- Base de Datos: CatalogoDB
-- Responsabilidad: Administrar la información comercial de los productos.
-- =========================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CatalogoDB')
BEGIN
    CREATE DATABASE CatalogoDB;
END
GO

USE CatalogoDB;
GO

IF OBJECT_ID('dbo.Producto', 'U') IS NOT NULL DROP TABLE dbo.Producto;
IF OBJECT_ID('dbo.Marca', 'U') IS NOT NULL DROP TABLE dbo.Marca;
IF OBJECT_ID('dbo.Categoria', 'U') IS NOT NULL DROP TABLE dbo.Categoria;
GO

CREATE TABLE dbo.Categoria (
    IdCategoria INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(250) NULL
);
GO

CREATE TABLE dbo.Marca (
    IdMarca INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    PaisOrigen NVARCHAR(100) NOT NULL
);
GO

-- Producto posee FKs FÍSICAS hacia Categoria y Marca (mismo dominio/BD)
CREATE TABLE dbo.Producto (
    IdProducto INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdCategoria INT NOT NULL,
    IdMarca INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    SKU NVARCHAR(50) NOT NULL UNIQUE,
    Precio DECIMAL(18,2) NOT NULL CHECK (Precio > 0),
    Estado NVARCHAR(30) NOT NULL DEFAULT 'Activo',
    CONSTRAINT FK_Producto_Categoria FOREIGN KEY (IdCategoria)
        REFERENCES dbo.Categoria (IdCategoria) ON DELETE NO ACTION,
    CONSTRAINT FK_Producto_Marca FOREIGN KEY (IdMarca)
        REFERENCES dbo.Marca (IdMarca) ON DELETE NO ACTION
);
GO

-- Datos semilla iniciales
INSERT INTO dbo.Categoria (Nombre, Descripcion) VALUES
('Laptops y Portátiles', 'Equipos portátiles empresariales y gaming'),
('Monitores y Pantallas', 'Monitores LED, IPS y curvos de alta resolución'),
('Periféricos y Accesorios', 'Teclados mecánicos, mouses e interfaces USB');

INSERT INTO dbo.Marca (Nombre, PaisOrigen) VALUES
('Dell Technologies', 'Estados Unidos'),
('Samsung Electronics', 'Corea del Sur'),
('Logitech', 'Suiza');

INSERT INTO dbo.Producto (IdCategoria, IdMarca, Nombre, SKU, Precio, Estado) VALUES
(1, 1, 'Laptop Dell Latitude 5540 Core i7 16GB 512GB SSD', 'DELL-LAT-5540', 1250.00, 'Activo'),
(2, 2, 'Monitor Samsung Odyssey G5 27 Pulgadas QHD', 'SAM-ODY-G527', 340.50, 'Activo'),
(3, 3, 'Mouse Inalámbrico Logitech MX Master 3S', 'LOG-MXM-3S', 99.99, 'Activo');
GO

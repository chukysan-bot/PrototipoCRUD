-- ============================================================
-- Minisúper El Ahorro - Script de base de datos (PostgreSQL)
-- Ejecutar primero: CREATE DATABASE minisuper;
-- Luego conectarse a la base "minisuper" y correr este script.
-- ============================================================

DROP TABLE IF EXISTS productos;
DROP TABLE IF EXISTS proveedores;

CREATE TABLE proveedores (
    id          SERIAL PRIMARY KEY,
    nombre      VARCHAR(100) NOT NULL,
    telefono    VARCHAR(20),
    correo      VARCHAR(100)
);

CREATE TABLE productos (
    id                 SERIAL PRIMARY KEY,
    nombre             VARCHAR(100) NOT NULL,
    codigo_barras      VARCHAR(30)  NOT NULL UNIQUE,
    precio             NUMERIC(10,2) NOT NULL CHECK (precio >= 0),
    stock              INT NOT NULL DEFAULT 0 CHECK (stock >= 0),
    stock_minimo       INT NOT NULL DEFAULT 5 CHECK (stock_minimo >= 0),
    fecha_vencimiento  DATE,
    proveedor_id       INT,
    CONSTRAINT fk_producto_proveedor
        FOREIGN KEY (proveedor_id) REFERENCES proveedores(id)
        ON DELETE SET NULL
);

-- Datos de ejemplo
INSERT INTO proveedores (nombre, telefono, correo) VALUES
    ('Distribuidora Andina', '70112233', 'ventas@andina.com'),
    ('Lácteos del Valle',    '71445566', 'pedidos@lacteosvalle.com');

INSERT INTO productos (nombre, codigo_barras, precio, stock, stock_minimo, fecha_vencimiento, proveedor_id) VALUES
    ('Leche 1L',  '7771001', 8.50, 40, 10, CURRENT_DATE + 5,  2),
    ('Arroz 1kg', '7771002', 9.00, 3,  10, NULL,              1);

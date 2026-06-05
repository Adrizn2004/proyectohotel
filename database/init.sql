CREATE DATABASE IF NOT EXISTS hotel_db;
USE hotel_db;

CREATE TABLE usuarios (
    id_usuario INT AUTO_INCREMENT PRIMARY KEY,
    correo VARCHAR(100) UNIQUE NOT NULL,
    contrasena VARCHAR(255) NOT NULL,
    rol ENUM('administrador', 'cliente') NOT NULL
);

CREATE TABLE reservas (
    id_reserva INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    fecha_registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE detalle_reservas (
    id_detalle INT AUTO_INCREMENT PRIMARY KEY,
    id_reserva INT NOT NULL,
    habitacion VARCHAR(50) NOT NULL,
    fecha_ingreso DATE NOT NULL,
    fecha_salida DATE NOT NULL,
    precio_unitario DECIMAL(10,2) NOT NULL
);

-- Datos de prueba
INSERT INTO usuarios (correo, contrasena, rol) VALUES 
('admin@hotel.com', '123456', 'administrador'),
('cliente@hotel.com', '123456', 'cliente');

-- Reservas de prueba para el panel admin
INSERT INTO reservas (id_usuario) VALUES (2), (2), (2), (2);
INSERT INTO detalle_reservas (id_reserva, habitacion, fecha_ingreso, fecha_salida, precio_unitario) VALUES 
(1, '101 (Estándar)', '2026-06-10', '2026-06-15', 50.00),
(2, '201 (VIP)', '2026-06-12', '2026-06-14', 80.00),
(3, '301 (Suite)', '2026-06-20', '2026-06-25', 150.00),
(4, '102 (Estándar)', '2026-07-01', '2026-07-05', 50.00);
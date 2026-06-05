CREATE DATABASE IF NOT EXISTS hotel_db;
USE hotel_db;

CREATE TABLE habitaciones (
    id_habitacion INT AUTO_INCREMENT PRIMARY KEY,
    numero_habitacion VARCHAR(10) NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    precio_noche DECIMAL(10, 2) NOT NULL,
    estado ENUM('disponible', 'ocupada', 'mantenimiento') DEFAULT 'disponible'
);

INSERT INTO habitaciones (numero_habitacion, tipo, precio_noche, estado) VALUES 
('101', 'Sencilla', 50.00, 'disponible'),
('102', 'Doble', 80.00, 'ocupada'),
('201', 'Suite', 150.00, 'disponible');
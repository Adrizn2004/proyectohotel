CREATE DATABASE IF NOT EXISTS hotel_db;
USE hotel_db;

CREATE TABLE usuarios (
    id_usuario INT AUTO_INCREMENT PRIMARY KEY,
    correo VARCHAR(100) UNIQUE NOT NULL,
    nombre VARCHAR(100) NOT NULL DEFAULT '',
    contrasena VARCHAR(255) NOT NULL,
    rol ENUM('administrador', 'cliente') NOT NULL
);

CREATE TABLE reservas (
    id_reserva INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    fecha_registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_reservas_usuario FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario) ON DELETE CASCADE
);

CREATE TABLE habitaciones (
    id_habitacion INT AUTO_INCREMENT PRIMARY KEY,
    numero VARCHAR(10) UNIQUE NOT NULL,
    tipo ENUM('estandar','vip','suite') NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    tamano VARCHAR(20) NOT NULL,
    cama VARCHAR(100) NOT NULL,
    precio DECIMAL(10,2) NOT NULL,
    amenidades VARCHAR(500) NOT NULL,
    imagen_clase VARCHAR(50) NOT NULL,
    incluye_desayuno BOOLEAN NOT NULL DEFAULT FALSE,
    tiene_jacuzzi BOOLEAN NOT NULL DEFAULT FALSE,
    tiene_wifi BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE detalle_reservas (
    id_detalle INT AUTO_INCREMENT PRIMARY KEY,
    id_reserva INT NOT NULL,
    habitacion VARCHAR(50) NOT NULL,
    fecha_ingreso DATE NOT NULL,
    fecha_salida DATE NOT NULL,
    precio_unitario DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_detalle_reserva FOREIGN KEY (id_reserva) REFERENCES reservas(id_reserva) ON DELETE CASCADE
);

CREATE TABLE soporte (
    id_solicitud INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    tipo VARCHAR(30) NOT NULL,
    asunto VARCHAR(150) NOT NULL,
    mensaje TEXT NOT NULL,
    estado ENUM('pendiente','resuelto') NOT NULL DEFAULT 'pendiente',
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_soporte_usuario FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario) ON DELETE CASCADE
);

-- Cuenta de administrador inicial (necesaria para poder entrar al panel admin;
-- el registro público solo crea cuentas de tipo 'cliente')
INSERT INTO usuarios (correo, nombre, contrasena, rol) VALUES
('admin@hotel.com', 'Administrador Hotel', '123456', 'administrador');

-- Catálogo de habitaciones: 9 habitaciones, 3 de cada tipo
INSERT INTO habitaciones (numero, tipo, nombre, tamano, cama, precio, amenidades, imagen_clase, incluye_desayuno, tiene_jacuzzi, tiene_wifi) VALUES
('101','estandar','Habitación Estándar','28 m²','Cama Queen o 2 individuales',80.00,'WiFi gratis|TV LCD|Aire acondicionado|Escritorio|Baño privado','img-standard',FALSE,FALSE,TRUE),
('102','estandar','Habitación Estándar','28 m²','Cama Queen o 2 individuales',80.00,'WiFi gratis|TV LCD|Aire acondicionado|Escritorio|Baño privado','img-standard',FALSE,FALSE,TRUE),
('103','estandar','Habitación Estándar','28 m²','Cama Queen o 2 individuales',80.00,'WiFi gratis|TV LCD|Aire acondicionado|Escritorio|Baño privado','img-standard',FALSE,FALSE,TRUE),
('201','vip','Habitación VIP','38 m²','Cama King',180.00,'WiFi premium|TV Smart|Aire|Jacuzzi privado|Vestidor|Desayuno incluido','img-vip',TRUE,TRUE,TRUE),
('202','vip','Habitación VIP','38 m²','Cama King',180.00,'WiFi premium|TV Smart|Aire|Jacuzzi privado|Vestidor|Desayuno incluido','img-vip',TRUE,TRUE,TRUE),
('203','vip','Habitación VIP','38 m²','Cama King',180.00,'WiFi premium|TV Smart|Aire|Jacuzzi privado|Vestidor|Desayuno incluido','img-vip',TRUE,TRUE,TRUE),
('301','suite','Suite Presidencial','65 m²','Cama King + sala de estar',250.00,'Terraza|WiFi ultra|Jacuzzi|Desayuno buffet|Minibar|Vista panorámica','img-suite',TRUE,TRUE,TRUE),
('302','suite','Suite Presidencial','65 m²','Cama King + sala de estar',250.00,'Terraza|WiFi ultra|Jacuzzi|Desayuno buffet|Minibar|Vista panorámica','img-suite',TRUE,TRUE,TRUE),
('303','suite','Suite Presidencial','65 m²','Cama King + sala de estar',250.00,'Terraza|WiFi ultra|Jacuzzi|Desayuno buffet|Minibar|Vista panorámica','img-suite',TRUE,TRUE,TRUE);
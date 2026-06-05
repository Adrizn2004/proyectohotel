from flask import Flask, request, jsonify
from flask_cors import CORS
import mysql.connector

app = Flask(__name__)
CORS(app)

# Configuración de base de datos
def get_db_connection():
    return mysql.connector.connect(
        host='localhost',
        user='root',         # Cambie por su usuario de MySQL
        password='',         # Cambie por su contraseña de MySQL
        database='hotel_db'
    )

@app.route('/api/login', methods=['POST'])
def login():
    datos = request.json
    correo = datos.get('correo')
    contrasena = datos.get('contrasena')

    conexion = get_db_connection()
    cursor = conexion.cursor(dictionary=True)
    cursor.execute("SELECT rol FROM usuarios WHERE correo = %s AND contrasena = %s", (correo, contrasena))
    usuario = cursor.fetchone()
    conexion.close()

    if usuario:
        redireccion = 'admin.html' if usuario['rol'] == 'administrador' else 'cliente.html'
        return jsonify({"estado": "exito", "redireccion": redireccion})
    
    return jsonify({"estado": "error", "mensaje": "Credenciales inválidas"}), 401

@app.route('/api/admin/reservas', methods=['GET'])
def obtener_reservas():
    conexion = get_db_connection()
    cursor = conexion.cursor(dictionary=True)
    consulta = """
        SELECT r.id_reserva, u.correo, d.habitacion, d.fecha_ingreso, d.fecha_salida, d.precio_unitario 
        FROM reservas r
        INNER JOIN detalle_reservas d ON r.id_reserva = d.id_reserva
        INNER JOIN usuarios u ON r.id_usuario = u.id_usuario
    """
    cursor.execute(consulta)
    reservas = cursor.fetchall()
    conexion.close()
    return jsonify(reservas)

if __name__ == '__main__':
    app.run(debug=True, port=5000)
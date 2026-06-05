from flask import Flask, jsonify, request
from flask_cors import CORS

app = Flask(__name__)
CORS(app) # Permite peticiones desde el frontend

@app.route('/api/disponibilidad', methods=['POST'])
def verificar_disponibilidad():
    datos = request.json
    fecha_ingreso = datos.get('ingreso')
    fecha_salida = datos.get('salida')
    
    # Aquí se implementaría la lógica de consulta a la base de datos
    
    respuesta = {
        "estado": "exito",
        "mensaje": "Habitaciones disponibles",
        "datos": {
            "habitaciones_libres": 5,
            "precio_estimado": 120.00
        }
    }
    return jsonify(respuesta), 200

if __name__ == '__main__':
    app.run(debug=True, port=5000)
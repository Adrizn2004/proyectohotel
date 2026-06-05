document.getElementById('reserva-form').addEventListener('submit', function(e) {
    e.preventDefault();
    
    const ingreso = document.getElementById('fecha-ingreso').value;
    const salida = document.getElementById('fecha-salida').value;
    const resultadoDiv = document.getElementById('resultado');

    resultadoDiv.innerHTML = "<p>Consultando disponibilidad en el servidor...</p>";

    // Simulación de consumo de API (Fetch al backend)
    setTimeout(() => {
        resultadoDiv.innerHTML = `<p style="color: green;">✔ Habitaciones disponibles entre ${ingreso} y ${salida}.</p>`;
    }, 1500);
});
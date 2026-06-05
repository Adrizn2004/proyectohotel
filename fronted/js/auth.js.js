document.getElementById('form-login').addEventListener('submit', async function(e) {
    e.preventDefault();
    const correo = document.getElementById('correo').value;
    const contrasena = document.getElementById('contrasena').value;

    const respuesta = await fetch('http://localhost:5000/api/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ correo, contrasena })
    });

    const resultado = await respuesta.json();
    if (resultado.estado === 'exito') {
        window.location.href = resultado.redireccion;
    } else {
        alert(resultado.mensaje);
    }
});
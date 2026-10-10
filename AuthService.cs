using System;
using System.Security.Cryptography;
using AppInsegura.Datos;
using AppInsegura.Modelos;

namespace AppInsegura.Servicios
{
    public class AuthService
    {
        private readonly BaseDatosUsuarios baseDatos;

        public AuthService(BaseDatosUsuarios baseDatos)
        {
            this.baseDatos = baseDatos;
        }

        public Usuario Registrar(string nombre, string contrasena, string rol = "jugador")
        {
            // Validar la entrada antes de guardar nada.
            if (!Validador.NombreValido(nombre))
            {
                throw new ArgumentException("Nombre no válido: usa de 3 a 20 letras o números.");
            }
            if (!Validador.ContrasenaValida(contrasena))
            {
                throw new ArgumentException("Contraseña no válida: debe tener entre 6 y 100 caracteres.");
            }
            if (baseDatos.BuscarExacto(nombre) != null)
            {
                throw new ArgumentException("Ese nombre de usuario no está disponible.");
            }

            // Cada usuario tiene su propia sal aleatoria.
            byte[] sal = GenerarBytesAleatorios(16);

            var nuevo = new Usuario
            {
                Nombre = nombre,
                ContrasenaSal = Convert.ToBase64String(sal),
                ContrasenaHash = CalcularHash(contrasena, sal),
                Rol = rol,
                TokenSesion = ""
            };

            baseDatos.Agregar(nuevo);
            return nuevo;
        }

        public Usuario? IniciarSesion(string nombre, string contrasena)
        {
            Usuario? usuario = baseDatos.BuscarExacto(nombre);
            if (usuario == null)
            {
                return null;
            }

            byte[] sal = Convert.FromBase64String(usuario.ContrasenaSal);
            string hashIntento = CalcularHash(contrasena, sal);
            if (usuario.ContrasenaHash != hashIntento)
            {
                return null;
            }

            // Token aleatorio seguro. Solo vive en memoria:
            // no se muestra, no se registra y no se guarda en disco.
            usuario.TokenSesion = Convert.ToHexString(GenerarBytesAleatorios(16));

            return usuario;
        }

        // Hash con sal usando un algoritmo lento (PBKDF2), como dicen los apuntes.
        private string CalcularHash(string contrasena, byte[] sal)
        {
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, 100000, HashAlgorithmName.SHA256, 32);
            return Convert.ToBase64String(hash);
        }

        // Generador seguro (igual que el ejemplo de los apuntes). Nada de System.Random.
        private byte[] GenerarBytesAleatorios(int cantidad)
        {
            byte[] bytes = new byte[cantidad];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return bytes;
        }
    }
}

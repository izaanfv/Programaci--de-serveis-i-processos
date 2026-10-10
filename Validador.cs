using System.Linq;

namespace AppInsegura.Servicios
{
    // Validación de entrada: solo se acepta lo que esperamos (lista blanca).
    public static class Validador
    {
        // Nombre: de 3 a 20 caracteres, solo letras y números.
        public static bool NombreValido(string nombre)
        {
            return !string.IsNullOrWhiteSpace(nombre)
                && nombre.Length >= 3
                && nombre.Length <= 20
                && nombre.All(char.IsLetterOrDigit);
        }

        // Contraseña: de 6 a 100 caracteres.
        public static bool ContrasenaValida(string contrasena)
        {
            return !string.IsNullOrWhiteSpace(contrasena)
                && contrasena.Length >= 6
                && contrasena.Length <= 100;
        }
    }
}

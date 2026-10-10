using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppInsegura.Servicios
{
    public class RedService
    {
        // Ahora HTTPS (antes http://).
        private const string UrlServidor = "https://api.miapp-insegura.local/puntuaciones";

        public void EnviarPuntuacion(string nombreUsuario, int puntuacion)
        {
            // La clave ya no está escrita en el código: se lee de una variable de entorno.
            string? apiKey = Environment.GetEnvironmentVariable("APPINSEGURA_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("No se puede sincronizar: falta la configuración.");
                return;
            }

            // No confiar en la entrada: solo nombres válidos viajan en la URL.
            if (!Validador.NombreValido(nombreUsuario))
            {
                Console.WriteLine("No se puede sincronizar: nombre no válido.");
                return;
            }

            try
            {
                EnviarPuntuacionAsync(nombreUsuario, puntuacion, apiKey).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                // Al usuario, un mensaje genérico. El detalle, al registro interno.
                File.AppendAllText("errores.log", ex + Environment.NewLine);
                Console.WriteLine("No se ha podido sincronizar la partida.");
            }
        }

        private async Task EnviarPuntuacionAsync(string nombreUsuario, int puntuacion, string apiKey)
        {
            using var cliente = new HttpClient();

            // La clave va en una cabecera, no dentro de la URL.
            cliente.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
            string url = $"{UrlServidor}?usuario={nombreUsuario}&puntos={puntuacion}";

            // Ya no se muestra la URL ni la clave por pantalla.
            Console.WriteLine("Enviando puntuación...");
            HttpResponseMessage respuesta = await cliente.GetAsync(url);
            Console.WriteLine($"Respuesta del servidor: {(int)respuesta.StatusCode}");
        }
    }
}

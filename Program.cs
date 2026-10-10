using System;
using System.IO;
using AppInsegura.Datos;
using AppInsegura.Modelos;
using AppInsegura.Servicios;

namespace AppInsegura
{
    public class Program
    {
        private static readonly BaseDatosUsuarios baseDatos = new BaseDatosUsuarios();
        private static readonly AuthService auth = new AuthService(baseDatos);
        private static Usuario? usuarioActual = null;

        public static void Main(string[] args)
        {
            CargarUsuariosDeEjemplo();

            Console.WriteLine("=== Gestor de Usuarios y Partidas ===");
            Console.WriteLine();

            bool salir = false;
            while (!salir)
            {
                MostrarMenu();
                string opcion = Console.ReadLine() ?? "";

                try
                {
                    switch (opcion)
                    {
                        case "1":
                            Registrar();
                            break;
                        case "2":
                            IniciarSesion();
                            break;
                        case "3":
                            BuscarUsuario();
                            break;
                        case "4":
                            VerPerfil();
                            break;
                        case "5":
                            PanelAdministracion();
                            break;
                        case "6":
                            SincronizarConServidor();
                            break;
                        case "0":
                            salir = true;
                            break;
                        default:
                            Console.WriteLine("Opción no válida.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Al usuario, un mensaje genérico. El detalle, al registro interno.
                    File.AppendAllText("errores.log", ex + Environment.NewLine);
                    Console.WriteLine("Ha ocurrido un error.");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Hasta luego.");
        }

        private static void CargarUsuariosDeEjemplo()
        {
            auth.Registrar("admin", "admin1234", "admin");
            auth.Registrar("ana", "ana2024", "jugador");
        }

        private static void MostrarMenu()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine($"Usuario actual: {(usuarioActual != null ? usuarioActual.Nombre : "ninguno")}");
            Console.WriteLine("1. Registrar usuario");
            Console.WriteLine("2. Iniciar sesión");
            Console.WriteLine("3. Buscar usuario por nombre");
            Console.WriteLine("4. Ver mi perfil");
            if (usuarioActual != null && usuarioActual.Rol == "admin")
            {
                Console.WriteLine("5. Panel de administración");
            }
            Console.WriteLine("6. Sincronizar partida con el servidor");
            Console.WriteLine("0. Salir");
            Console.Write("Elige una opción: ");
        }

        // Lee la contraseña sin enseñarla: por cada tecla escribe un asterisco (*).
        private static string LeerContrasena()
        {
            string contrasena = "";
            ConsoleKeyInfo tecla = Console.ReadKey(true); // true = no mostrar la tecla
            while (tecla.Key != ConsoleKey.Enter)
            {
                if (tecla.Key == ConsoleKey.Backspace)
                {
                    if (contrasena.Length > 0)
                    {
                        contrasena = contrasena.Substring(0, contrasena.Length - 1);
                        Console.Write("\b \b"); // borra el último asterisco
                    }
                }
                else if (!char.IsControl(tecla.KeyChar))
                {
                    contrasena += tecla.KeyChar;
                    Console.Write("*");
                }
                tecla = Console.ReadKey(true);
            }
            Console.WriteLine();
            return contrasena;
        }

        private static void Registrar()
        {
            Console.Write("Nombre de usuario: ");
            string nombre = Console.ReadLine() ?? "";
            Console.Write("Contraseña: ");
            string contrasena = LeerContrasena();

            try
            {
                Usuario nuevo = auth.Registrar(nombre, contrasena);
                Console.WriteLine($"Usuario '{nuevo.Nombre}' registrado.");
            }
            catch (ArgumentException ex)
            {
                // Estos mensajes los hemos escrito nosotros y son seguros de mostrar.
                Console.WriteLine(ex.Message);
            }
        }

        private static void IniciarSesion()
        {
            Console.Write("Nombre de usuario: ");
            string nombre = Console.ReadLine() ?? "";
            Console.Write("Contraseña: ");
            string contrasena = LeerContrasena();

            Usuario? usuario = auth.IniciarSesion(nombre, contrasena);
            if (usuario == null)
            {
                Console.WriteLine("Usuario o contraseña incorrectos.");
                return;
            }

            usuarioActual = usuario;
            Console.WriteLine($"Bienvenido, {usuario.Nombre}.");
        }

        private static void BuscarUsuario()
        {
            // Hay que haber iniciado sesión para buscar usuarios.
            if (usuarioActual == null)
            {
                Console.WriteLine("Primero debes iniciar sesión.");
                return;
            }

            Console.Write("Nombre a buscar: ");
            string nombre = Console.ReadLine() ?? "";

            if (!Validador.NombreValido(nombre))
            {
                Console.WriteLine("Nombre no válido.");
                return;
            }

            Usuario? encontrado = baseDatos.BuscarPorNombre(nombre);
            // Solo mostramos el nombre, no el rol.
            Console.WriteLine(encontrado != null
                ? $"Encontrado: {encontrado.Nombre}"
                : "No se ha encontrado ningún usuario con ese nombre.");
        }

        private static void VerPerfil()
        {
            if (usuarioActual == null)
            {
                Console.WriteLine("Primero debes iniciar sesión.");
                return;
            }

            Console.WriteLine($"Nombre: {usuarioActual.Nombre}");
            Console.WriteLine($"Rol: {usuarioActual.Rol}");
        }

        private static void PanelAdministracion()
        {
            // Comprobamos el permiso aquí, no solo ocultando la opción del menú.
            // Si no se puede comprobar el rol, no se entra.
            if (usuarioActual == null || usuarioActual.Rol != "admin")
            {
                Console.WriteLine("No tienes permisos para acceder a esta opción.");
                return;
            }

            Console.WriteLine("=== PANEL DE ADMINISTRACIÓN ===");
            Console.WriteLine("Lista de usuarios registrados:");
            foreach (Usuario u in baseDatos.ListarTodos())
            {
                Console.WriteLine($" - {u.Nombre} ({u.Rol})");
            }
        }

        private static void SincronizarConServidor()
        {
            if (usuarioActual == null)
            {
                Console.WriteLine("Primero debes iniciar sesión.");
                return;
            }

            // OJO: en una app real la puntuación la comprobaría el servidor,
            // porque lo que envía el cliente no es de fiar.
            var red = new RedService();
            red.EnviarPuntuacion(usuarioActual.Nombre, 1000);
        }
    }
}

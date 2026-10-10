using System;
using System.Collections.Generic;
using System.Linq;
using AppInsegura.Modelos;

namespace AppInsegura.Datos
{
    // Simula una tabla de base de datos (equivalente a una tabla SQLite/sqflite).
    public class BaseDatosUsuarios
    {
        private readonly List<Usuario> usuarios = new List<Usuario>();

        public void Agregar(Usuario usuario)
        {
            usuarios.Add(usuario);
        }

        public List<Usuario> ListarTodos()
        {
            return usuarios;
        }

        public Usuario? BuscarExacto(string nombre)
        {
            return usuarios.FirstOrDefault(u => u.Nombre == nombre);
        }

        // CONSULTA PARAMETRIZADA. Equivale a:
        //   SELECT * FROM usuarios WHERE nombre = @nombre
        // El texto del usuario se compara siempre como un DATO.
        // Ya no se construye ninguna consulta pegando texto.
        public Usuario? BuscarPorNombre(string nombreBuscado)
        {
            return usuarios.FirstOrDefault(u => u.Nombre == nombreBuscado);
        }
    }
}

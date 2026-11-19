using System;

namespace EcoRutas.Service.DTOs
{
    public class UsuarioListadoDto
    {
        public string Id { get; set; }
        public string CodigoUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
        public DateTime? UltimaConexion { get; set; }
    }

    public class UsuarioGuardarDto
    {
        public string Id { get; set; }
        public string CodigoUsuario { get; set; }   // se genera al crear; solo lectura al editar
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
    }
}

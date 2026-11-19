using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EcoRutas.Service.DTOs;

namespace EcoRutas.Web.Models
{
    // Formulario de editar usuario (y base del de crear).
    public class UsuarioFormViewModel
    {
        // Nombre del rol en la base de datos -> texto que se muestra.
        public static readonly Dictionary<string, string> Roles = new Dictionary<string, string>
        {
            { "Administrador", "Administrador" },
            { "Guia", "Guía" },
            { "Asociado", "Asociado" }
        };

        public static string NombreRol(string rol)
        {
            if (string.IsNullOrEmpty(rol)) return "Sin rol";
            return Roles.ContainsKey(rol) ? Roles[rol] : rol;
        }

        public string Id { get; set; }

        [Display(Name = "Código")]
        public string CodigoUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(120, ErrorMessage = "El nombre no puede superar 120 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [StringLength(256, ErrorMessage = "El correo no puede superar 256 caracteres.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Seleccione un rol.")]
        [Display(Name = "Rol")]
        public string Rol { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }

    // Formulario de crear usuario: igual al de editar, más la contraseña.
    public class UsuarioCrearViewModel : UsuarioFormViewModel
    {
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }
    }

    // Pantalla de lista con filtros.
    public class UsuarioIndexViewModel
    {
        [Display(Name = "Buscar")]
        public string Busqueda { get; set; }

        [Display(Name = "Rol")]
        public string Rol { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; }   // "", "activos" o "inactivos"

        public IList<UsuarioListadoDto> Usuarios { get; set; } = new List<UsuarioListadoDto>();
    }
}

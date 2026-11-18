using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRutas.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(20)]
        public string CodigoUsuario { get; set; }

        [Required]
        [StringLength(120)]
        public string NombreCompleto { get; set; }

        public bool Activo { get; set; }

        public DateTime? UltimaConexion { get; set; }
    }
}

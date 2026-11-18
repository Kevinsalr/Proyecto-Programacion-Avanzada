using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRutas.Data.Entities
{
    public class Tour
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Index("IX_Tour_Codigo", IsUnique = true)]
        [Display(Name = "Código")]
        public string Codigo { get; set; }

        [Required]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required]
        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        [Required]
        [Display(Name = "Dificultad")]
        public string Dificultad { get; set; }

        [Display(Name = "Duración")]
        public int DuracionMinutos { get; set; }

        [Required]
        [Display(Name = "Punto de encuentro")]
        public string PuntoEncuentro { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        public virtual ICollection<Imagen> Imagenes { get; set; } = new HashSet<Imagen>();
        public virtual ICollection<Salida> Salidas { get; set; } = new HashSet<Salida>();
        public virtual ICollection<Resena> Resenas { get; set; } = new HashSet<Resena>();
    }
}         
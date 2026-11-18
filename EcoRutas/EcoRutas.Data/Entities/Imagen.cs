using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRutas.Data.Entities
{
    public class Imagen
    {
        public int Id { get; set; }

        public int TourId { get; set; }

        [Required]
        [Display(Name = "Nombre del archivo")]
        public string NombreArchivo { get; set; }

        [Required]
        [Display(Name = "Tipo de contenido")]
        public string TipoContenido { get; set; }   //Imagen 

        [Required]
        public byte[] Contenido { get; set; }

        public int Orden { get; set; }

        public virtual Tour Tour { get; set; }
    }
}
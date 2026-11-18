using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRutas.Data.Entities
{
    public class Resena
    {
      
            public int ResenaId { get; set; }

            [Required]
            public int TourId { get; set; }

            [Required]
            public string AsociadoId { get; set; }

            [Range(1, 5)]
            public int Calificacion { get; set; }

            [Required]
            [StringLength(1000)]
            public string Comentario { get; set; }

            [Required]
            [StringLength(20)]
            public string Estado { get; set; }

            public DateTime FechaCreacion { get; set; }

            public virtual Tour Tour { get; set; }

            public virtual ApplicationUser Asociado { get; set; }
        }
    }

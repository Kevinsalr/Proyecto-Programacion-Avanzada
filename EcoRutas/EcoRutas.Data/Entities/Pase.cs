using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRutas.Data.Entities
{
    public class Pase
    {
        public int PaseId { get; set; }

        [Required]
        public int DetalleReservaId { get; set; }

        [Required]
        [StringLength(40)]
        [Index("IX_Pase_Codigo", IsUnique = true)]
        public string Codigo { get; set; }

        [Required]
        [StringLength(120)]
        public string NombrePasajero { get; set; }

        [Required]
        [StringLength(30)]
        public string IdentificacionPasajero { get; set; }

        public bool Asistio { get; set; }

        public bool Anulado { get; set; }

        public virtual DetalleReserva DetalleReserva { get; set; }
    }
}
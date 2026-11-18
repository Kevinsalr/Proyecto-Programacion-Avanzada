using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRutas.Data.Entities
{
    public class Salida
    {
        public Salida()
        {
            DetallesReserva = new HashSet<DetalleReserva>();
        }

        public int SalidaId { get; set; }

        [Required]
        public int TourId { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        public string GuiaId { get; set; }

        [Range(typeof(decimal), "0.01", "9999999")]
        public decimal PrecioXPersona { get; set; }

        [Range(1, 500)]
        public int CupoMax { get; set; }

        [Range(0, 500)]
        public int CuposDisponibles { get; set; }

        public bool Activa { get; set; }

        [Timestamp]
        public byte[] VersionFila { get; set; }

        public virtual Tour Tour { get; set; }

        [ForeignKey("GuiaId")]
        public virtual ApplicationUser Guia { get; set; }

        public virtual ICollection<DetalleReserva> DetallesReserva { get; set; }
    }
}

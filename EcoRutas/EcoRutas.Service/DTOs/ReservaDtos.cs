using System;
using System.Collections.Generic;
using EcoRutas.Data.Entities;

namespace EcoRutas.Service.DTOs
{
    // ---------- Datos para reservar (vienen del carrito) ----------

    public class ReservaLineaDto
    {
        public int SalidaId { get; set; }
        public List<PasajeroDto> Pasajeros { get; set; } = new List<PasajeroDto>();
    }

    public class PasajeroDto
    {
        public string Nombre { get; set; }
        public string Identificacion { get; set; }
    }

    // ---------- Datos para mostrar (confirmación e historial) ----------

    public class ReservaDto
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public decimal Total { get; set; }   // suma de las líneas que no están canceladas
        public List<DetalleReservaDto> Detalles { get; set; } = new List<DetalleReservaDto>();
    }

    public class DetalleReservaDto
    {
        public int Id { get; set; }
        public int SalidaId { get; set; }
        public string TourNombre { get; set; }
        public string PuntoEncuentro { get; set; }
        public DateTime FechaHora { get; set; }
        public int CantidadPasajeros { get; set; }
        public decimal PrecioXPersona { get; set; }
        public decimal Subtotal { get; set; }
        public EstadoDetalle Estado { get; set; }
        public DateTime? FechaCancelacion { get; set; }
        public bool PuedeCancelar { get; set; }
        public List<PaseDto> Pases { get; set; } = new List<PaseDto>();
    }

    public class PaseDto
    {
        public string Codigo { get; set; }
        public string NombrePasajero { get; set; }
        public string IdentificacionPasajero { get; set; }
        public bool Asistio { get; set; }
        public bool Anulado { get; set; }
    }
}

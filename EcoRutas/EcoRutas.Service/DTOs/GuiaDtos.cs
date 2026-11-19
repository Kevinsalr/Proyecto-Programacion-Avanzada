using System;
using System.Collections.Generic;

namespace EcoRutas.Service.DTOs
{
    // Una salida asignada al guía.
    public class SalidaGuiaDto
    {
        public int Id { get; set; }
        public string TourNombre { get; set; }
        public string PuntoEncuentro { get; set; }
        public DateTime FechaHora { get; set; }
        public int CupoMax { get; set; }
        public int Pasajeros { get; set; }      // pasajeros con reserva confirmada
        public bool Activa { get; set; }
        public bool ListaCerrada { get; set; }
    }

    // Lista de pasajeros de una salida (solo consulta).
    public class ListaPasajerosDto
    {
        public int SalidaId { get; set; }
        public string TourNombre { get; set; }
        public string PuntoEncuentro { get; set; }
        public DateTime FechaHora { get; set; }
        public int CupoMax { get; set; }
        public bool ListaCerrada { get; set; }
        public List<PasajeroListaDto> Pasajeros { get; set; } = new List<PasajeroListaDto>();
    }

    public class PasajeroListaDto
    {
        public string CodigoPase { get; set; }
        public string Nombre { get; set; }
        public string Identificacion { get; set; }
        public string ReservadoPor { get; set; }
        public bool Asistio { get; set; }
    }
}

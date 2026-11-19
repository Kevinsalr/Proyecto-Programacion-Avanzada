using System;
using System.Collections.Generic;

namespace EcoRutas.Service.DTOs
{
    // Tarjeta de un tour en el catálogo público.
    public class TourCatalogoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string Dificultad { get; set; }
        public int DuracionMinutos { get; set; }
        public int? ImagenPrincipalId { get; set; }
        public decimal? PrecioDesde { get; set; }        // precio más bajo de sus próximas salidas
        public DateTime? ProximaSalida { get; set; }
        public int SalidasDisponibles { get; set; }     // próximas salidas con cupo
    }

    // Detalle público de un tour con sus próximas salidas.
    public class TourCatalogoDetalleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public string Dificultad { get; set; }
        public int DuracionMinutos { get; set; }
        public string PuntoEncuentro { get; set; }
        public List<int> ImagenIds { get; set; } = new List<int>();
        public List<SalidaCatalogoDto> Salidas { get; set; } = new List<SalidaCatalogoDto>();
    }

    public class SalidaCatalogoDto
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string GuiaNombre { get; set; }
        public decimal PrecioXPersona { get; set; }
        public int CuposDisponibles { get; set; }
    }

    // Datos actuales de una salida para mostrarla en el carrito.
    public class SalidaCarritoDto
    {
        public int SalidaId { get; set; }
        public int TourId { get; set; }
        public string TourNombre { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal PrecioXPersona { get; set; }
        public int CuposDisponibles { get; set; }
        public bool Disponible { get; set; }   // activa, tour activo, futura y con la lista abierta
    }
}

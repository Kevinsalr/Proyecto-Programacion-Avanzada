using System;
using System.Collections.Generic;

namespace EcoRutas.Service.DTOs
{
    public class TourListadoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string Dificultad { get; set; }
        public bool Activo { get; set; }
        public int CantidadImagenes { get; set; }
        public int CantidadSalidas { get; set; }
    }

    public class TourGuardarDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public string Dificultad { get; set; }
        public int DuracionMinutos { get; set; }
        public string PuntoEncuentro { get; set; }
        public bool Activo { get; set; }
    }

    public class TourDetalleDto : TourGuardarDto
    {
        public List<ImagenDto> Imagenes { get; set; } = new List<ImagenDto>();
        public List<SalidaDto> Salidas { get; set; } = new List<SalidaDto>();
    }

    public class ImagenDto
    {
        public int Id { get; set; }
        public string NombreArchivo { get; set; }
        public int Orden { get; set; }
    }

    public class ImagenNuevaDto
    {
        public string NombreArchivo { get; set; }
        public string TipoContenido { get; set; }
        public byte[] Contenido { get; set; }
    }

    public class ImagenContenidoDto
    {
        public string TipoContenido { get; set; }
        public byte[] Contenido { get; set; }
    }

    public class SalidaDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public DateTime FechaHora { get; set; }
        public string GuiaId { get; set; }
        public string GuiaNombre { get; set; }
        public decimal PrecioXPersona { get; set; }
        public int CupoMax { get; set; }
        public int CuposDisponibles { get; set; }
        public bool Activa { get; set; }
        public bool ListaCerrada { get; set; }
        public int ReservasConfirmadas { get; set; }
    }

    public class SalidaGuardarDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public DateTime FechaHora { get; set; }
        public string GuiaId { get; set; }
        public decimal PrecioXPersona { get; set; }
        public int CupoMax { get; set; }
        public bool Activa { get; set; }

        // Versión de fila leída al abrir el formulario de edición (control de concurrencia).
        public byte[] VersionFila { get; set; }
    }

    public class GuiaDto
    {
        public string Id { get; set; }
        public string NombreCompleto { get; set; }
    }
}
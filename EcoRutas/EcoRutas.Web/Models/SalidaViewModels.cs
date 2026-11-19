using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EcoRutas.Web.Models
{
    public class SalidaFormViewModel
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string TourNombre { get; set; }

        [Required(ErrorMessage = "La fecha y hora son obligatorias.")]
        [Display(Name = "Fecha y hora")]
        public DateTime FechaHora { get; set; }

        [Required(ErrorMessage = "Seleccione un guía.")]
        [Display(Name = "Guía asignado")]
        public string GuiaId { get; set; }

        [Range(0.01, 9999999, ErrorMessage = "El precio debe ser mayor a cero.")]
        [Display(Name = "Precio por persona")]
        public decimal PrecioXPersona { get; set; }

        [Range(1, 500, ErrorMessage = "El cupo máximo debe estar entre 1 y 500.")]
        [Display(Name = "Cupo máximo")]
        public int CupoMax { get; set; }

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        // Versión de fila (en Base64) para detectar ediciones concurrentes.
        public string VersionFilaBase64 { get; set; }

        public IEnumerable<SelectListItem> Guias { get; set; } = new List<SelectListItem>();
    }
}
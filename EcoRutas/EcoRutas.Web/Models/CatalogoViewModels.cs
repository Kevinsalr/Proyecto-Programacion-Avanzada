using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EcoRutas.Service.DTOs;

namespace EcoRutas.Web.Models
{
    // Catálogo público con filtros.
    public class CatalogoIndexViewModel
    {
        [Display(Name = "Buscar")]
        public string Busqueda { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        [Display(Name = "Dificultad")]
        public string Dificultad { get; set; }

        public IList<TourCatalogoDto> Tours { get; set; } = new List<TourCatalogoDto>();
    }
}

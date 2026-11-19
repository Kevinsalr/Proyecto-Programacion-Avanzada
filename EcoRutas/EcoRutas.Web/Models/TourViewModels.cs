using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EcoRutas.Service.DTOs;

namespace EcoRutas.Web.Models
{
    // Formulario de crear / editar tour.
    public class TourFormViewModel
    {
        public static readonly string[] Categorias =
        {
            "Caminata", "Canopy", "Avistamiento de aves", "Rafting", "Otro"
        };

        public static readonly string[] Dificultades =
        {
            "Fácil", "Moderada", "Difícil"
        };

        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(20, ErrorMessage = "El código no puede superar 20 caracteres.")]
        [Display(Name = "Código")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Seleccione una categoría.")]
        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        [Required(ErrorMessage = "Seleccione la dificultad.")]
        [Display(Name = "Dificultad")]
        public string Dificultad { get; set; }

        [Range(1, 10080, ErrorMessage = "La duración debe estar entre 1 y 10080 minutos.")]
        [Display(Name = "Duración (minutos)")]
        public int DuracionMinutos { get; set; }

        [Required(ErrorMessage = "El punto de encuentro es obligatorio.")]
        [StringLength(150, ErrorMessage = "El punto de encuentro no puede superar 150 caracteres.")]
        [Display(Name = "Punto de encuentro")]
        public string PuntoEncuentro { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }

    // Pantalla de lista con filtros.
    public class TourIndexViewModel
    {
        [Display(Name = "Buscar")]
        public string Busqueda { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; }   // "", "activos" o "inactivos"

        public IList<TourListadoDto> Tours { get; set; } = new List<TourListadoDto>();
    }
}
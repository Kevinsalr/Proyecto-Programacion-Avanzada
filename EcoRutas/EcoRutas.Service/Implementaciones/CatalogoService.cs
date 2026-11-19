using System;
using System.Collections.Generic;
using System.Linq;
using EcoRutas.Repository.Interfaces;
using EcoRutas.Service.DTOs;
using EcoRutas.Service.Exceptions;
using EcoRutas.Service.Interfaces;

namespace EcoRutas.Service.Implementaciones
{
    // Consultas públicas: solo muestra tours activos y salidas que todavía se pueden reservar.
    public class CatalogoService : ICatalogoService
    {
        private readonly IUnitOfWork _uow;

        public CatalogoService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public IList<TourCatalogoDto> ListarTours(string busqueda, string categoria, string dificultad)
        {
            var ahora = DateTime.Now;
            var consulta = _uow.Tours.Consultar().Where(t => t.Activo);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                consulta = consulta.Where(t => t.Nombre.Contains(texto) || t.Descripcion.Contains(texto));
            }
            if (!string.IsNullOrWhiteSpace(categoria))
                consulta = consulta.Where(t => t.Categoria == categoria);
            if (!string.IsNullOrWhiteSpace(dificultad))
                consulta = consulta.Where(t => t.Dificultad == dificultad);

            // Proyección: no se cargan los bytes de las imágenes, solo el Id de la primera.
            return consulta.OrderBy(t => t.Nombre)
                .Select(t => new TourCatalogoDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    Categoria = t.Categoria,
                    Dificultad = t.Dificultad,
                    DuracionMinutos = t.DuracionMinutos,
                    ImagenPrincipalId = t.Imagenes.OrderBy(i => i.Orden).Select(i => (int?)i.Id).FirstOrDefault(),
                    PrecioDesde = t.Salidas
                        .Where(s => s.Activa && !s.ListaCerrada && s.FechaHora > ahora)
                        .Min(s => (decimal?)s.PrecioXPersona),
                    ProximaSalida = t.Salidas
                        .Where(s => s.Activa && !s.ListaCerrada && s.FechaHora > ahora)
                        .Min(s => (DateTime?)s.FechaHora),
                    SalidasDisponibles = t.Salidas
                        .Count(s => s.Activa && !s.ListaCerrada && s.FechaHora > ahora && s.CuposDisponibles > 0)
                }).ToList();
        }

        public TourCatalogoDetalleDto ObtenerTour(int id)
        {
            var ahora = DateTime.Now;
            var tour = _uow.Tours.Consultar()
                .Where(t => t.Id == id && t.Activo)
                .Select(t => new TourCatalogoDetalleDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    Categoria = t.Categoria,
                    Dificultad = t.Dificultad,
                    DuracionMinutos = t.DuracionMinutos,
                    PuntoEncuentro = t.PuntoEncuentro
                }).FirstOrDefault();

            if (tour == null)
                throw new NoEncontradoException("El tour solicitado no está disponible.");

            tour.ImagenIds = _uow.Imagenes.Consultar()
                .Where(i => i.TourId == id)
                .OrderBy(i => i.Orden)
                .Select(i => i.Id)
                .ToList();

            tour.Salidas = _uow.Salidas.Consultar()
                .Where(s => s.TourId == id && s.Activa && !s.ListaCerrada && s.FechaHora > ahora)
                .OrderBy(s => s.FechaHora)
                .Select(s => new SalidaCatalogoDto
                {
                    Id = s.SalidaId,
                    FechaHora = s.FechaHora,
                    GuiaNombre = s.Guia.NombreCompleto,
                    PrecioXPersona = s.PrecioXPersona,
                    CuposDisponibles = s.CuposDisponibles
                }).ToList();

            return tour;
        }

        public IList<SalidaCarritoDto> ObtenerSalidasParaCarrito(IEnumerable<int> salidaIds)
        {
            var ids = (salidaIds ?? Enumerable.Empty<int>()).Distinct().ToList();
            if (ids.Count == 0) return new List<SalidaCarritoDto>();

            var ahora = DateTime.Now;
            return _uow.Salidas.Consultar()
                .Where(s => ids.Contains(s.SalidaId))
                .Select(s => new SalidaCarritoDto
                {
                    SalidaId = s.SalidaId,
                    TourId = s.TourId,
                    TourNombre = s.Tour.Nombre,
                    FechaHora = s.FechaHora,
                    PrecioXPersona = s.PrecioXPersona,
                    CuposDisponibles = s.CuposDisponibles,
                    Disponible = s.Activa && s.Tour.Activo && !s.ListaCerrada && s.FechaHora > ahora
                }).ToList();
        }
    }
}

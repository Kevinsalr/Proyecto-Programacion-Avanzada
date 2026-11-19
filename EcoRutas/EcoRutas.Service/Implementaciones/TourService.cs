using System;
using System.Collections.Generic;
using System.Linq;
using EcoRutas.Data.Entities;
using EcoRutas.Repository.Interfaces;
using EcoRutas.Service.DTOs;
using EcoRutas.Service.Exceptions;
using EcoRutas.Service.Interfaces;

namespace EcoRutas.Service.Implementaciones
{
    public class TourService : ITourService
    {
        private const int MinImagenes = 3;
        private const int MaxBytesImagen = 2 * 1024 * 1024;
        private static readonly string[] TiposPermitidos = { "image/jpeg", "image/png", "image/gif", "image/webp" };

        private readonly IUnitOfWork _uow;

        public TourService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ---------- Consultas ----------

        public IList<TourListadoDto> Listar(string busqueda, bool? activo)
        {
            var consulta = _uow.Tours.Consultar();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                consulta = consulta.Where(t => t.Nombre.Contains(texto)
                    || t.Codigo.Contains(texto) || t.Categoria.Contains(texto));
            }

            if (activo.HasValue)
            {
                var valor = activo.Value;
                consulta = consulta.Where(t => t.Activo == valor);
            }

            // Proyección: no se cargan los bytes de las imágenes.
            return consulta.OrderBy(t => t.Nombre)
                .Select(t => new TourListadoDto
                {
                    Id = t.Id,
                    Codigo = t.Codigo,
                    Nombre = t.Nombre,
                    Categoria = t.Categoria,
                    Dificultad = t.Dificultad,
                    Activo = t.Activo,
                    CantidadImagenes = t.Imagenes.Count(),
                    CantidadSalidas = t.Salidas.Count()
                }).ToList();
        }

        public TourDetalleDto ObtenerDetalle(int id)
        {
            var tour = _uow.Tours.Consultar()
                .Where(t => t.Id == id)
                .Select(t => new TourDetalleDto
                {
                    Id = t.Id,
                    Codigo = t.Codigo,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    Categoria = t.Categoria,
                    Dificultad = t.Dificultad,
                    DuracionMinutos = t.DuracionMinutos,
                    PuntoEncuentro = t.PuntoEncuentro,
                    Activo = t.Activo
                }).FirstOrDefault();

            if (tour == null)
                throw new NoEncontradoException("El tour solicitado no existe.");

            tour.Imagenes = _uow.Imagenes.Consultar()
                .Where(i => i.TourId == id)
                .OrderBy(i => i.Orden)
                .Select(i => new ImagenDto { Id = i.Id, NombreArchivo = i.NombreArchivo, Orden = i.Orden })
                .ToList();

            tour.Salidas = _uow.Salidas.Consultar()
                .Where(s => s.TourId == id)
                .OrderBy(s => s.FechaHora)
                .Select(s => new SalidaDto
                {
                    Id = s.SalidaId,
                    TourId = s.TourId,
                    FechaHora = s.FechaHora,
                    GuiaId = s.GuiaId,
                    GuiaNombre = s.Guia.NombreCompleto,
                    PrecioXPersona = s.PrecioXPersona,
                    CupoMax = s.CupoMax,
                    CuposDisponibles = s.CuposDisponibles,
                    Activa = s.Activa,
                    ListaCerrada = s.ListaCerrada,
                    ReservasConfirmadas = s.DetallesReserva.Count(d => d.Estado == EstadoDetalle.Confirmada)
                }).ToList();

            return tour;
        }

        public TourGuardarDto ObtenerParaEditar(int id)
        {
            var tour = ObtenerTour(id);
            return new TourGuardarDto
            {
                Id = tour.Id,
                Codigo = tour.Codigo,
                Nombre = tour.Nombre,
                Descripcion = tour.Descripcion,
                Categoria = tour.Categoria,
                Dificultad = tour.Dificultad,
                DuracionMinutos = tour.DuracionMinutos,
                PuntoEncuentro = tour.PuntoEncuentro,
                Activo = tour.Activo
            };
        }

        // ---------- Comandos ----------

        public int Crear(TourGuardarDto dto, IList<ImagenNuevaDto> imagenes)
        {
            ValidarCampos(dto);

            if (_uow.Tours.ExisteCodigo(dto.Codigo.Trim(), null))
                throw new ConflictoException("Ya existe un tour con ese código.");

            var lista = imagenes ?? new List<ImagenNuevaDto>();
            if (lista.Count < MinImagenes)
                throw new ValidacionException("Cada tour debe tener al menos " + MinImagenes + " imágenes.");
            foreach (var imagen in lista) ValidarImagen(imagen);

            var tour = new Tour { Activo = dto.Activo };
            AplicarCampos(tour, dto);

            var orden = 1;
            foreach (var imagen in lista)
            {
                tour.Imagenes.Add(new Imagen
                {
                    NombreArchivo = imagen.NombreArchivo,
                    TipoContenido = imagen.TipoContenido,
                    Contenido = imagen.Contenido,
                    Orden = orden++
                });
            }

            _uow.Tours.Agregar(tour);
            _uow.Guardar();
            return tour.Id;
        }

        public void Editar(TourGuardarDto dto)
        {
            var tour = ObtenerTour(dto.Id);
            ValidarCampos(dto);

            if (_uow.Tours.ExisteCodigo(dto.Codigo.Trim(), tour.Id))
                throw new ConflictoException("Ya existe otro tour con ese código.");

            if (dto.Activo) ExigirImagenesMinimas(tour.Id);

            AplicarCampos(tour, dto);
            tour.Activo = dto.Activo;
            _uow.Guardar();
        }

        public void CambiarEstado(int id, bool activo)
        {
            var tour = ObtenerTour(id);
            if (activo) ExigirImagenesMinimas(id);

            tour.Activo = activo;
            _uow.Guardar();
        }

        public void Eliminar(int id)
        {
            var tour = ObtenerTour(id);

            if (_uow.Tours.TieneDependencias(id))
                throw new ConflictoException(
                    "El tour tiene salidas o reseñas asociadas y no puede eliminarse. Puede inactivarlo.");

            _uow.Tours.Eliminar(tour);   // las imágenes se eliminan en cascada
            _uow.Guardar();
        }

        // ---------- Imágenes ----------

        public void AgregarImagenes(int tourId, IList<ImagenNuevaDto> imagenes)
        {
            ObtenerTour(tourId);

            if (imagenes == null || imagenes.Count == 0)
                throw new ValidacionException("Seleccione al menos una imagen.");
            foreach (var imagen in imagenes) ValidarImagen(imagen);

            var orden = _uow.Imagenes.SiguienteOrden(tourId);
            foreach (var imagen in imagenes)
            {
                _uow.Imagenes.Agregar(new Imagen
                {
                    TourId = tourId,
                    NombreArchivo = imagen.NombreArchivo,
                    TipoContenido = imagen.TipoContenido,
                    Contenido = imagen.Contenido,
                    Orden = orden++
                });
            }
            _uow.Guardar();
        }

        public int EliminarImagen(int imagenId)
        {
            var imagen = _uow.Imagenes.ObtenerPorId(imagenId);
            if (imagen == null)
                throw new NoEncontradoException("La imagen no existe.");

            if (_uow.Imagenes.ContarPorTour(imagen.TourId) <= MinImagenes)
                throw new ValidacionException("El tour debe conservar al menos " + MinImagenes + " imágenes.");

            var tourId = imagen.TourId;
            _uow.Imagenes.Eliminar(imagen);
            _uow.Guardar();
            return tourId;
        }

        public ImagenContenidoDto ObtenerImagen(int imagenId)
        {
            var imagen = _uow.Imagenes.ObtenerPorId(imagenId);
            if (imagen == null)
                throw new NoEncontradoException("La imagen no existe.");

            return new ImagenContenidoDto { TipoContenido = imagen.TipoContenido, Contenido = imagen.Contenido };
        }

        // ---------- Auxiliares ----------

        private Tour ObtenerTour(int id)
        {
            var tour = _uow.Tours.ObtenerPorId(id);
            if (tour == null)
                throw new NoEncontradoException("El tour solicitado no existe.");
            return tour;
        }

        private void ExigirImagenesMinimas(int tourId)
        {
            if (_uow.Imagenes.ContarPorTour(tourId) < MinImagenes)
                throw new ValidacionException(
                    "Para activar el tour debe tener al menos " + MinImagenes + " imágenes.");
        }

        private static void AplicarCampos(Tour tour, TourGuardarDto dto)
        {
            tour.Codigo = dto.Codigo.Trim();
            tour.Nombre = dto.Nombre.Trim();
            tour.Descripcion = dto.Descripcion.Trim();
            tour.Categoria = dto.Categoria.Trim();
            tour.Dificultad = dto.Dificultad.Trim();
            tour.DuracionMinutos = dto.DuracionMinutos;
            tour.PuntoEncuentro = dto.PuntoEncuentro.Trim();
        }

        private static void ValidarCampos(TourGuardarDto dto)
        {
            if (dto == null) throw new ValidacionException("Datos del tour incompletos.");

            Requerido(dto.Codigo, "El código");
            Requerido(dto.Nombre, "El nombre");
            Requerido(dto.Descripcion, "La descripción");
            Requerido(dto.Categoria, "La categoría");
            Requerido(dto.Dificultad, "La dificultad");
            Requerido(dto.PuntoEncuentro, "El punto de encuentro");

            if (dto.Codigo.Trim().Length > 20)
                throw new ValidacionException("El código no puede superar 20 caracteres.");
            if (dto.DuracionMinutos <= 0)
                throw new ValidacionException("La duración debe ser mayor a cero.");
        }

        private static void Requerido(string valor, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ValidacionException(campo + " es obligatorio.");
        }

        private static void ValidarImagen(ImagenNuevaDto imagen)
        {
            if (imagen == null || imagen.Contenido == null || imagen.Contenido.Length == 0)
                throw new ValidacionException("Hay una imagen vacía.");
            if (!TiposPermitidos.Contains(imagen.TipoContenido))
                throw new ValidacionException("Solo se permiten imágenes JPG, PNG, GIF o WEBP.");
            if (imagen.Contenido.Length > MaxBytesImagen)
                throw new ValidacionException("Cada imagen puede pesar como máximo 2 MB.");
        }
    }
}
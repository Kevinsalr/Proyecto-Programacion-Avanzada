using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EcoRutas.Data.Entities;
using EcoRutas.Repository.Interfaces;
using EcoRutas.Service.DTOs;
using EcoRutas.Service.Exceptions;
using EcoRutas.Service.Interfaces;

namespace EcoRutas.Service.Implementaciones
{
    public class SalidaService : ISalidaService
    {
        private const string RolGuia = "Guia";

        private readonly IUnitOfWork _uow;

        public SalidaService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public SalidaGuardarDto ObtenerParaEditar(int salidaId)
        {
            var salida = ObtenerSalida(salidaId);
            return new SalidaGuardarDto
            {
                Id = salida.SalidaId,
                TourId = salida.TourId,
                FechaHora = salida.FechaHora,
                GuiaId = salida.GuiaId,
                PrecioXPersona = salida.PrecioXPersona,
                CupoMax = salida.CupoMax,
                Activa = salida.Activa,
                VersionFila = salida.VersionFila
            };
        }

        public int Crear(SalidaGuardarDto dto)
        {
            if (_uow.Tours.ObtenerPorId(dto.TourId) == null)
                throw new NoEncontradoException("El tour solicitado no existe.");

            ValidarDatos(dto);

            if (dto.FechaHora <= DateTime.Now)
                throw new ValidacionException("La fecha y hora de la salida deben ser futuras.");

            ValidarGuia(dto.GuiaId);

            var salida = new Salida
            {
                TourId = dto.TourId,
                FechaHora = dto.FechaHora,
                GuiaId = dto.GuiaId,
                PrecioXPersona = dto.PrecioXPersona,
                CupoMax = dto.CupoMax,
                CuposDisponibles = dto.CupoMax,
                Activa = dto.Activa,
                ListaCerrada = false
            };

            _uow.Salidas.Agregar(salida);
            Guardar();
            return salida.SalidaId;
        }

        public void Editar(SalidaGuardarDto dto)
        {
            var salida = ObtenerSalida(dto.Id);

            if (salida.ListaCerrada)
                throw new ConflictoException("La lista de esta salida ya fue cerrada y no puede modificarse.");

            ValidarDatos(dto);

            // Si otra operación (p. ej. una reserva) cambió la salida desde que se abrió el formulario,
            // el UPDATE no afectará filas y EF lanzará DbUpdateConcurrencyException.
            if (dto.VersionFila != null)
                _uow.Salidas.EstablecerVersionOriginal(salida, dto.VersionFila);

            // Regla: el cupo máximo no puede bajar de los cupos ya reservados.
            var reservados = salida.CupoMax - salida.CuposDisponibles;
            if (dto.CupoMax < reservados)
                throw new ValidacionException(
                    "El cupo máximo no puede ser menor a los cupos ya reservados (" + reservados + ").");

            // Regla: una salida con reservas confirmadas no puede inactivarse.
            if (salida.Activa && !dto.Activa && _uow.Salidas.ContarDetallesConfirmados(salida.SalidaId) > 0)
                throw new ConflictoException("La salida tiene reservas confirmadas y no puede inactivarse.");

            if (dto.FechaHora != salida.FechaHora && dto.FechaHora <= DateTime.Now)
                throw new ValidacionException("La nueva fecha y hora deben ser futuras.");

            if (dto.GuiaId != salida.GuiaId)
                ValidarGuia(dto.GuiaId);

            salida.CuposDisponibles += dto.CupoMax - salida.CupoMax;
            salida.CupoMax = dto.CupoMax;
            salida.FechaHora = dto.FechaHora;
            salida.GuiaId = dto.GuiaId;
            salida.PrecioXPersona = dto.PrecioXPersona;
            salida.Activa = dto.Activa;

            Guardar();
        }

        public int Eliminar(int salidaId)
        {
            var salida = ObtenerSalida(salidaId);

            if (_uow.Salidas.ContarDetallesConfirmados(salidaId) > 0)
                throw new ConflictoException("La salida tiene reservas confirmadas y no puede eliminarse.");

            if (_uow.Salidas.TieneDetalles(salidaId))
                throw new ConflictoException(
                    "La salida tiene historial de reservas y no puede eliminarse. Puede inactivarla.");

            var tourId = salida.TourId;
            _uow.Salidas.Eliminar(salida);
            Guardar();
            return tourId;
        }

        public IList<GuiaDto> ListarGuias()
        {
            return _uow.Usuarios.ConsultarPorRol(RolGuia)
                .Where(u => u.Activo)
                .OrderBy(u => u.NombreCompleto)
                .Select(u => new GuiaDto { Id = u.Id, NombreCompleto = u.NombreCompleto })
                .ToList();
        }

        // ---------- Auxiliares ----------

        private Salida ObtenerSalida(int id)
        {
            var salida = _uow.Salidas.ObtenerPorId(id);
            if (salida == null)
                throw new NoEncontradoException("La salida solicitada no existe.");
            return salida;
        }

        private void ValidarGuia(string guiaId)
        {
            if (string.IsNullOrWhiteSpace(guiaId))
                throw new ValidacionException("Debe asignar un guía.");

            var esGuiaActivo = _uow.Usuarios.ConsultarPorRol(RolGuia)
                .Any(u => u.Id == guiaId && u.Activo);

            if (!esGuiaActivo)
                throw new ValidacionException("El usuario seleccionado no es un guía activo.");
        }

        private static void ValidarDatos(SalidaGuardarDto dto)
        {
            if (dto == null) throw new ValidacionException("Datos de la salida incompletos.");
            if (dto.PrecioXPersona <= 0)
                throw new ValidacionException("El precio por persona debe ser mayor a cero.");
            if (dto.CupoMax < 1 || dto.CupoMax > 500)
                throw new ValidacionException("El cupo máximo debe estar entre 1 y 500.");
        }

        private void Guardar()
        {
            try
            {
                _uow.Guardar();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictoException(
                    "La salida fue modificada por otra operación (por ejemplo, una reserva). " +
                    "Recargue la página e intente de nuevo.");
            }
        }
    }
}
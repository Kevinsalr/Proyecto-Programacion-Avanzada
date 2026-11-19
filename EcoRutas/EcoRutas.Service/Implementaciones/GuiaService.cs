using System.Collections.Generic;
using System.Linq;
using EcoRutas.Data.Entities;
using EcoRutas.Repository.Interfaces;
using EcoRutas.Service.DTOs;
using EcoRutas.Service.Exceptions;
using EcoRutas.Service.Interfaces;

namespace EcoRutas.Service.Implementaciones
{
    // Consultas del guía: sus salidas y la lista de pasajeros. No modifica nada.
    public class GuiaService : IGuiaService
    {
        private readonly IUnitOfWork _uow;

        public GuiaService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public IList<SalidaGuiaDto> ListarSalidas(string guiaId)
        {
            return _uow.Salidas.Consultar()
                .Where(s => s.GuiaId == guiaId)
                .OrderBy(s => s.FechaHora)
                .Select(s => new SalidaGuiaDto
                {
                    Id = s.SalidaId,
                    TourNombre = s.Tour.Nombre,
                    PuntoEncuentro = s.Tour.PuntoEncuentro,
                    FechaHora = s.FechaHora,
                    CupoMax = s.CupoMax,
                    Activa = s.Activa,
                    ListaCerrada = s.ListaCerrada,
                    Pasajeros = s.DetallesReserva
                        .Where(d => d.Estado != EstadoDetalle.Cancelada)
                        .Sum(d => (int?)d.CantidadPasajeros) ?? 0
                }).ToList();
        }

        public ListaPasajerosDto ObtenerPasajeros(int salidaId, string guiaId)
        {
            var salida = _uow.Salidas.ObtenerPorId(salidaId);
            if (salida == null)
                throw new NoEncontradoException("La salida solicitada no existe.");

            // Regla: el guía solo puede ver las salidas que tiene asignadas.
            if (salida.GuiaId != guiaId)
                throw new AccesoDenegadoException("Esta salida no está asignada a usted.");

            var tour = _uow.Tours.ObtenerPorId(salida.TourId);

            var pasajeros = _uow.Pases.Consultar()
                .Where(p => p.DetalleReserva.SalidaId == salidaId && !p.Anulado
                    && p.DetalleReserva.Estado != EstadoDetalle.Cancelada)
                .OrderBy(p => p.NombrePasajero)
                .Select(p => new PasajeroListaDto
                {
                    CodigoPase = p.Codigo,
                    Nombre = p.NombrePasajero,
                    Identificacion = p.IdentificacionPasajero,
                    ReservadoPor = p.DetalleReserva.Reserva.Asociado.NombreCompleto,
                    Asistio = p.Asistio
                }).ToList();

            return new ListaPasajerosDto
            {
                SalidaId = salida.SalidaId,
                TourNombre = tour.Nombre,
                PuntoEncuentro = tour.PuntoEncuentro,
                FechaHora = salida.FechaHora,
                CupoMax = salida.CupoMax,
                ListaCerrada = salida.ListaCerrada,
                Pasajeros = pasajeros
            };
        }
    }
}

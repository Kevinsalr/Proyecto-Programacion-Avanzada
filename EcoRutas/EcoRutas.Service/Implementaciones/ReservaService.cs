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
    public class ReservaService : IReservaService
    {
        private const string RolAsociado = "Asociado";
        private const int MinPasajeros = 1;
        private const int MaxPasajeros = 6;
        private const int HorasMinimasCancelacion = 48;
        private const int MaxIntentos = 3;

        private readonly IUnitOfWork _uow;

        public ReservaService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ---------- Consultas ----------

        public IList<ReservaDto> ListarPorAsociado(string asociadoId)
        {
            var ahora = DateTime.Now;
            return _uow.Reservas.ConsultarConDetalles()
                .Where(r => r.AsociadoId == asociadoId)
                .OrderByDescending(r => r.FechaCreacion)
                .ToList()
                .Select(r => ADto(r, ahora))
                .ToList();
        }

        public ReservaDto ObtenerDeAsociado(int reservaId, string asociadoId)
        {
            var reserva = _uow.Reservas.ConsultarConDetalles()
                .FirstOrDefault(r => r.ReservaId == reservaId);

            if (reserva == null)
                throw new NoEncontradoException("La reserva solicitada no existe.");
            if (reserva.AsociadoId != asociadoId)
                throw new AccesoDenegadoException("No puede consultar reservas de otro asociado.");

            return ADto(reserva, DateTime.Now);
        }

        // ---------- Comandos ----------

        // Todo o nada: si alguna salida no tiene cupo o no es válida, no se guarda ninguna línea.
        public int Reservar(string asociadoId, IList<ReservaLineaDto> lineas)
        {
            ValidarAsociado(asociadoId);
            ValidarLineas(lineas);

            for (var intento = 1; ; intento++)
            {
                try
                {
                    var reserva = ArmarReserva(asociadoId, lineas);
                    _uow.Reservas.Agregar(reserva);
                    _uow.Guardar();   // un solo SaveChanges = una sola transacción
                    return reserva.ReservaId;
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Otra operación cambió alguna salida (su VersionFila ya no coincide).
                    // Se descartan los cambios, se recargan los cupos y se vuelve a intentar.
                    _uow.DescartarCambios();
                    if (intento >= MaxIntentos)
                        throw new ConflictoException(
                            "Las salidas seleccionadas están recibiendo muchas reservas en este momento. Intente de nuevo.");
                }
                catch (NegocioException)
                {
                    _uow.DescartarCambios();   // no deja cupos descontados en memoria
                    throw;
                }
            }
        }

        // Cancela una línea de la reserva (una salida) y devuelve sus cupos.
        public void CancelarDetalle(int detalleId, string asociadoId)
        {
            for (var intento = 1; ; intento++)
            {
                try
                {
                    var detalle = _uow.DetallesReserva.ObtenerPorId(detalleId);
                    if (detalle == null)
                        throw new NoEncontradoException("La reserva solicitada no existe.");

                    var reserva = _uow.Reservas.ObtenerPorId(detalle.ReservaId);
                    if (reserva.AsociadoId != asociadoId)
                        throw new AccesoDenegadoException("No puede cancelar reservas de otro asociado.");

                    if (detalle.Estado != EstadoDetalle.Confirmada)
                        throw new ConflictoException("Esta reserva ya no está confirmada y no puede cancelarse.");

                    var salida = _uow.Salidas.ObtenerPorId(detalle.SalidaId);
                    var ahora = DateTime.Now;

                    if (salida.ListaCerrada)
                        throw new ValidacionException("La lista de pasajeros de esta salida ya fue cerrada.");

                    // Regla: solo se puede cancelar con al menos 48 horas de anticipación.
                    if (!CumpleAnticipacion(salida.FechaHora, ahora))
                        throw new ValidacionException(
                            "Solo se puede cancelar hasta " + HorasMinimasCancelacion + " horas antes de la salida.");

                    detalle.Estado = EstadoDetalle.Cancelada;
                    detalle.FechaCancelacion = ahora;
                    salida.CuposDisponibles += detalle.CantidadPasajeros;

                    foreach (var pase in _uow.Pases.Consultar().Where(p => p.DetalleReservaId == detalle.Id).ToList())
                        pase.Anulado = true;

                    _uow.Guardar();
                    return;
                }
                catch (DbUpdateConcurrencyException)
                {
                    _uow.DescartarCambios();
                    if (intento >= MaxIntentos)
                        throw new ConflictoException("La salida está siendo modificada en este momento. Intente de nuevo.");
                }
                catch (NegocioException)
                {
                    _uow.DescartarCambios();
                    throw;
                }
            }
        }

        // ---------- Auxiliares ----------

        // Arma la reserva en memoria y descuenta los cupos de cada salida.
        private Reserva ArmarReserva(string asociadoId, IList<ReservaLineaDto> lineas)
        {
            var ahora = DateTime.Now;
            var reserva = new Reserva { AsociadoId = asociadoId, FechaCreacion = ahora };

            foreach (var linea in lineas)
            {
                var salida = _uow.Salidas.ObtenerPorId(linea.SalidaId);
                if (salida == null)
                    throw new NoEncontradoException("Una de las salidas seleccionadas ya no existe.");

                var tour = _uow.Tours.ObtenerPorId(salida.TourId);
                ValidarSalidaReservable(salida, tour, ahora);

                var cantidad = linea.Pasajeros.Count;
                if (salida.CuposDisponibles < cantidad)
                    throw new ConflictoException(
                        "La salida de " + tour.Nombre + " del " + salida.FechaHora.ToString("dd/MM/yyyy HH:mm") +
                        " solo tiene " + salida.CuposDisponibles + " cupos disponibles.");

                // Al guardar, EF compara la VersionFila de la salida para detectar reservas simultáneas.
                salida.CuposDisponibles -= cantidad;

                var detalle = new DetalleReserva
                {
                    SalidaId = salida.SalidaId,
                    CantidadPasajeros = cantidad,
                    PrecioXPersona = salida.PrecioXPersona,
                    Estado = EstadoDetalle.Confirmada
                };

                // Un pase por pasajero.
                foreach (var pasajero in linea.Pasajeros)
                {
                    detalle.Pases.Add(new Pase
                    {
                        Codigo = GenerarCodigoPase(),
                        NombrePasajero = pasajero.Nombre.Trim(),
                        IdentificacionPasajero = pasajero.Identificacion.Trim(),
                        Asistio = false,
                        Anulado = false
                    });
                }

                reserva.Detalles.Add(detalle);
            }

            return reserva;
        }

        private string GenerarCodigoPase()
        {
            string codigo;
            do
            {
                codigo = "PASE-" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
            } while (_uow.Pases.ExisteCodigo(codigo));
            return codigo;
        }

        private void ValidarAsociado(string asociadoId)
        {
            var esAsociadoActivo = !string.IsNullOrWhiteSpace(asociadoId)
                && _uow.Usuarios.ConsultarPorRol(RolAsociado).Any(u => u.Id == asociadoId && u.Activo);

            if (!esAsociadoActivo)
                throw new AccesoDenegadoException("Solo un asociado activo puede hacer reservas.");
        }

        private static void ValidarLineas(IList<ReservaLineaDto> lineas)
        {
            if (lineas == null || lineas.Count == 0)
                throw new ValidacionException("El carrito está vacío.");

            if (lineas.Select(l => l.SalidaId).Distinct().Count() != lineas.Count)
                throw new ValidacionException("Una misma salida aparece más de una vez en la reserva.");

            foreach (var linea in lineas)
            {
                // Regla: de 1 a 6 pasajeros por salida.
                if (linea.Pasajeros == null || linea.Pasajeros.Count < MinPasajeros || linea.Pasajeros.Count > MaxPasajeros)
                    throw new ValidacionException(
                        "Cada salida debe tener entre " + MinPasajeros + " y " + MaxPasajeros + " pasajeros.");

                foreach (var pasajero in linea.Pasajeros)
                {
                    if (pasajero == null || string.IsNullOrWhiteSpace(pasajero.Nombre)
                        || string.IsNullOrWhiteSpace(pasajero.Identificacion))
                        throw new ValidacionException("Debe indicar el nombre y la identificación de cada pasajero.");
                    if (pasajero.Nombre.Trim().Length > 120)
                        throw new ValidacionException("El nombre del pasajero no puede superar 120 caracteres.");
                    if (pasajero.Identificacion.Trim().Length > 30)
                        throw new ValidacionException("La identificación del pasajero no puede superar 30 caracteres.");
                }

                var repetidos = linea.Pasajeros
                    .GroupBy(p => p.Identificacion.Trim().ToUpper())
                    .Any(g => g.Count() > 1);
                if (repetidos)
                    throw new ValidacionException("Hay pasajeros con la misma identificación en una misma salida.");
            }
        }

        private static void ValidarSalidaReservable(Salida salida, Tour tour, DateTime ahora)
        {
            if (tour == null || !tour.Activo || !salida.Activa)
                throw new ValidacionException("Una de las salidas seleccionadas ya no está disponible.");
            if (salida.ListaCerrada)
                throw new ValidacionException("La lista de pasajeros de " + tour.Nombre + " ya fue cerrada.");
            if (salida.FechaHora <= ahora)
                throw new ValidacionException("La salida de " + tour.Nombre + " ya no admite reservas porque ya pasó.");
        }

        private static bool CumpleAnticipacion(DateTime fechaSalida, DateTime ahora)
        {
            return fechaSalida - ahora >= TimeSpan.FromHours(HorasMinimasCancelacion);
        }

        private static ReservaDto ADto(Reserva reserva, DateTime ahora)
        {
            var detalles = reserva.Detalles
                .OrderBy(d => d.Salida.FechaHora)
                .Select(d => new DetalleReservaDto
                {
                    Id = d.Id,
                    SalidaId = d.SalidaId,
                    TourNombre = d.Salida.Tour.Nombre,
                    PuntoEncuentro = d.Salida.Tour.PuntoEncuentro,
                    FechaHora = d.Salida.FechaHora,
                    CantidadPasajeros = d.CantidadPasajeros,
                    PrecioXPersona = d.PrecioXPersona,
                    Subtotal = d.Subtotal,
                    Estado = d.Estado,
                    FechaCancelacion = d.FechaCancelacion,
                    PuedeCancelar = d.Estado == EstadoDetalle.Confirmada
                        && !d.Salida.ListaCerrada
                        && CumpleAnticipacion(d.Salida.FechaHora, ahora),
                    Pases = d.Pases
                        .OrderBy(p => p.PaseId)
                        .Select(p => new PaseDto
                        {
                            Codigo = p.Codigo,
                            NombrePasajero = p.NombrePasajero,
                            IdentificacionPasajero = p.IdentificacionPasajero,
                            Asistio = p.Asistio,
                            Anulado = p.Anulado
                        }).ToList()
                }).ToList();

            return new ReservaDto
            {
                Id = reserva.ReservaId,
                FechaCreacion = reserva.FechaCreacion,
                Total = detalles.Where(d => d.Estado != EstadoDetalle.Cancelada).Sum(d => d.Subtotal),
                Detalles = detalles
            };
        }
    }
}

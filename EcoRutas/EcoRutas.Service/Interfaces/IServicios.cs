using System.Collections.Generic;
using EcoRutas.Service.DTOs;

namespace EcoRutas.Service.Interfaces
{
    public interface ITourService
    {
        IList<TourListadoDto> Listar(string busqueda, bool? activo);
        TourDetalleDto ObtenerDetalle(int id);
        TourGuardarDto ObtenerParaEditar(int id);
        int Crear(TourGuardarDto dto, IList<ImagenNuevaDto> imagenes);
        void Editar(TourGuardarDto dto);
        void CambiarEstado(int id, bool activo);
        void Eliminar(int id);

        void AgregarImagenes(int tourId, IList<ImagenNuevaDto> imagenes);
        int EliminarImagen(int imagenId);          // devuelve el TourId
        ImagenContenidoDto ObtenerImagen(int imagenId);
    }

    public interface ISalidaService
    {
        SalidaGuardarDto ObtenerParaEditar(int salidaId);
        int Crear(SalidaGuardarDto dto);
        void Editar(SalidaGuardarDto dto);
        int Eliminar(int salidaId);                // devuelve el TourId
        IList<GuiaDto> ListarGuias();
    }

    public interface IReservaService
    {
        IList<ReservaDto> ListarPorAsociado(string asociadoId);
        ReservaDto ObtenerDeAsociado(int reservaId, string asociadoId);
        int Reservar(string asociadoId, IList<ReservaLineaDto> lineas);   // devuelve el ReservaId
        void CancelarDetalle(int detalleId, string asociadoId);
    }

    public interface IUsuarioService
    {
        IList<UsuarioListadoDto> Listar(string busqueda, string rol, bool? activo);
        UsuarioGuardarDto ObtenerParaEditar(string id);
        string Crear(UsuarioGuardarDto dto, string password);
        void Editar(UsuarioGuardarDto dto, string usuarioActualId);
        void CambiarEstado(string id, bool activo, string usuarioActualId);
        void Eliminar(string id, string usuarioActualId);
    }
}
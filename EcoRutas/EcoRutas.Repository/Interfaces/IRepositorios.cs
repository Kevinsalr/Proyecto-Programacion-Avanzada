using System;
using System.Collections.Generic;
using System.Linq;
using EcoRutas.Data.Entities;

namespace EcoRutas.Repository.Interfaces
{
    public interface IRepositorio<T> where T : class
    {
        T ObtenerPorId(int id);
        IQueryable<T> Consultar();
        void Agregar(T entidad);
        void Eliminar(T entidad);
    }

    public interface ITourRepositorio : IRepositorio<Tour>
    {
        bool ExisteCodigo(string codigo, int? excluirId);
        bool TieneDependencias(int tourId);
    }

    public interface ISalidaRepositorio : IRepositorio<Salida>
    {
        void EstablecerVersionOriginal(Salida salida, byte[] version);
        int ContarDetallesConfirmados(int salidaId);
        bool TieneDetalles(int salidaId);
    }

    public interface IImagenRepositorio : IRepositorio<Imagen>
    {
        int ContarPorTour(int tourId);
        int SiguienteOrden(int tourId);
    }

    public interface IUsuarioRepositorio
    {
        ApplicationUser ObtenerPorId(string id);
        IQueryable<ApplicationUser> Consultar();
        IQueryable<ApplicationUser> ConsultarPorRol(string rol);
        IDictionary<string, string> ObtenerNombresRoles();   // RoleId -> nombre del rol
        bool ExisteCodigo(string codigo);
        bool TieneSalidasFuturas(string guiaId);
        bool TieneDependencias(string usuarioId);
    }

    public interface IUnitOfWork
    {
        ITourRepositorio Tours { get; }
        ISalidaRepositorio Salidas { get; }
        IImagenRepositorio Imagenes { get; }
        IUsuarioRepositorio Usuarios { get; }

        int Guardar();
    }
}
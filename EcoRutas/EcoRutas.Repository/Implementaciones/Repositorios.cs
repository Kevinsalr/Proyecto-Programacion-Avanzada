using System.Data.Entity;
using System.Linq;
using EcoRutas.Data.Context;
using EcoRutas.Data.Entities;
using EcoRutas.Repository.Interfaces;

namespace EcoRutas.Repository.Implementaciones
{
    public class Repositorio<T> : IRepositorio<T> where T : class
    {
        protected readonly EcoRutasContext Contexto;

        public Repositorio(EcoRutasContext contexto)
        {
            Contexto = contexto;
        }

        public virtual T ObtenerPorId(int id)
        {
            return Contexto.Set<T>().Find(id);
        }

        public virtual IQueryable<T> Consultar()
        {
            return Contexto.Set<T>();
        }

        public virtual void Agregar(T entidad)
        {
            Contexto.Set<T>().Add(entidad);
        }

        public virtual void Eliminar(T entidad)
        {
            Contexto.Set<T>().Remove(entidad);
        }
    }

    public class TourRepositorio : Repositorio<Tour>, ITourRepositorio
    {
        public TourRepositorio(EcoRutasContext contexto) : base(contexto) { }

        public bool ExisteCodigo(string codigo, int? excluirId)
        {
            return Contexto.Tours.Any(t => t.Codigo == codigo
                && (!excluirId.HasValue || t.Id != excluirId.Value));
        }

        public bool TieneDependencias(int tourId)
        {
            return Contexto.Salidas.Any(s => s.TourId == tourId)
                || Contexto.Resenas.Any(r => r.TourId == tourId);
        }
    }

    public class SalidaRepositorio : Repositorio<Salida>, ISalidaRepositorio
    {
        public SalidaRepositorio(EcoRutasContext contexto) : base(contexto) { }

        // Usa la versión que el formulario tenía al cargarse, para detectar ediciones concurrentes.
        public void EstablecerVersionOriginal(Salida salida, byte[] version)
        {
            Contexto.Entry(salida).Property(s => s.VersionFila).OriginalValue = version;
        }

        public int ContarDetallesConfirmados(int salidaId)
        {
            return Contexto.DetallesReserva.Count(d => d.SalidaId == salidaId
                && d.Estado == EstadoDetalle.Confirmada);
        }

        public bool TieneDetalles(int salidaId)
        {
            return Contexto.DetallesReserva.Any(d => d.SalidaId == salidaId);
        }
    }

    public class ImagenRepositorio : Repositorio<Imagen>, IImagenRepositorio
    {
        public ImagenRepositorio(EcoRutasContext contexto) : base(contexto) { }

        public int ContarPorTour(int tourId)
        {
            return Contexto.Imagenes.Count(i => i.TourId == tourId);
        }

        public int SiguienteOrden(int tourId)
        {
            var maximo = Contexto.Imagenes
                .Where(i => i.TourId == tourId)
                .Select(i => (int?)i.Orden)
                .Max();
            return (maximo ?? 0) + 1;
        }
    }

    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly EcoRutasContext _contexto;

        public UsuarioRepositorio(EcoRutasContext contexto)
        {
            _contexto = contexto;
        }

        public ApplicationUser ObtenerPorId(string id)
        {
            return _contexto.Users.FirstOrDefault(u => u.Id == id);
        }

        public IQueryable<ApplicationUser> ConsultarPorRol(string rol)
        {
            var rolId = _contexto.Roles
                .Where(r => r.Name == rol)
                .Select(r => r.Id)
                .FirstOrDefault();

            return _contexto.Users.Where(u => u.Roles.Any(r => r.RoleId == rolId));
        }
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly EcoRutasContext _contexto;

        public UnitOfWork(EcoRutasContext contexto)
        {
            _contexto = contexto;
            Tours = new TourRepositorio(contexto);
            Salidas = new SalidaRepositorio(contexto);
            Imagenes = new ImagenRepositorio(contexto);
            Usuarios = new UsuarioRepositorio(contexto);
        }

        public ITourRepositorio Tours { get; }
        public ISalidaRepositorio Salidas { get; }
        public IImagenRepositorio Imagenes { get; }
        public IUsuarioRepositorio Usuarios { get; }

        public int Guardar()
        {
            return _contexto.SaveChanges();
        }
    }
}
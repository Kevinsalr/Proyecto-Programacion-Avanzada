using EcoRutas.Data.Context;
using EcoRutas.Data.Entities;
using EcoRutas.Repository.Implementaciones;
using EcoRutas.Service.Interfaces;
using Microsoft.AspNet.Identity;

namespace EcoRutas.Service.Implementaciones
{
    public class ServiceFactory
    {
        public ServiceFactory(EcoRutasContext contexto, UserManager<ApplicationUser> userManager)
        {
            var uow = new UnitOfWork(contexto);
            Tours = new TourService(uow);
            Salidas = new SalidaService(uow);
            Reservas = new ReservaService(uow);
            Usuarios = new UsuarioService(uow, userManager);
        }

        public ITourService Tours { get; }
        public ISalidaService Salidas { get; }
        public IReservaService Reservas { get; }
        public IUsuarioService Usuarios { get; }
    }
}
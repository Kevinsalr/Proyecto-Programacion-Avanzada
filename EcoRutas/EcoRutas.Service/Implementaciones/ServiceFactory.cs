using EcoRutas.Data.Context;
using EcoRutas.Repository.Implementaciones;
using EcoRutas.Service.Interfaces;

namespace EcoRutas.Service.Implementaciones
{
    public class ServiceFactory
    {
        public ServiceFactory(EcoRutasContext contexto)
        {
            var uow = new UnitOfWork(contexto);
            Tours = new TourService(uow);
            Salidas = new SalidaService(uow);
        }

        public ITourService Tours { get; }
        public ISalidaService Salidas { get; }
    }
}
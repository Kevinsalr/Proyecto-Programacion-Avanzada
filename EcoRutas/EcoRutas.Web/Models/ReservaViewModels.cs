using EcoRutas.Data.Entities;

namespace EcoRutas.Web.Models
{
    // Texto y color con que se muestra el estado de cada línea de una reserva.
    public static class EstadoReserva
    {
        public static string Texto(EstadoDetalle estado)
        {
            switch (estado)
            {
                case EstadoDetalle.Confirmada: return "Confirmada";
                case EstadoDetalle.Cancelada: return "Cancelada";
                case EstadoDetalle.Completada: return "Completada";
                case EstadoDetalle.NoPresentada: return "No presentada";
                default: return estado.ToString();
            }
        }

        public static string Clase(EstadoDetalle estado)
        {
            switch (estado)
            {
                case EstadoDetalle.Confirmada: return "bg-success";
                case EstadoDetalle.Cancelada: return "bg-secondary";
                case EstadoDetalle.Completada: return "bg-primary";
                default: return "bg-warning text-dark";
            }
        }
    }
}

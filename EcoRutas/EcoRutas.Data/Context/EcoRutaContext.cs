using System.Data.Entity;
using EcoRutas.Data.Entities;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EcoRutas.Data.Context
{
    public class EcoRutasContext : IdentityDbContext<ApplicationUser>
    {
        public EcoRutasContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static EcoRutasContext Create()
        {
            return new EcoRutasContext();
        }

        public DbSet<Tour> Tours { get; set; }
        public DbSet<Imagen> Imagenes { get; set; }
        public DbSet<Salida> Salidas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<DetalleReserva> DetallesReserva { get; set; }
        public DbSet<Pase> Pases { get; set; }
        public DbSet<Resena> Resenas { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Salida>()
                .HasRequired(s => s.Guia)
                .WithMany()
                .HasForeignKey(s => s.GuiaId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DetalleReserva>()
                .HasRequired(d => d.Salida)
                .WithMany(s => s.DetallesReserva)
                .HasForeignKey(d => d.SalidaId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Resena>()
                .HasRequired(r => r.Asociado)
                .WithMany()
                .HasForeignKey(r => r.AsociadoId)
                .WillCascadeOnDelete(false);
        }
    }
}
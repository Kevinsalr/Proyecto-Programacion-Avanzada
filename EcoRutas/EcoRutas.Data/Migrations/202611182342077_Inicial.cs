namespace EcoRutas.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Inicial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DetalleReservas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReservaId = c.Int(nullable: false),
                        SalidaId = c.Int(nullable: false),
                        CantidadPasajeros = c.Int(nullable: false),
                        PrecioXPersona = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Estado = c.Int(nullable: false),
                        FechaCancelacion = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Reservas", t => t.ReservaId, cascadeDelete: true)
                .ForeignKey("dbo.Salidas", t => t.SalidaId)
                .Index(t => t.ReservaId)
                .Index(t => t.SalidaId);
            
            CreateTable(
                "dbo.Pases",
                c => new
                    {
                        PaseId = c.Int(nullable: false, identity: true),
                        DetalleReservaId = c.Int(nullable: false),
                        Codigo = c.String(nullable: false, maxLength: 40),
                        NombrePasajero = c.String(nullable: false, maxLength: 120),
                        IdentificacionPasajero = c.String(nullable: false, maxLength: 30),
                        Asistio = c.Boolean(nullable: false),
                        Anulado = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PaseId)
                .ForeignKey("dbo.DetalleReservas", t => t.DetalleReservaId, cascadeDelete: true)
                .Index(t => t.DetalleReservaId)
                .Index(t => t.Codigo, unique: true, name: "IX_Pase_Codigo");
            
            CreateTable(
                "dbo.Reservas",
                c => new
                    {
                        ReservaId = c.Int(nullable: false, identity: true),
                        AsociadoId = c.String(nullable: false, maxLength: 128),
                        FechaCreacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ReservaId)
                .ForeignKey("dbo.AspNetUsers", t => t.AsociadoId, cascadeDelete: true)
                .Index(t => t.AsociadoId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CodigoUsuario = c.String(nullable: false, maxLength: 20),
                        NombreCompleto = c.String(nullable: false, maxLength: 120),
                        Activo = c.Boolean(nullable: false),
                        UltimaConexion = c.DateTime(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Salidas",
                c => new
                    {
                        SalidaId = c.Int(nullable: false, identity: true),
                        TourId = c.Int(nullable: false),
                        FechaHora = c.DateTime(nullable: false),
                        GuiaId = c.String(nullable: false, maxLength: 128),
                        PrecioXPersona = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CupoMax = c.Int(nullable: false),
                        CuposDisponibles = c.Int(nullable: false),
                        Activa = c.Boolean(nullable: false),
                        ListaCerrada = c.Boolean(nullable: false),
                        VersionFila = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.SalidaId)
                .ForeignKey("dbo.AspNetUsers", t => t.GuiaId)
                .ForeignKey("dbo.Tours", t => t.TourId, cascadeDelete: true)
                .Index(t => t.TourId)
                .Index(t => t.GuiaId);
            
            CreateTable(
                "dbo.Tours",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Codigo = c.String(nullable: false, maxLength: 20),
                        Nombre = c.String(nullable: false),
                        Descripcion = c.String(nullable: false),
                        Categoria = c.String(nullable: false),
                        Dificultad = c.String(nullable: false),
                        DuracionMinutos = c.Int(nullable: false),
                        PuntoEncuentro = c.String(nullable: false),
                        Activo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Codigo, unique: true, name: "IX_Tour_Codigo");
            
            CreateTable(
                "dbo.Imagens",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TourId = c.Int(nullable: false),
                        NombreArchivo = c.String(nullable: false),
                        TipoContenido = c.String(nullable: false),
                        Contenido = c.Binary(nullable: false),
                        Orden = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tours", t => t.TourId, cascadeDelete: true)
                .Index(t => t.TourId);
            
            CreateTable(
                "dbo.Resenas",
                c => new
                    {
                        ResenaId = c.Int(nullable: false, identity: true),
                        TourId = c.Int(nullable: false),
                        AsociadoId = c.String(nullable: false, maxLength: 128),
                        Calificacion = c.Int(nullable: false),
                        Comentario = c.String(nullable: false, maxLength: 1000),
                        Estado = c.String(nullable: false, maxLength: 20),
                        FechaCreacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ResenaId)
                .ForeignKey("dbo.AspNetUsers", t => t.AsociadoId)
                .ForeignKey("dbo.Tours", t => t.TourId, cascadeDelete: true)
                .Index(t => t.TourId)
                .Index(t => t.AsociadoId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.DetalleReservas", "SalidaId", "dbo.Salidas");
            DropForeignKey("dbo.Salidas", "TourId", "dbo.Tours");
            DropForeignKey("dbo.Resenas", "TourId", "dbo.Tours");
            DropForeignKey("dbo.Resenas", "AsociadoId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Imagens", "TourId", "dbo.Tours");
            DropForeignKey("dbo.Salidas", "GuiaId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DetalleReservas", "ReservaId", "dbo.Reservas");
            DropForeignKey("dbo.Reservas", "AsociadoId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Pases", "DetalleReservaId", "dbo.DetalleReservas");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Resenas", new[] { "AsociadoId" });
            DropIndex("dbo.Resenas", new[] { "TourId" });
            DropIndex("dbo.Imagens", new[] { "TourId" });
            DropIndex("dbo.Tours", "IX_Tour_Codigo");
            DropIndex("dbo.Salidas", new[] { "GuiaId" });
            DropIndex("dbo.Salidas", new[] { "TourId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Reservas", new[] { "AsociadoId" });
            DropIndex("dbo.Pases", "IX_Pase_Codigo");
            DropIndex("dbo.Pases", new[] { "DetalleReservaId" });
            DropIndex("dbo.DetalleReservas", new[] { "SalidaId" });
            DropIndex("dbo.DetalleReservas", new[] { "ReservaId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Resenas");
            DropTable("dbo.Imagens");
            DropTable("dbo.Tours");
            DropTable("dbo.Salidas");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Reservas");
            DropTable("dbo.Pases");
            DropTable("dbo.DetalleReservas");
        }
    }
}

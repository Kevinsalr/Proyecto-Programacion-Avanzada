using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using EcoRutas.Data.Context;
using EcoRutas.Data.Entities;

namespace EcoRutas.Data.Seed
{
    // Datos iniciales del catálogo: tours con 3 imágenes y salidas pasadas, del día y futuras.
    // Se puede ejecutar varias veces: si un tour (por código) ya existe, no se vuelve a crear.
    public static class CatalogoSeed
    {
        private const string RolGuia = "Guia";
        private const int ImagenesPorTour = 3;

        // Días respecto a la fecha en que se ejecuta el seed: dos pasadas, una del día y dos futuras.
        private static readonly int[] DiasSalidas = { -10, -2, 0, 5, 15 };

        public static void Sembrar(EcoRutasContext context)
        {
            var guias = ObtenerGuias(context);
            if (guias.Count == 0) return;   // sin guías no se pueden crear salidas

            var guia1 = guias[0];
            var guia2 = guias.Count > 1 ? guias[1] : guias[0];

            CrearTour(context, new Tour
            {
                Codigo = "TOUR-001",
                Nombre = "Caminata al Volcán Arenal",
                Descripcion = "Recorrido por senderos entre bosque lluvioso y antiguas coladas de lava, " +
                    "con miradores hacia el volcán y la laguna de Arenal.",
                Categoria = "Caminata",
                Dificultad = "Moderada",
                DuracionMinutos = 180,
                PuntoEncuentro = "Entrada principal del Parque Nacional Volcán Arenal, La Fortuna",
                Activo = true
            }, Color.FromArgb(46, 125, 50), 9, 45m, 12, guia1, guia2);

            CrearTour(context, new Tour
            {
                Codigo = "TOUR-002",
                Nombre = "Canopy en Monteverde",
                Descripcion = "Circuito de cables y puentes colgantes sobre el bosque nuboso, " +
                    "con equipo de seguridad completo y guías certificados.",
                Categoria = "Canopy",
                Dificultad = "Moderada",
                DuracionMinutos = 120,
                PuntoEncuentro = "Recepción de la Reserva Santa Elena, Monteverde",
                Activo = true
            }, Color.FromArgb(0, 105, 92), 15, 60m, 10, guia2, guia1);

            CrearTour(context, new Tour
            {
                Codigo = "TOUR-003",
                Nombre = "Aves del Parque Nacional Carara",
                Descripcion = "Caminata temprano en la mañana con binoculares para observar lapas rojas, " +
                    "tucanes y aves migratorias en la transición entre bosque seco y húmedo.",
                Categoria = "Avistamiento de aves",
                Dificultad = "Fácil",
                DuracionMinutos = 150,
                PuntoEncuentro = "Puesto de ingreso del Parque Nacional Carara, Tárcoles",
                Activo = true
            }, Color.FromArgb(230, 81, 0), 6, 35m, 8, guia1, guia2);

            CrearTour(context, new Tour
            {
                Codigo = "TOUR-004",
                Nombre = "Rafting en el río Pacuare",
                Descripcion = "Descenso por rápidos clase III y IV entre cañones y cataratas, " +
                    "con almuerzo típico en la ribera del río.",
                Categoria = "Rafting",
                Dificultad = "Difícil",
                DuracionMinutos = 300,
                PuntoEncuentro = "Punto de embarque en Tres Equis, Turrialba",
                Activo = true
            }, Color.FromArgb(21, 101, 192), 7, 95m, 14, guia2, guia1);

            context.SaveChanges();
        }

        // ---------- Auxiliares ----------

        private static IList<string> ObtenerGuias(EcoRutasContext context)
        {
            var rolId = context.Roles
                .Where(r => r.Name == RolGuia)
                .Select(r => r.Id)
                .FirstOrDefault();

            return context.Users
                .Where(u => u.Activo && u.Roles.Any(r => r.RoleId == rolId))
                .OrderBy(u => u.Email)
                .Select(u => u.Id)
                .ToList();
        }

        // Agrega el tour con sus imágenes y salidas. Las salidas alternan entre los dos guías.
        private static void CrearTour(EcoRutasContext context, Tour tour, Color color, int hora,
            decimal precio, int cupoMax, string guiaA, string guiaB)
        {
            if (context.Tours.Any(t => t.Codigo == tour.Codigo)) return;

            for (var i = 1; i <= ImagenesPorTour; i++)
            {
                tour.Imagenes.Add(new Imagen
                {
                    NombreArchivo = tour.Codigo.ToLower() + "-" + i + ".jpg",
                    TipoContenido = "image/jpeg",
                    Contenido = GenerarImagen(tour.Nombre,
                        tour.Categoria + " · Foto " + i + " de " + ImagenesPorTour, color, i),
                    Orden = i
                });
            }

            var hoy = DateTime.Today;
            for (var i = 0; i < DiasSalidas.Length; i++)
            {
                tour.Salidas.Add(new Salida
                {
                    FechaHora = hoy.AddDays(DiasSalidas[i]).AddHours(hora),
                    GuiaId = i % 2 == 0 ? guiaA : guiaB,
                    PrecioXPersona = precio,
                    CupoMax = cupoMax,
                    CuposDisponibles = cupoMax,
                    Activa = true,
                    ListaCerrada = false
                });
            }

            context.Tours.Add(tour);
        }

        // Genera una imagen JPG de 800x500 con el color del tour, un paisaje simple y el nombre.
        private static byte[] GenerarImagen(string titulo, string detalle, Color color, int numero)
        {
            const int ancho = 800;
            const int alto = 500;

            using (var imagen = new Bitmap(ancho, alto))
            using (var g = Graphics.FromImage(imagen))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                // Fondo degradado; cada foto del tour cambia la dirección del degradado.
                var area = new Rectangle(0, 0, ancho, alto);
                var direccion = (LinearGradientMode)(numero % 4);
                using (var fondo = new LinearGradientBrush(area, Aclarar(color), color, direccion))
                    g.FillRectangle(fondo, area);

                // Siluetas de montañas.
                var desplazamiento = numero * 120;
                using (var lejos = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                    g.FillPolygon(lejos, new[]
                    {
                        new Point(desplazamiento - 100, alto),
                        new Point(desplazamiento + 250, 230),
                        new Point(desplazamiento + 600, alto)
                    });
                using (var cerca = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                    g.FillPolygon(cerca, new[]
                    {
                        new Point(0, alto),
                        new Point(0, 380),
                        new Point(300, 300),
                        new Point(ancho, 400),
                        new Point(ancho, alto)
                    });

                using (var formato = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var fuenteTitulo = new Font("Segoe UI", 34, FontStyle.Bold))
                using (var fuenteDetalle = new Font("Segoe UI", 18))
                using (var sombra = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                {
                    var zonaTitulo = new RectangleF(40, 110, ancho - 80, 130);
                    var zonaSombra = zonaTitulo;
                    zonaSombra.Offset(2, 2);
                    var zonaDetalle = new RectangleF(40, 240, ancho - 80, 50);

                    g.DrawString(titulo, fuenteTitulo, sombra, zonaSombra, formato);
                    g.DrawString(titulo, fuenteTitulo, Brushes.White, zonaTitulo, formato);
                    g.DrawString(detalle, fuenteDetalle, Brushes.White, zonaDetalle, formato);
                }

                using (var memoria = new MemoryStream())
                {
                    imagen.Save(memoria, ImageFormat.Jpeg);
                    return memoria.ToArray();
                }
            }
        }

        private static Color Aclarar(Color color)
        {
            return Color.FromArgb(
                color.R + (255 - color.R) / 2,
                color.G + (255 - color.G) / 2,
                color.B + (255 - color.B) / 2);
        }
    }
}

using Microsoft.AspNet.Identity.EntityFramework;

namespace EcoRutas.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NombreCompleto { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
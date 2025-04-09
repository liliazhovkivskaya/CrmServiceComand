using Microsoft.EntityFrameworkCore;
using CrmServiceApp.Models;

namespace CrmServiceApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
    }
}

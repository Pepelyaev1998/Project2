using Microsoft.EntityFrameworkCore;
using Project2.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Project2.Services
{
    public class EntityRepository : DbContext, IEntityRepository
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Message> Messages { get; set; }

        public EntityRepository(DbContextOptions options)
              : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var adminRoleName = "admin";
            var userRoleName = "user";

            var adminEmail = "admin@mail.ru";
            var adminPassword = "Qwerty123";

            var adminRole = new Role { Id = 1, Name = adminRoleName };
            var userRole = new Role { Id = 2, Name = userRoleName };
            var adminUser = new User { Id = 1, Email = adminEmail, Password = adminPassword, RoleId = adminRole.Id };

            modelBuilder.Entity<Role>().HasData(new Role[] { adminRole, userRole });
            modelBuilder.Entity<User>().HasData(new User[] { adminUser });
            base.OnModelCreating(modelBuilder);
        }

        public void SaveEntity(Entity entity)
        {

            Entry(entity).State = entity.Id == default
                ? EntityState.Added
                : EntityState.Modified;

        }

        public void DeleteEntity(Entity entity)
        {
            Entry(entity).State = EntityState.Deleted;

        }

        public async Task SaveChanges()
        {
            await SaveChangesAsync();
        }
    }
}

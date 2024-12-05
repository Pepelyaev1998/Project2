using Microsoft.EntityFrameworkCore;

namespace Project2.Models
{
    public class DBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
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
    }
}

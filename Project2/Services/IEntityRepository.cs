using Microsoft.EntityFrameworkCore;
using Project2.Models;
using System.Threading.Tasks;

namespace Project2.Services
{
    public interface IEntityRepository
    {
        void SaveEntity(Entity entity);
        void DeleteEntity(Entity entity);
        public DbSet<Package> Packages { get; }
        public DbSet<User> Users { get; }
        public DbSet<Role> Roles { get; }
        public DbSet<Message> Messages { get; }
        Task SaveChanges();
    }
}

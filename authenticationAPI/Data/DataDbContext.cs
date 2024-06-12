using authenticationAPI.Model;
using authenticationAPI.Model.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;

namespace authenticationAPI.Data
{
    public class DataDbContext : DbContext
    {
        public DataDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
    }
}
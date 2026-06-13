using System;
using Microsoft.EntityFrameworkCore;
namespace CabinetMaster.Models;

public class CabinetMasterDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<Client> Clients { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=cabinetmaster.db");
    }
}
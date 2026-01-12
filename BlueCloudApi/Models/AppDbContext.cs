using Microsoft.EntityFrameworkCore;

namespace BlueCloudApi.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaskModel> Tasks { get; set; }
}
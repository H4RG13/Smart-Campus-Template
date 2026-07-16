using Microsoft.EntityFrameworkCore;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    public DbSet<AcademicTerm> AcademicTerms => Set<AcademicTerm>();
    public DbSet<CalendarException> CalendarExceptions => Set<CalendarException>();
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Staff> Staff => Set<Staff>();

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Further DbSets are added feature-by-feature as later PLAN.md phases introduce entities.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

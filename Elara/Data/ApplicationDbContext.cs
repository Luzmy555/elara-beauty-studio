using ElaraMVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Especialidad> Especialidades => Set<Especialidad>();
    public DbSet<EmpleadoEspecialidad> EmpleadoEspecialidades => Set<EmpleadoEspecialidad>();
    public DbSet<HorarioTrabajo> HorariosTrabajo => Set<HorarioTrabajo>();
    public DbSet<CategoriaServicio> CategoriasServicio => Set<CategoriaServicio>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<FacturaDetalle> FacturaDetalles => Set<FacturaDetalle>();
    public DbSet<ConfiguracionNegocio> ConfiguracionNegocio => Set<ConfiguracionNegocio>();
    public DbSet<HorarioNegocio> HorariosNegocio => Set<HorarioNegocio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasIndex(c => c.Email).IsUnique();
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.ApplicationUserId).IsUnique();
            entity.Property(e => e.ComisionPorcentaje).HasPrecision(5, 2);

            entity.HasOne(e => e.ApplicationUser)
                .WithOne()
                .HasForeignKey<Empleado>(e => e.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Especialidad>(entity =>
        {
            entity.HasIndex(s => s.Nombre).IsUnique();
        });

        modelBuilder.Entity<EmpleadoEspecialidad>(entity =>
        {
            entity.HasKey(ee => new { ee.EmpleadoId, ee.EspecialidadId });

            entity.HasOne(ee => ee.Empleado)
                .WithMany(e => e.EmpleadoEspecialidades)
                .HasForeignKey(ee => ee.EmpleadoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ee => ee.Especialidad)
                .WithMany(s => s.EmpleadoEspecialidades)
                .HasForeignKey(ee => ee.EspecialidadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HorarioTrabajo>(entity =>
        {
            entity.HasOne(h => h.Empleado)
                .WithMany(e => e.Horarios)
                .HasForeignKey(h => h.EmpleadoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CategoriaServicio>(entity =>
        {
            entity.HasIndex(c => c.Nombre).IsUnique();
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.Property(s => s.Precio).HasPrecision(10, 2);

            entity.HasOne(s => s.CategoriaServicio)
                .WithMany(c => c.Servicios)
                .HasForeignKey(s => s.CategoriaServicioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cita>(entity =>
        {
            entity.HasOne(c => c.Cliente)
                .WithMany()
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Empleado)
                .WithMany()
                .HasForeignKey(c => c.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Servicio)
                .WithMany()
                .HasForeignKey(c => c.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => new { c.EmpleadoId, c.FechaHoraInicio });
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(p => p.CantidadActual).HasPrecision(10, 2);
            entity.Property(p => p.CantidadMinima).HasPrecision(10, 2);
            entity.Property(p => p.PrecioCosto).HasPrecision(10, 2);
        });

        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.Property(m => m.Cantidad).HasPrecision(10, 2);

            entity.HasOne(m => m.Producto)
                .WithMany(p => p.Movimientos)
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.Property(f => f.Subtotal).HasPrecision(10, 2);
            entity.Property(f => f.Descuento).HasPrecision(10, 2);
            entity.Property(f => f.Total).HasPrecision(10, 2);
            entity.Property(f => f.MontoRecibido).HasPrecision(10, 2);

            // Único por construcción (se deriva 1 a 1 del Id autoincremental
            // justo después del insert), no hace falta un índice único aparte.
            entity.Property(f => f.NumeroFactura).HasMaxLength(20);

            entity.HasIndex(f => f.CitaId).IsUnique();

            entity.HasOne(f => f.Cita)
                .WithOne()
                .HasForeignKey<Factura>(f => f.CitaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.Cliente)
                .WithMany()
                .HasForeignKey(f => f.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FacturaDetalle>(entity =>
        {
            entity.Property(d => d.PrecioUnitario).HasPrecision(10, 2);
            entity.Property(d => d.Subtotal).HasPrecision(10, 2);
            entity.Property(d => d.ComisionEmpleado).HasPrecision(10, 2);

            entity.HasOne(d => d.Factura)
                .WithMany(f => f.FacturaDetalles)
                .HasForeignKey(d => d.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Servicio)
                .WithMany()
                .HasForeignKey(d => d.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Empleado)
                .WithMany()
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HorarioNegocio>(entity =>
        {
            entity.HasOne(h => h.ConfiguracionNegocio)
                .WithMany(c => c.Horarios)
                .HasForeignKey(h => h.ConfiguracionNegocioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

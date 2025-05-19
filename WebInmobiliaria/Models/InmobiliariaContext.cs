using Microsoft.EntityFrameworkCore;

public class InmobiliariaContext : DbContext
{
    public InmobiliariaContext(DbContextOptions<InmobiliariaContext> options) : base(options) { }

    public DbSet<Propietario> Propietarios { get; set; }
    public DbSet<Inquilino> Inquilinos { get; set; }
    public DbSet<Inmueble> Inmuebles { get; set; }
    public DbSet<TipoInmueble> TipoInmueble { get; set; }
    public DbSet<Contrato> Contratos { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Relación Propietario - Inmueble (1:N)
        modelBuilder.Entity<Inmueble>()
            .HasOne(i => i.Propietario)
            .WithMany(p => p.Inmuebles)
            .HasForeignKey(i => i.PropietarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación TipoInmueble - Inmueble (1:N)
        modelBuilder.Entity<Inmueble>()
            .HasOne(i => i.TipoInmueble)
            .WithMany(t => t.Inmuebles)
            .HasForeignKey(i => i.TipoInmuebleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Inmueble - Contrato (1:N)
        modelBuilder.Entity<Contrato>()
            .HasOne(c => c.Inmueble)
            .WithMany()
            .HasForeignKey(c => c.InmuebleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Inquilino - Contrato (1:N)
        modelBuilder.Entity<Contrato>()
            .HasOne(c => c.Inquilino)
            .WithMany()
            .HasForeignKey(c => c.InquilinoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Usuario que creó contrato (1:N)
        modelBuilder.Entity<Contrato>()
            .HasOne(c => c.UsuarioAlta)
            .WithMany()
            .HasForeignKey(c => c.UsuarioAltaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Usuario que terminó contrato (opcional)
        modelBuilder.Entity<Contrato>()
            .HasOne(c => c.UsuarioBaja)
            .WithMany()
            .HasForeignKey(c => c.UsuarioBajaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Contrato - Pagos (1:N)
        modelBuilder.Entity<Pago>()
            .HasOne(p => p.Contrato)
            .WithMany()
            .HasForeignKey(p => p.ContratoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Usuario que creó pago (1:N)
        modelBuilder.Entity<Pago>()
            .HasOne(p => p.UsuarioAlta)
            .WithMany()
            .HasForeignKey(p => p.UsuarioAltaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Usuario que anuló pago (opcional)
        modelBuilder.Entity<Pago>()
            .HasOne(p => p.UsuarioAnulacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioAnulacionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

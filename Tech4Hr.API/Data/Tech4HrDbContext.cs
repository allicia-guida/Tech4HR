using Microsoft.EntityFrameworkCore;
using Tech4Hr.API.Models;

namespace Tech4Hr.API.Data;

public class Tech4HrDbContext : DbContext
{
    public Tech4HrDbContext(
        DbContextOptions<Tech4HrDbContext> options
    ) : base(options)
    {
    }

    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Ponto> Pontos => Set<Ponto>();
    public DbSet<RegistroPonto> RegistrosPonto => Set<RegistroPonto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // FUNCIONARIO

        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.ToTable("Funcionario", "dbo");

            entity.HasKey(f => f.IdFuncionario);

            entity.Property(f => f.IdFuncionario)
                .ValueGeneratedOnAdd();

            entity.Property(f => f.Nome)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(f => f.Sobrenome)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(f => f.EmailCorporativo)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(f => f.EmailCorporativo)
                .IsUnique();

            entity.Property(f => f.SenhaHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(f => f.DataAdmissao)
                .HasColumnType("date");

            entity.Property(f => f.Ativo)
                .HasDefaultValue(true);

            entity.Property(f => f.CPF)
                .HasColumnType("char(11)")
                .IsRequired();

            entity.HasIndex(f => f.CPF)
                .IsUnique();
        });

        // USUARIO

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario", "dbo");

            entity.HasKey(u => u.IdUsuario);

            entity.Property(u => u.IdUsuario)
                .ValueGeneratedOnAdd();

            entity.Property(u => u.Nome)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.Sobrenome)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.SenhaHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(u => u.Ativo)
                .HasDefaultValue(true);

            entity.Property(u => u.NivelUsuario)
                .HasColumnType("varchar(20)")
                .IsRequired();
        });

        // PONTO

        modelBuilder.Entity<Ponto>(entity =>
        {
            entity.ToTable("Ponto", "dbo");

            entity.HasKey(p => p.IdPonto);

            entity.Property(p => p.IdPonto)
                .ValueGeneratedOnAdd();

            entity.Property(p => p.DataPonto)
                .HasColumnType("date");

            entity.Property(p => p.Entrada)
                .HasColumnType("datetime2(0)");

            entity.Property(p => p.SaidaAlmoco)
                .HasColumnType("datetime2(0)");

            entity.Property(p => p.EntradaAlmoco)
                .HasColumnType("datetime2(0)");

            entity.Property(p => p.Saida)
                .HasColumnType("datetime2(0)");

            entity.HasIndex(p => new
            {
                p.IdFuncionario,
                p.DataPonto
            }).IsUnique();

            entity.HasOne(p => p.Funcionario)
                .WithMany(f => f.Pontos)
                .HasForeignKey(p => p.IdFuncionario)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // REGISTRO PONTO

        modelBuilder.Entity<RegistroPonto>(entity =>
        {
            entity.ToTable("RegistroPonto", "dbo");

            entity.HasKey(r => r.IdRegistroPonto);

            entity.Property(r => r.IdRegistroPonto)
                .ValueGeneratedOnAdd();

            entity.Property(r => r.TipoRegistro)
                .HasColumnType("varchar(20)")
                .IsRequired();

            entity.Property(r => r.DataHora)
                .HasColumnType("datetimeoffset(0)");

            entity.Property(r => r.DataHoraRecebimento)
                .HasColumnType("datetimeoffset(0)")
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity.HasIndex(r => new
            {
                r.IdFuncionario,
                r.DataHora
            });

            entity.HasIndex(r => new
            {
                r.IdPonto,
                r.TipoRegistro
            }).IsUnique();

            entity.HasOne(r => r.Funcionario)
                .WithMany(f => f.RegistrosPonto)
                .HasForeignKey(r => r.IdFuncionario)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(r => r.Ponto)
                .WithMany(p => p.RegistrosPonto)
                .HasForeignKey(r => r.IdPonto)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}

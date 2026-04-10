using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace paczkomat.Models;

public partial class inpostContext : DbContext
{
    public inpostContext()
    {
    }

    public inpostContext(DbContextOptions<inpostContext> options)
        : base(options)
    {
    }

    public virtual DbSet<admin> admins { get; set; }

    public virtual DbSet<box> boxs { get; set; }

    public virtual DbSet<klient> klients { get; set; }

    public virtual DbSet<kurier> kuriers { get; set; }

    public virtual DbSet<kuriers_datum> kuriers_data { get; set; }

    public virtual DbSet<pack> packs { get; set; }

    public virtual DbSet<paczkomat> paczkomats { get; set; }

    public virtual DbSet<paczkomat_datum> paczkomat_data { get; set; }

    public virtual DbSet<user> users { get; set; }
    public virtual DbSet<pending_pack> pending_packs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=inpost;user=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.32-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8_polish_ci")
            .HasCharSet("utf8");

        modelBuilder.Entity<admin>(entity =>
        {
            entity.HasKey(e => e.admin_id).HasName("PRIMARY");

            entity
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_polish_ci");

            entity.HasIndex(e => e.user_id, "user_id");

            entity.Property(e => e.admin_id).HasColumnType("int(11)");
            entity.Property(e => e.user_id).HasColumnType("int(11)");

            entity.HasOne(d => d.user).WithMany(p => p.admins)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("admins_ibfk_1");
        });

        modelBuilder.Entity<box>(entity =>
        {
            entity.HasKey(e => e.box_id).HasName("PRIMARY");

            entity
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_polish_ci");

            entity.HasIndex(e => e.pack_id, "pack_id");

            entity.Property(e => e.box_id).HasColumnType("int(11)");
            entity.Property(e => e.pack_id).HasColumnType("int(11)");
            entity.Property(e => e.size).HasColumnType("text");

            entity.HasOne(d => d.pack).WithMany(p => p.boxes)
                .HasForeignKey(d => d.pack_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("boxs_ibfk_1");
        });

        modelBuilder.Entity<klient>(entity =>
        {
            entity.HasKey(e => e.klient_id).HasName("PRIMARY");

            entity
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_polish_ci");

            entity.HasIndex(e => new { e.user_id, e.pack_id }, "user_id");

            entity.Property(e => e.klient_id).HasColumnType("int(11)");
            entity.Property(e => e.pack_id).HasColumnType("int(11)");
            entity.Property(e => e.user_id).HasColumnType("int(11)");

            entity.HasOne(d => d.user).WithMany(p => p.klients)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("klients_ibfk_1");
        });

        modelBuilder.Entity<kurier>(entity =>
        {
            entity.HasKey(e => e.kurier_id).HasName("PRIMARY");

            entity
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_polish_ci");

            entity.HasIndex(e => e.user_id, "user_id");

            entity.Property(e => e.kurier_id).HasColumnType("int(11)");
            entity.Property(e => e.state).HasColumnType("text");
            entity.Property(e => e.user_id).HasColumnType("int(11)");

            entity.HasOne(d => d.user).WithMany(p => p.kuriers)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("kuriers_ibfk_1");
        });

        modelBuilder.Entity<kuriers_datum>(entity =>
        {
            entity.HasKey(e => new { e.kurier_id, e.pack_id });

            entity
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_polish_ci");

            entity.HasIndex(e => new { e.kurier_id, e.pack_id }, "kurier_id");
            entity.HasIndex(e => e.pack_id, "pack_id");

            entity.Property(e => e.kurier_id).HasColumnType("int(11)");
            entity.Property(e => e.pack_id).HasColumnType("int(11)");

            entity.HasOne(d => d.kurier).WithMany()
                .HasForeignKey(d => d.kurier_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("kuriers_data_ibfk_2");

            entity.HasOne(d => d.pack).WithMany()
                .HasForeignKey(d => d.pack_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("kuriers_data_ibfk_1");
        });

        modelBuilder.Entity<pack>(entity =>
        {
            entity.HasKey(e => e.pack_id).HasName("PRIMARY");

            entity
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_polish_ci");

            entity.HasIndex(e => e.klient_id, "klient_id");

            entity.Property(e => e.pack_id).HasColumnType("int(11)");
            entity.Property(e => e.klient_id).HasColumnType("int(11)");
            entity.Property(e => e.size).HasColumnType("text");

            entity.HasOne(d => d.klient).WithMany(p => p.packs)
                .HasForeignKey(d => d.klient_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("packs_ibfk_1");
        });

        modelBuilder.Entity<paczkomat>(entity =>
        {
            entity.HasKey(e => e.paczkomat_id).HasName("PRIMARY");

            entity
                .ToTable("paczkomat")
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_polish_ci");

            entity.Property(e => e.paczkomat_id).HasColumnType("int(11)");
            entity.Property(e => e.address).HasColumnType("text");
            entity.Property(e => e.paczkomat_name).HasColumnType("text");
        });

        modelBuilder.Entity<paczkomat_datum>(entity =>
        {
            entity
                .HasNoKey()
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_polish_ci");

            entity.HasIndex(e => e.box_id, "box_id");

            entity.HasIndex(e => new { e.paczkomat_id, e.box_id }, "paczkomat_id");

            entity.Property(e => e.box_id).HasColumnType("int(11)");
            entity.Property(e => e.paczkomat_id).HasColumnType("int(11)");

            entity.HasOne(d => d.box).WithMany()
                .HasForeignKey(d => d.box_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("paczkomat_data_ibfk_2");

            entity.HasOne(d => d.paczkomat).WithMany()
                .HasForeignKey(d => d.paczkomat_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("paczkomat_data_ibfk_1");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_polish_ci");

            entity.Property(e => e.id).HasColumnType("int(11)");
            entity.Property(e => e.email).HasColumnType("text");
            entity.Property(e => e.name).HasColumnType("text");
            entity.Property(e => e.password).HasColumnType("text");
            entity.Property(e => e.phone_number).HasColumnType("int(11)");
            entity.Property(e => e.role).HasColumnType("text");
            entity.Property(e => e.selfie).HasColumnType("text");
            entity.Property(e => e.surname).HasColumnType("text");
        });
        modelBuilder.Entity<pending_pack>(entity =>
        {
            entity.HasKey(e => e.pending_id).HasName("PRIMARY");

            entity
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_polish_ci");

            entity.Property(e => e.pending_id).HasColumnType("int(11)");
            entity.Property(e => e.sender_klient_id).HasColumnType("int(11)");
            entity.Property(e => e.receiver_klient_id).HasColumnType("int(11)");
            entity.Property(e => e.paczkomat_id).HasColumnType("int(11)");
            entity.Property(e => e.size).HasColumnType("text");
            entity.Property(e => e.status).HasMaxLength(20).HasDefaultValue("waiting");
            entity.Property(e => e.created_at).HasColumnType("datetime");

            entity.HasOne(d => d.sender_klient).WithMany()
                .HasForeignKey(d => d.sender_klient_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("pending_packs_ibfk_1");

            entity.HasOne(d => d.receiver_klient).WithMany()
                .HasForeignKey(d => d.receiver_klient_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("pending_packs_ibfk_2");

            entity.HasOne(d => d.paczkomat).WithMany()
                .HasForeignKey(d => d.paczkomat_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("pending_packs_ibfk_3");
        });
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

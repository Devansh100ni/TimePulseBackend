using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AttendenceManagement.Models;

public partial class AttendanceManagement001Context : DbContext
{
    public AttendanceManagement001Context()
    {
    }

    public AttendanceManagement001Context(DbContextOptions<AttendanceManagement001Context> options)
        : base(options)
    {
    }

    public virtual DbSet<AttAttendanceLog> AttAttendanceLogs { get; set; }

    public virtual DbSet<AttAttendanceRegister> AttAttendanceRegisters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttAttendanceLog>(entity =>
        {
            entity.ToTable("Att_AttendanceLog");

            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.AttDate)
                .HasColumnType("date")
                .HasColumnName("AttDate");
            entity.Property(e => e.Createdby)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Createddate)
                .HasColumnType("datetime")
                .HasColumnName("CreatedDate");
            entity.Property(e => e.LogTime).HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Username");
        });

        modelBuilder.Entity<AttAttendanceRegister>(entity =>
        {
            entity.ToTable("Att_AttendanceRegister");

            entity.Property(e => e.AttDate).HasColumnType("datetime");
            entity.Property(e => e.EartlyOutMinute).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.LateInMinute).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.OverTimeMinute).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Processby)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Processdate)
                .HasColumnType("datetime")
                .HasColumnName("ProcessDate");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Workhour).HasColumnType("numeric(18, 0)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

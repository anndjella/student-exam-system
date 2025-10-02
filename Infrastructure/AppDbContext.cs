using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entity;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Reflection.Emit;

namespace Infrastructure
{
    public class AppDbContext : DbContext 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Person> People => Set<Person>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<Exam> Exams => Set<Exam>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(b =>
            {
                b.ToTable("Person");
                b.HasKey(p => p.ID);

                b.Property(p => p.FirstName).HasMaxLength(50).IsRequired(); 
                b.Property(p => p.LastName).HasMaxLength(50).IsRequired();
                b.Property(p => p.DateOfBirth).HasColumnType("date").IsRequired();
                b.Property(p => p.JMBG).HasColumnType("char(13)").IsUnicode(false).IsRequired();
                b.HasIndex(p => p.JMBG).IsUnique();
                b.HasCheckConstraint(
                    "CK_Person_JMBG_13Digits",
                    "LEN([JMBG]) = 13 AND PATINDEX('%[^0-9]%', [JMBG]) = 0");

                b.Property(p => p.Age)
                .HasComputedColumnSql(
                     "DATEDIFF(YEAR, [DateOfBirth], GETDATE()) - CASE WHEN FORMAT(GETDATE(),'MMdd') < FORMAT([DateOfBirth],'MMdd') THEN 1 ELSE 0 END",
                     stored: false
                    )
                .ValueGeneratedOnAddOrUpdate();

            });

            modelBuilder.Entity<Student>(b =>
            {
                b.ToTable("Student");
                b.Property(s => s.IndexNumber).HasMaxLength(20).IsRequired();
                b.HasIndex(s => s.IndexNumber).IsUnique();

                b.Ignore(s => s.GPA);
            });

            modelBuilder.Entity<Teacher>(b =>
            {
                b.ToTable("Teacher");
                b.Property(t => t.Title).HasConversion<byte>();
                b.HasCheckConstraint("CK_Teacher_Title", "[Title] BETWEEN 1 AND 4");
            });

            modelBuilder.Entity<Exam>(b =>
            {
                b.ToTable("Exam");
                b.HasKey(e => e.ID);

                b.Property(e => e.Date)
                   .HasColumnType("date")
                   .IsRequired();

                b.Property(e => e.Grade)
                 .IsRequired();
                b.HasCheckConstraint("CK_Exam_Grade", "[Grade] BETWEEN 5 AND 10");

                b.HasOne(e => e.Student)
                 .WithMany(s => s.Exams)
                 .HasForeignKey(e => e.StudentID)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(e => e.Examiner)
                .WithMany(t => t.ExamsAsExaminer)
                .HasForeignKey(e => e.ExaminerID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.Supervisor)
                .WithMany(t => t.ExamsAsSupervisor)
                .HasForeignKey(e => e.SupervisorID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.Subject)
                 .WithMany(s => s.Exams)
                 .HasForeignKey(e => e.SubjectID)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<Subject>(b =>
            {
                b.ToTable("Subject");
                b.HasKey(s => s.ID);

                b.Property(s => s.Name)
                 .HasMaxLength(100)
                 .IsRequired();

                b.Property(s => s.ESPB)
                 .HasColumnType("tinyint")
                 .IsRequired();
                b.HasCheckConstraint("CK_Subject_Espb", "[ESPB] BETWEEN 1 AND 60");

            });
        }
    }
}

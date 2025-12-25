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
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics.CodeAnalysis;

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

        [ExcludeFromCodeCoverage]
        [Obsolete]
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var isSqlite = Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";

            modelBuilder.Entity<Person>(b =>
            {
                b.ToTable("Person");
                b.HasKey(p => p.ID);

                b.Property(p => p.FirstName).HasMaxLength(50).IsRequired();
                b.Property(p => p.LastName).HasMaxLength(50).IsRequired();
                b.Property(p => p.DateOfBirth).HasColumnType("date").IsRequired();
                b.Property(p => p.JMBG).HasColumnType("char(13)").IsUnicode(false).IsRequired();
                b.HasIndex(p => p.JMBG).IsUnique();
                if (!isSqlite)
                {
                    b.HasCheckConstraint(
                                "CK_Person_JMBG_13Digits",
                                "LEN([JMBG]) = 13 AND PATINDEX('%[^0-9]%', [JMBG]) = 0");

                    b.HasCheckConstraint(
                        "CK_Person_JMBG_DateOfBirth",
                        "LEFT([JMBG], 7) = " +
                        "    RIGHT('00' + CAST(DATEPART(DAY, [DateOfBirth]) AS varchar(2)), 2) +" +
                        "    RIGHT('00' + CAST(DATEPART(MONTH, [DateOfBirth]) AS varchar(2)), 2) +" +
                        "    RIGHT('000' + CAST(DATEPART(YEAR, [DateOfBirth]) AS varchar(4)), 3)" );

                    b.Property(p => p.Age)
                    .HasComputedColumnSql(
                         "DATEDIFF(YEAR, [DateOfBirth], GETDATE()) - CASE WHEN FORMAT(GETDATE(),'MMdd') < FORMAT([DateOfBirth],'MMdd') THEN 1 ELSE 0 END",
                         stored: false )
                    .ValueGeneratedOnAddOrUpdate();
                }

                var min = new DateOnly(1900, 1, 1);
                var max = new DateOnly(2008, 12, 31);

                if (!isSqlite)
                {
                    modelBuilder.Entity<Person>()
                                .HasCheckConstraint(
                                    "CK_Person_DateOfBirth_Range",
                                    $"[DateOfBirth] >= '{min:yyyy-MM-dd}' AND [DateOfBirth] <= '{max:yyyy-MM-dd}'"
                                );
                }});

            modelBuilder.Entity<Student>(b =>
            {
                b.ToTable("Student");
                b.Property(s => s.IndexNumber).HasMaxLength(20).IsRequired();
                b.HasIndex(s => s.IndexNumber).IsUnique();
                if (!isSqlite)
                {
                    b.HasCheckConstraint(
                               "CK_Student_IndexNumber_Format",
                               "[IndexNumber] NOT LIKE '%[^0-9/]%' " +
                               "AND CHARINDEX('/', [IndexNumber]) = 5 " +
                               "AND LEN([IndexNumber]) BETWEEN 6 AND 10 " +
                               "AND RIGHT([IndexNumber], LEN([IndexNumber]) - 5) NOT LIKE '%[^0-9]%'"

              );
                }
                b.Property(s => s.GPA)
                    .HasColumnType("decimal(4,2)")
                    .ValueGeneratedOnAddOrUpdate(); 

                b.Property(s => s.GPA).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            });

            modelBuilder.Entity<Teacher>(b =>
            {
                b.ToTable("Teacher");
                b.Property(t => t.Title).HasConversion<byte>();
                b.HasCheckConstraint("CK_Teacher_Title", "[Title] BETWEEN 1 AND 4");
            });

            modelBuilder.Entity<Exam>(b =>
            {
                b.ToTable("Exam", tb =>
                {
                    tb.HasTrigger("trg_Exam_UpdateGpa");
                });

                b.ToTable("Exam");
                b.HasKey(e => new { e.Date,e.SubjectID, e.StudentID});

                b.Property(e => e.Note)
                    .HasMaxLength(500);

                b.Property(e => e.Grade)
                 .IsRequired();
                b.HasCheckConstraint("CK_Exam_Grade", "[Grade] BETWEEN 5 AND 10");

                b.HasIndex(e => new { e.StudentID, e.SubjectID })
                 .IsUnique()
                 .HasFilter("[Grade] >= 6")
                 .HasDatabaseName("UX_Exam_PassOnce");

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

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
        public DbSet<User> Users => Set<User>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<Term> Terms => Set<Term>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<Registration> Registrations => Set<Registration>();
        public DbSet<Exam> Exams => Set<Exam>();
        public DbSet<TeachingAssignment> TeachingAssignments => Set<TeachingAssignment>();

        [ExcludeFromCodeCoverage]
        [Obsolete]
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var isSqlite = Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";

            modelBuilder.Entity<Person>(b =>
            {
                b.ToTable("Person");
                b.HasKey(p => p.ID);

                b.Property(p => p.FirstName).HasMaxLength(50);
                b.Property(p => p.LastName).HasMaxLength(50);
                b.Property(p => p.DateOfBirth).HasColumnType("date");
                b.Property(p => p.JMBG).HasColumnType("char(13)").IsUnicode(false);
                b.HasIndex(p => p.JMBG).IsUnique().HasFilter("[IsDeleted] = 0");
                if (!isSqlite)
                {
                    b.HasCheckConstraint(
                                "CK_Person_JMBG_13Digits",
                                "LEN([JMBG]) = 13 AND PATINDEX('%[^0-9]%', [JMBG]) = 0");
                
                }
                var min = new DateOnly(1900, 1, 1);
                if (!isSqlite)
                {
                    modelBuilder.Entity<Person>()
                                .HasCheckConstraint(
                                    "CK_Person_DateOfBirth_Range",
                                    $"[DateOfBirth] >= '{min:yyyy-MM-dd}' AND [DateOfBirth] <= CONVERT(date, GETDATE())"
                                );
                }
                b.HasQueryFilter(p => !p.IsDeleted);
            });

            modelBuilder.Entity<Student>(b =>
            {
                b.ToTable("Student");
                b.Property(s => s.IndexNumber).HasMaxLength(9);
                b.HasIndex(s => s.IndexNumber).IsUnique();
                if (!isSqlite)
                {
                    b.HasCheckConstraint(
                    "CK_Student_IndexNumber_Format",
                    "[IndexNumber] LIKE '[0-9][0-9][0-9][0-9]/[0-9][0-9][0-9][0-9]'"
                );
                }
                b.Ignore(s => s.GPA);
                b.Ignore(s => s.ECTSCount);
            });

            modelBuilder.Entity<Teacher>(b =>
            {
                b.ToTable("Teacher");
                b.HasCheckConstraint("CK_Teacher_Title", "[Title] BETWEEN 1 AND 4");
                b.Property(t=>t.EmployeeNumber).HasMaxLength(9);
                b.HasIndex(s => s.EmployeeNumber).IsUnique();
                if (!isSqlite)
                {
                    b.HasCheckConstraint(
                    "CK_Teacher_EmployeeNumber_Format",
                    "[EmployeeNumber] LIKE '[0-9][0-9][0-9][0-9]/[0-9][0-9][0-9][0-9]'"
                );
                }
            });

            modelBuilder.Entity<Exam>(b =>
            {
                b.ToTable("Exam");
                b.HasKey(e=>e.ID);
                b.Property(e => e.Note).HasMaxLength(500);
                b.HasCheckConstraint("CK_Exam_Grade", "[Grade] BETWEEN 5 AND 10");

                b.HasOne(e => e.Registration)
                 .WithOne(r => r.Exam)
                 .HasForeignKey<Exam>(e => new { e.StudentID, e.SubjectID, e.TermID })
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.Teacher)
                 .WithMany(t => t.SignedExams)
                 .HasForeignKey(e => e.TeacherID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Subject>(b =>
            {
                b.ToTable("Subject");
                b.HasKey(s => s.ID);
                b.Property(s => s.Name).HasMaxLength(100);       
                b.HasCheckConstraint("CK_Subject_ECTS", "[ECTS] BETWEEN 1 AND 15");
                b.HasQueryFilter(s => !s.IsDeleted);
            });

            modelBuilder.Entity<Term>(t =>
            {
                t.ToTable("Term");
                t.HasKey(s => s.ID);

                t.Property(s => s.Name).HasMaxLength(50);
                t.Property(s => s.StartDate).HasColumnType("date");
                t.Property(s => s.EndDate).HasColumnType("date");
                t.Property(s => s.RegistrationStartDate).HasColumnType("date");
                t.Property(s => s.RegistrationEndDate).HasColumnType("date");

                t.HasCheckConstraint("CK_Term_EndDate_After_StartDate","[EndDate] > [StartDate]" );
                t.HasCheckConstraint("CK_Term_RegEndDate_After_RegStartDate", "[RegistrationEndDate] > [RegistrationStartDate]");
            });
           

            modelBuilder.Entity<Enrollment>(e =>
            {
                e.ToTable("Enrollment");
                e.HasKey(m => new { m.StudentID, m.SubjectID});

                e.HasOne(r => r.Student)
                  .WithMany(s => s.Enrollments)
                  .HasForeignKey(r => r.StudentID)
                  .OnDelete(DeleteBehavior.Restrict);

               e.HasOne(r => r.Subject)
                 .WithMany(su => su.Enrollments)
                 .HasForeignKey(r => r.SubjectID)
                 .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<TeachingAssignment>(t =>
            {
                t.ToTable("TeachingAssignment");
                t.HasKey(e => new { e.SubjectID, e.TeacherID });

                t.HasOne(x => x.Teacher)
                 .WithMany(g => g.TeachingAssignments)
                 .HasForeignKey(x => x.TeacherID)
                 .OnDelete(DeleteBehavior.Cascade);

                t.HasOne(x => x.Subject)
                 .WithMany(s => s.TeachingAssignments)
                 .HasForeignKey(x => x.SubjectID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Registration>(r =>
            {
                r.ToTable("Registration");
                r.HasKey(e => new { e.SubjectID, e.StudentID, e.TermID });

                r.HasOne(m => m.Student)
                  .WithMany(s => s.Registrations)
                  .HasForeignKey(m => m.StudentID)
                  .OnDelete(DeleteBehavior.Cascade);

                r.HasOne(m => m.Subject)
                 .WithMany(su => su.Registrations)
                 .HasForeignKey(m => m.SubjectID)
                 .OnDelete(DeleteBehavior.Restrict);

                r.HasOne(m => m.Term)
                 .WithMany(t => t.Registrations)
                 .HasForeignKey(m => m.TermID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("User");
                b.HasKey(u => u.ID);

                b.Property(u => u.Username).HasMaxLength(20);
                b.HasIndex(u => u.Username).IsUnique();
                b.Property(u => u.PasswordHash).HasMaxLength(200);
                b.Property(u => u.isActive).HasDefaultValue(true);
                b.HasCheckConstraint("CK_User_Role", "[Role] BETWEEN 1 AND 3");

                b.HasOne(p => p.Person)
                 .WithOne(u => u.User)
                 .HasForeignKey<User>(p => p.PersonID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StudentStats>(b =>
            {
                b.ToView("vw_StudentStats");
                b.HasNoKey();

                b.Property(x => x.StudentID).HasColumnName("StudentID");
                b.Property(x => x.GPA).HasColumnType("decimal(4,2)");
                b.Property(x => x.ECTSCount);
            });
        }
    }
}

using Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Abstractions
{
    public interface IUnitOfWork
    {
        IStudentRepository Students { get; }
        ITeacherRepository Teachers { get; }
        IPersonRepository People { get; }
        IUserRepository Users { get; }
        ISubjectRepository Subjects { get; }
        IEnrollmentRepository Enrollments { get; }
        ITeachingAssignmentRepository TeachingAssignments { get; }
        ITermRepository Terms { get; }
        IRegistrationRepository Registrations { get; }
        IExamRepository Exams { get; }
        IStudentStatsRepository StudentStats { get; }

        Task<int> CommitAsync(CancellationToken ct = default);
    }
}

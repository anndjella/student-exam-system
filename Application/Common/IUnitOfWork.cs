using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
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

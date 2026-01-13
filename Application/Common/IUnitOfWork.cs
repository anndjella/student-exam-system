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
        ISchoolYearRepository SchoolYears { get; }
        IEnrollmentRepository Enrollments { get; }
        ITeachingAssignmentRepository TeachingAssignments { get; }

        Task<int> CommitAsync(CancellationToken ct = default);
    }
}

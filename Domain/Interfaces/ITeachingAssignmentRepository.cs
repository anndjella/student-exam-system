using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITeachingAssignmentRepository
    {
        //exist check
        Task<bool> ExistsAsync(int teacherId, int subjectId, CancellationToken ct);
        //read
        Task<TeachingAssignment?> GetAsync(int teacherId, int subjectId, CancellationToken ct);
        Task<List<TeachingAssignment>> ListByTeacherIdAsync(int teacherId, CancellationToken ct);
        Task<List<TeachingAssignment>> ListBySubjectIdAsync(int subjectId, CancellationToken ct);
        // write
        void Add(TeachingAssignment teachingAssignment);
        Task<bool> CanTeacherGradeAsync(int teacherId, int subjectId, CancellationToken ct);


        //delete
        void Remove(TeachingAssignment assignment);
    }
}

using Application.DTO.Me.Teacher;
using Application.DTO.Subjects;
using Application.DTO.TeachingAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface ITeachingAssignmentService
    {
        Task<TeachingAssignmentResponse> CreateAsync(CreateTeachingAssignmentRequest req,CancellationToken ct);
        Task UpdateCanGradeAsync(CreateTeachingAssignmentRequest req,CancellationToken ct);

        Task DeleteAsync(int teacherId, int subjectId, CancellationToken ct);
        Task<TeachingAssignmentResponse> GetAsync(int teacherId, int subjectId, CancellationToken ct);
        Task<bool> CanTeacherGradeAsync(int teacherId, int subjectId, CancellationToken ct);
        Task<List<TeachingAssignmentResponse>> ListByTeacherAsync(int teacherId, CancellationToken ct);
        Task<List<TeachingAssignmentResponse>> ListBySubjectAsync(int subjectId, CancellationToken ct);
        Task<TeacherSubjectsResponse> ListTeacherSubjectsDividedAsync(int personId, CancellationToken ct);

    }
}

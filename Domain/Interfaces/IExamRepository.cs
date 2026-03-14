using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IExamRepository
    {
        Task<Exam?> GetByKeyAsync(int studentId, int subjectId, int termId, CancellationToken ct = default);
        Task<List<Exam>> ListUnsignedBySubjectTermAsync(int subjectId, int termId, CancellationToken ct = default);
        Task<List<Exam>> ListBySubjectTermAsync(int subjectId, int termId, CancellationToken ct = default);
        Task<List<Exam>> ListUnsignedBySubjectTermWithRegistrationAsync(int subjectId, int termId,CancellationToken ct = default);
        Task<List<Exam>> ListSignedByStudentIdAsync(int studentId, CancellationToken ct = default);
        public Task<List<Exam>> ListAllBySubjectTermForTeacherAsync(int subjectId, int termId, int teacherId, CancellationToken ct = default);
        Task<bool> ExistsAnyForTermAsync(int termId, CancellationToken ct = default);
        Task<bool> ExistsAnyForSubjectAsync(int subjectId, CancellationToken ct=default);
        Task<bool> ExistsAnyForSubjectAndStudentAsync(int subjectId, int studentId, CancellationToken ct = default);
        Task<int> CountPagedAsync(int subjectId,int termId,string? query,CancellationToken ct = default);

        Task<List<Exam>> ListPagedAsync(int subjectId,int termId,int skip,int take,string? query,CancellationToken ct = default);

        Task<int> CountUnsignedBySubjectTermAsync(int subjectId, int termId, CancellationToken ct = default);
        void Add(Exam exam);
    }
}

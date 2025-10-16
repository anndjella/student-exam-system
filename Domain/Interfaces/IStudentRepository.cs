using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public  interface IStudentRepository
    {
        // existence checks
        Task<bool> ExistsByJmbgAsync(string jmbg, CancellationToken ct = default);
        Task<bool> ExistsByIndexAsync(string indexNumber, CancellationToken ct = default);

        // read
        Task<Student?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Student>> ListAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Exam>> GetExamsAsync(int studentId, CancellationToken ct = default);


        // write
        Task<int> CreateAsync(Student student, CancellationToken ct = default);
        Task UpdateAsync(Student student, CancellationToken ct = default);
        Task DeleteAsync(Student student, CancellationToken ct = default);
    }
}

using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITeacherRepository
    {
        //existence check
        Task<bool> ExistsByEmployeeNumAsync(string employeeNum, CancellationToken ct = default);
        //read
        Task<Teacher?> GetByIdAsync(int id, CancellationToken ct = default);
        //write
        Task<int> CreateAsync(Teacher teacher, CancellationToken ct = default);
        Task UpdateAsync(Teacher teacher, CancellationToken ct = default);
        //delete
        Task DeleteByIdAsync(int teacherId, CancellationToken ct = default);

    }
}

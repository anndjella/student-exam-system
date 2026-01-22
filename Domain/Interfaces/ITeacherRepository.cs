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
        Task<bool> ExistsByIdAsync(int id, CancellationToken ct = default);
        //read
        Task<Teacher?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Teacher?> GetByIdWithUserAsync(int id, CancellationToken ct = default);
        Task<Teacher?> GetByEmployeeNumAsync(string employeeNum, CancellationToken ct = default);
        // write
        void Add(Teacher teacher);
    }
}

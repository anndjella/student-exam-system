using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IUserService
    {
        //Task<int> CreateForStudentAsync(Student student, CancellationToken ct = default);
        //Task<int> CreateForTeacherAsync(Teacher teacher, CancellationToken ct = default);

        //Task DeactivateByPersonIdAsync(int personId, CancellationToken ct = default);

        //Task ChangePasswordAsync(int userId, string newPasswordPlain, CancellationToken ct = default);
        //Task<User> CreateForStudentAsync(Student student, CancellationToken ct = default);
        //Task<User> CreateForTeacherAsync(Teacher teacher, CancellationToken ct = default);
        //Task DeactivateByPersonIdAsync(int personId, CancellationToken ct = default);
        //Task ChangePasswordAsync(int userId, string newPasswordPlain, CancellationToken ct = default);

        Task DeactivateByPersonIdAsync(int personId, CancellationToken ct = default);
        Task ChangePasswordAsync(int userId, string newPasswordPlain, CancellationToken ct = default);
    }
}

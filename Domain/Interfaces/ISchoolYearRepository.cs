using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ISchoolYearRepository
    {
        Task<bool> ExistsByIdAsync(int id, CancellationToken ct);
    }
}

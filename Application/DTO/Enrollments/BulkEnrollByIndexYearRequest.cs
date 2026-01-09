using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Enrollments
{
    public sealed record BulkEnrollByIndexYearRequest(int IndexStartYear,int SchoolYearId, List<int> SubjectIds);
}

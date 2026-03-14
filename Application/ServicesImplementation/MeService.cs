using Application.Common;
using Application.DTO.Me;
using Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServicesImplementation
{
    public sealed class MeService:IMeService
    {
        private readonly IUnitOfWork _uow;
        public MeService(IUnitOfWork uow) => _uow = uow;

        public async Task<MeResponse> GetMeAsync(int personId, CancellationToken ct = default)
        {
            var p = await _uow.People.GetByIdAsync(personId, ct);
            if (p is null)
                throw new AppException(AppErrorCode.NotFound, $"Person with id {personId} not found.");

            var resp = new MeResponse
            {
                FirstName = p.FirstName,
                LastName = p.LastName,
                JMBG = p.JMBG,
                DateOfBirth = p.DateOfBirth
            };

            var s = await _uow.Students.GetByIdAsync(personId, ct);
            if (s is not null)
            {
                var stats = await _uow.StudentStats.GetByStudentIdAsync(personId, ct);
                resp.IndexNumber = s.IndexNumber;
                resp.GPA = stats?.GPA;
                resp.ECTSCount = stats?.ECTSCount;
                return resp;
            }

            var t = await _uow.Teachers.GetByIdAsync(personId, ct);
            if (t is not null)
            {
                resp.EmployeeNumber = t.EmployeeNumber;
                resp.Title = t.Title;
                return resp;
            }

            return resp;
        }
    }
}

using Application.Common;
using Application.DTO.Enrollments;
using Application.Services;
using Domain.Entity;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServicesImplementation
{
    public sealed class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _uow;

        public EnrollmentService(IUnitOfWork uow) => _uow = uow;

        public async Task<BulkEnrollResult> BulkEnrollByIndexYearAsync(
            BulkEnrollByIndexYearRequest req,
            CancellationToken ct)
        {          
            if (!await _uow.SchoolYears.ExistsByIdAsync(req.SchoolYearId, ct))
                throw new AppException(AppErrorCode.NotFound, $"SchoolYear {req.SchoolYearId} not found.");

            var distinctSubjectIds = req.SubjectIds.Distinct().ToList();

            var existingSubjects = await _uow.Subjects.GetExistingIdsAsync(distinctSubjectIds, ct);
            if (existingSubjects.Count != distinctSubjectIds.Count)
            {
                var missing = distinctSubjectIds.Except(existingSubjects).ToList();
                throw new AppException(AppErrorCode.NotFound, $"Some subjects not found: {string.Join(",", missing)}");
            }

            var prefix = req.IndexStartYear.ToString(CultureInfo.InvariantCulture) + "/";
            var studentIds = await _uow.Students.ListIdsByIndexPrefixAsync(prefix, ct);

            if (studentIds.Count == 0)
                return new BulkEnrollResult(StudentsMatched: 0, EnrollmentsCreated: 0, EnrollmentsSkipped: 0);

            // 2) Ucitaj postojece enrollmente da izbegnes duplikate
            var existingPairs = await _uow.Enrollments.ListExistingPairsAsync(
                studentIds,
                req.SchoolYearId,
                distinctSubjectIds,
                ct);

            // 3) Kreiraj samo nove
            var toCreate = new List<Enrollment>(capacity: studentIds.Count * distinctSubjectIds.Count);

            foreach (var sid in studentIds)
            {
                foreach (var subId in distinctSubjectIds)
                {
                    if (existingPairs.Contains((sid, subId)))
                        continue;

                    toCreate.Add(new Enrollment
                    {
                        StudentID = sid,
                        SubjectID = subId,
                        SchoolYearID = req.SchoolYearId,
                        Status = EnrollmentStatus.Active
                    });
                }
            }

            _uow.Enrollments.AddRange(toCreate);
            await _uow.CommitAsync(ct);

            var totalRequested = studentIds.Count * distinctSubjectIds.Count;
            var created = toCreate.Count;
            var skipped = totalRequested - created;

            return new BulkEnrollResult(
                StudentsMatched: studentIds.Count,
                EnrollmentsCreated: created,
                EnrollmentsSkipped: skipped
            );
        }
    }
}

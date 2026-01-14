using Application.Common;
using Application.DTO.Term;
using Application.Services;
using Domain.Entity;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServicesImplementation
{
    public sealed class TermService : ITermService
    {
        private readonly IUnitOfWork _uow;
        public TermService(IUnitOfWork uow) => _uow = uow;
        private static DateOnly TodayUtc => DateOnly.FromDateTime(DateTime.UtcNow);

        public async Task<TermResponse> CreateAsync(CreateTermRequest req, CancellationToken ct = default)
        {
            if (await _uow.Terms.ExistsOverlapAsync(req.StartDate, req.EndDate,excludeId:null, ct))
                throw new AppException(AppErrorCode.Conflict, "Term dates overlap with an existing term.");

            var term = new Term
            {
                Name = req.TermName.Trim(),
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                RegistrationStartDate = req.RegistrationStartDate,
                RegistrationEndDate = req.RegistrationEndDate
            };

            _uow.Terms.Add(term);
            await _uow.CommitAsync(ct);

            return Mapper.TermToResponse(term);
        }

        public async Task<TermResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var term = await _uow.Terms.GetByIdAsync(id, ct);
            if (term is null)
                throw new AppException(AppErrorCode.NotFound, $"Term {id} not found.");

            return Mapper.TermToResponse(term);
        }
        public async Task UpdateAsync(int id, UpdateTermRequest req, CancellationToken ct = default)
        {
            var term = await _uow.Terms.GetByIdAsync(id, ct)
                ?? throw new AppException(AppErrorCode.NotFound, $"Term {id} not found.");

            var hasSigned = await _uow.Exams.ExistsSignedForTermAsync(id, ct);

            if (req.Name is not null)
                term.Name = req.Name.Trim();

            if (hasSigned)
            {
                if (req.StartDate is not null || req.EndDate is not null ||
                    req.RegistrationStartDate is not null || req.RegistrationEndDate is not null)
                    throw new AppException(AppErrorCode.Conflict, "Term has signed exams. Dates cannot be changed.");
            }
            else
            {
                var newStart = req.StartDate ?? term.StartDate;
                var newEnd = req.EndDate ?? term.EndDate;
                var newRegStart = req.RegistrationStartDate ?? term.RegistrationStartDate;
                var newRegEnd = req.RegistrationEndDate ?? term.RegistrationEndDate;

                if (await _uow.Terms.ExistsOverlapAsync(newStart, newEnd,id, ct))
                    throw new AppException(AppErrorCode.Conflict, "Term dates overlap with an existing term.");

                term.StartDate = newStart;
                term.EndDate = newEnd;
                term.RegistrationStartDate = newRegStart;
                term.RegistrationEndDate = newRegEnd;
            }

            await _uow.CommitAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var term = await _uow.Terms.GetByIdAsync(id, ct)
                ?? throw new AppException(AppErrorCode.NotFound, $"Term {id} not found.");

            if(await _uow.Registrations.ExistsAnyForTermAsync(id, ct))
                throw new AppException(AppErrorCode.Conflict, "Term cannot be deleted because it has registrations.");

            if(await _uow.Exams.ExistsAnyForTermAsync(id, ct))
                 throw new AppException(AppErrorCode.Conflict, "Term cannot be deleted because it has exams.");

            _uow.Terms.Remove(term);
            await _uow.CommitAsync(ct);
        }

        public Task<List<Term>> ListAsync(UserRole role, CancellationToken ct)
        => role switch
            {
                UserRole.StudentService => _uow.Terms.ListAllAsync(ct),
                UserRole.Student or UserRole.Teacher=> _uow.Terms.ListCurrentAndFutureAsync(TodayUtc, ct),
                _ => throw new AppException(AppErrorCode.Forbidden, "Role not supported.")
            };       
        public async Task<List<Term>> ListForGradingAsync(CancellationToken ct)
        => await _uow.Terms.ListCurrentAndPrev2Async(TodayUtc, ct);
        
        public async Task<List<Term>> ListOpenForRegistrationAsync(CancellationToken ct)
        => await _uow.Terms.ListOpenForRegistrationAsync(TodayUtc, ct);
    }
}

using Application.Common;
using Application.DTO.Term;
using Application.Services;
using Domain.Entity;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
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
        private readonly IClock _clock;
        public TermService(IUnitOfWork uow, IClock clock)
        {
            _uow = uow;
            _clock = clock;
        }  

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
                UserRole.Student or UserRole.Teacher=> _uow.Terms.ListCurrentAndFutureAsync(_clock.Today, ct),
                _ => throw new AppException(AppErrorCode.Forbidden, "Role not supported.")
            };       
        public async Task<List<Term>> ListForGradingAsync(CancellationToken ct)
        => await _uow.Terms.ListCurrentAndPrev2Async(_clock.Today, ct);
        
        public async Task<List<Term>> ListOpenForRegistrationAsync(CancellationToken ct)
        => await _uow.Terms.ListOpenForRegistrationAsync(_clock.Today, ct);
    }
}

using Application.DTO.Term;
using Domain.Entity;

namespace Application.Common.Mapping
{
    public static class TermMapper
    {
        public static TermResponse ToResponse(Term term) => new()
        {
            TermID = term.ID,
            TermName = term.Name,
            StartDate = term.StartDate,
            EndDate = term.EndDate,
            RegistrationEndDate = term.RegistrationEndDate,
            RegistrationStartDate = term.RegistrationStartDate
        };
    }
}

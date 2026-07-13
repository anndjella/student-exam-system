using Domain.Entity;

namespace Application.Common.Mapping
{
    internal static class MappingHelpers
    {
        public static string FullName(Person? person)
            => person is null ? string.Empty : $"{person.FirstName} {person.LastName}";
    }
}

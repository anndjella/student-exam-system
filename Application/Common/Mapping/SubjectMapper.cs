using Application.DTO.Subjects;
using Domain.Entity;

namespace Application.Common.Mapping
{
    public static class SubjectMapper
    {
        public static SimpleSubjectResponse ToSimpleResponse(Subject subject) => new()
        {
            ID = subject.ID,
            Name = subject.Name,
            Code = subject.Code
        };

        public static SubjectResponse ToResponse(Subject subject) => new()
        {
            ID = subject.ID,
            Name = subject.Name,
            ECTS = subject.ECTS,
            Code = subject.Code,
            Teachers = (subject.TeachingAssignments ?? Enumerable.Empty<TeachingAssignment>())
                .Where(ta => ta.Teacher != null)
                .Select(ta => new SubjectTeacherItem
                {
                    ID = ta.TeacherID,
                    FirstName = ta.Teacher!.FirstName,
                    LastName = ta.Teacher!.LastName,
                })
                .ToList()
        };

        public static StudServiceSubjectResponse ToAdminResponse(Subject subject) => new()
        {
            ID = subject.ID,
            Name = subject.Name,
            ECTS = subject.ECTS,
            Code = subject.Code,
            IsActive = subject.IsActive,
            Teachers = (subject.TeachingAssignments ?? Enumerable.Empty<TeachingAssignment>())
                .Where(ta => ta.Teacher != null)
                .Select(ta => new StudServiceSubjectTeacherItem
                {
                    ID = ta.TeacherID,
                    FirstName = ta.Teacher!.FirstName,
                    LastName = ta.Teacher!.LastName,
                    EmployeeNumber = ta.Teacher!.EmployeeNumber,
                    Title = ta.Teacher!.Title,
                    CanGrade = ta.CanGrade
                })
                .ToList()
        };
    }
}

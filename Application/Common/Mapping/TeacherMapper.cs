using Application.DTO.Teachers;
using Domain.Entity;

namespace Application.Common.Mapping
{
    public static class TeacherMapper
    {
        public static TeacherResponse ToResponse(Teacher teacher) => new()
        {
            ID = teacher.ID,
            FirstName = teacher.FirstName,
            DateOfBirth = teacher.DateOfBirth,
            LastName = teacher.LastName,
            EmployeeNumber = teacher.EmployeeNumber,
            Title = teacher.Title,
            DeletedAt = teacher.DeletedAt
        };

        public static Teacher CreateToTeacher(CreateTeacherRequest req, int id) => new()
        {
            ID = id,
            JMBG = req.JMBG,
            FirstName = req.FirstName,
            LastName = req.LastName,
            EmployeeNumber = req.EmployeeNumber,
            Title = req.Title
        };
    }
}

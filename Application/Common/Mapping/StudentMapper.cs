using Application.DTO.Students;
using Domain.Entity;

namespace Application.Common.Mapping
{
    public static class StudentMapper
    {
        public static StudentResponse ToResponse(Student student) => new()
        {
            ID = student.ID,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            DateOfBirth = student.DateOfBirth,
            GPA = null,
            ECTSCount = null,
            IndexNumber = student.IndexNumber
        };

        public static StudentResponse ToResponseWithStats(Student student, StudentStats? stats) => new()
        {
            ID = student.ID,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            DateOfBirth = student.DateOfBirth,
            GPA = stats?.GPA is null ? null : (double)stats.GPA.Value,
            ECTSCount = stats?.ECTSCount,
            IndexNumber = student.IndexNumber,
            DeletedAt = student.DeletedAt
        };

        public static Student CreateToStudent(CreateStudentRequest req, int id) => new()
        {
            ID = id,
            JMBG = req.JMBG,
            FirstName = req.FirstName,
            LastName = req.LastName,
            Email = req.Email,
            IndexNumber = req.IndexNumber
        };
    }
}

using Application.DTO.Students;
using Application.Validators.Student;
using System;
using FluentValidation.TestHelper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Application
{
    public class CreateStudentValidatorTests
    {
        private readonly CreateStudentValidator _validator;
        public CreateStudentValidatorTests()
        {
            _validator = new CreateStudentValidator();  
        }
        [Fact]
        public void Create_ValidModel_Passes()
        {
            CreateStudentRequest model = new CreateStudentRequest
            {
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1998, 4, 15),
                JMBG = "1504998710016",
                IndexNumber = "2024/1234"
            };

            var r = _validator.TestValidate(model, o => o.IncludeRuleSets("Create"));
            r.ShouldNotHaveAnyValidationErrors();
        }
        [Fact]
        public void Create_Requires_CoreFields()
        {
            CreateStudentRequest model = new CreateStudentRequest
            {
                FirstName = "",
                LastName = "",
                DateOfBirth = default,
                JMBG = "",
                IndexNumber = ""
            };

            var r = _validator.TestValidate(model, o => o.IncludeRuleSets("Create"));
            r.ShouldHaveValidationErrorFor(x => x.FirstName);
            r.ShouldHaveValidationErrorFor(x => x.LastName);
            r.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
            r.ShouldHaveValidationErrorFor(x => x.JMBG);
            r.ShouldHaveValidationErrorFor(x => x.IndexNumber);
        }
        [Theory]
        [InlineData("2024/")]
        [InlineData("24/1234")]
        [InlineData("2024-1234")]
        [InlineData("202A/1234")]
        [InlineData("2024/1234567")]
        public void Create_IndexNumber_BadFormats_Fails(string badIndex)
        {
            CreateStudentRequest model = new CreateStudentRequest
            {
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1998, 4, 15),
                JMBG = "1504998710016",
                IndexNumber = badIndex
            };

            var r = _validator.TestValidate(model, o => o.IncludeRuleSets("Create"));
            r.ShouldHaveValidationErrorFor(x => x.IndexNumber);
        }
        [Fact]
        public void Create_Jmbg_MustBeDigits_Length13_Checksum()
        {
            CreateStudentRequest m1 = new CreateStudentRequest
            {
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1998, 4, 15),
                JMBG = "15049987100A6",
                IndexNumber = "2024/1234"
            };
            var r1 = _validator.TestValidate(m1, o => o.IncludeRuleSets("Create"));
            r1.ShouldHaveValidationErrorFor(x => x.JMBG)
                .WithErrorMessage("JMBG must contain only digits.");

            CreateStudentRequest m2 = new CreateStudentRequest
            {
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1998, 4, 15),
                JMBG = "150499871001",
                IndexNumber = "2024/1234"
            };
            var r2 = _validator.TestValidate(m2, o => o.IncludeRuleSets("Create"));
            r2.ShouldHaveValidationErrorFor(x => x.JMBG)
                .WithErrorMessage("JMBG must be 13 digits.");

            CreateStudentRequest m3 = new CreateStudentRequest
            {
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1998, 4, 15),
                JMBG = "1504998710017",
                IndexNumber = "2024/1234"
            };
            var r3 = _validator.TestValidate(m3, o => o.IncludeRuleSets("Create"));
            r3.ShouldHaveValidationErrorFor(x => x.JMBG)
                .WithErrorMessage("Check digit of JMBG is not valid.");
        }

        [Fact]
        public void Create_Dob_MustMatch_Jmbg_DatePart()
        {
            CreateStudentRequest model = new CreateStudentRequest
            {
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1998, 4, 16),
                JMBG = "1504998710016",
                IndexNumber = "2024/1234"
            };

            var r = _validator.TestValidate(model, o => o.IncludeRuleSets("Create"));
            r.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("DateOfBirth doesn't match the date from JMBG.");
        }

        [Theory]
        [InlineData(1899, 12, 31)]
        [InlineData(2009, 1, 1)]
        public void Create_Dob_OutOfRange_Fails(int y, int m, int d)
        {
            CreateStudentRequest model = new CreateStudentRequest
            {
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(y, m, d),
                JMBG = "1504998710016",
                IndexNumber = "2024/1234"
            };

            var r = _validator.TestValidate(model, o => o.IncludeRuleSets("Create"));
            r.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
        }
    }
}

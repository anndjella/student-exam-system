using Application.DTO.Students;
using Application.Validators.Student;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Application
{
    public class UpdateStudentValidatorTests
    {
        private readonly UpdateStudentValidator _validator;
        public UpdateStudentValidatorTests()
        {
            _validator = new UpdateStudentValidator();
        }

        [Fact]
        public void Update_MinimalValid_Passes()
        {
            UpdateStudentRequest model = new UpdateStudentRequest
            {
                FirstName = "Ana",
                LastName = "Anić",
                IndexNumber = null
            };

            var r = _validator.TestValidate(model, o => o.IncludeRuleSets("Update"));
            r.ShouldNotHaveAnyValidationErrors();
        }
        [Fact]
        public void Update_FirstName_IsEmpty_Error()
        {
            UpdateStudentRequest model = new UpdateStudentRequest
            {
                FirstName = "",
                LastName = "Anić",
                IndexNumber = null
            };

            var r = _validator.TestValidate(model, o => o.IncludeRuleSets("Update"));
            r.ShouldHaveValidationErrorFor(x => x.FirstName)
                .WithErrorMessage("First name is required.");
        }

        [Theory]
        [InlineData("24/1234")]
        [InlineData("2024-1234")]
        [InlineData("2024/1234567")]
        public void Update_IndexNumber_WhenProvided_MustMatchPattern(string badIndex)
        {
            var model = new UpdateStudentRequest
            {
                FirstName = "Ana",
                LastName = "Anić",
                IndexNumber = badIndex
            };

            var r = _validator.TestValidate(model, o => o.IncludeRuleSets("Update"));
            r.ShouldHaveValidationErrorFor(x => x.IndexNumber);
        }      
    }
}

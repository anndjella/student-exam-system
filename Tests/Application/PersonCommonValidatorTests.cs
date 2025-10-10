using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using FluentValidation.TestHelper;
using Moq;

namespace Tests.Application
{
    public class PersonCommonValidatorTests
    {
        private static PersonCommonValidator<IPersonCreate> MakeValidator()
            =>new PersonCommonValidator<IPersonCreate>();

        private static IPersonCreate ValidSample()
        {
            var m = new Mock<IPersonCreate>();
            m.SetupGet(x => x.FirstName).Returns("Ana");
            m.SetupGet(x => x.LastName).Returns("Petrović");
            m.SetupGet(x => x.JMBG).Returns("0101000710006");
            m.SetupGet(x => x.DateOfBirth).Returns(new DateOnly(2000, 1, 1));
            return m.Object;
        }
        [Fact]
        public void Does_not_HaveError()
        {
            var v = MakeValidator();
            var r = v.TestValidate(ValidSample());
            r.ShouldNotHaveValidationErrorFor(x => x.LastName);
        }

        [Fact]
        public void FirstName_Empty_HasError()
        {
            var v = MakeValidator();

            var mock = new Mock<IPersonCreate>();
            mock.SetupGet(x => x.FirstName).Returns("");
            mock.SetupGet(x => x.LastName).Returns("Petrović");
            mock.SetupGet(x => x.JMBG).Returns("0101000710006");
            mock.SetupGet(x => x.DateOfBirth).Returns(new DateOnly(2000, 1, 1));

            var r = v.TestValidate(mock.Object);
            r.ShouldHaveValidationErrorFor(x => x.FirstName);
        }
        [Fact]
        public void JMBG_NotMatching_DateOfBirth_HasError()
        {
            var v = MakeValidator();

            var mock = new Mock<IPersonCreate>();
            mock.SetupGet(x => x.FirstName).Returns("Milica");
            mock.SetupGet(x => x.LastName).Returns("Petrović");
            mock.SetupGet(x => x.JMBG).Returns("0101000710006");
            mock.SetupGet(x => x.DateOfBirth).Returns(new DateOnly(2000, 1, 10));

            var r = v.TestValidate(mock.Object);
            r.ShouldHaveValidationErrorFor(x => x.JMBG);
        }
    }
}

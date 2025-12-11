using Domain.Validation;
namespace Tests.Application
{
    public class JMBGTests
    {
        [Fact]
        public void Validator_ReturnsTrue_AllDigits()
        {
            string JMBG = "0101990712345";
            var result=JmbgValidation.IsAllDigits(JMBG);
            Assert.True(result);
        }
        [Fact]
        public void Validator_ReturnsFalse_AllDigits()
        {
            string JMBG = "01O1990712345";
            var result = JmbgValidation.IsAllDigits(JMBG);
            Assert.False(result);
        }
        [Fact]
        public void Validator_ReturnsTrue_13Digits()
        {
            string JMBG = "1234567895372";
            var result = JmbgValidation.IsAllDigits(JMBG);
            Assert.True(result);
        }
        [Fact]
        public void Validator_ReturnsFalse_13Digits()
        {
            string JMBG = "12345678";
            var result = JmbgValidation.IsAllDigits(JMBG);
            Assert.False(result);
        }
        [Fact]
        public void Validator_ReturnsTrue_ParseDate()
        {
            string JMBG = "0101990712345";

            bool ok=JmbgValidation.TryParseDate(JMBG,out DateOnly dob);

            Assert.True(ok);
            Assert.Equal(new DateOnly(1990, 1, 1), dob);        
        }
        [Fact]
        public void Validator_ReturnsFalse_ParseDate()
        {
            string JMBG = "0101990712345";

            bool ok = JmbgValidation.TryParseDate(JMBG, out DateOnly dob);

            Assert.True(ok);
            Assert.NotEqual(new DateOnly(1990, 1, 10), dob);
        }

        [Fact]
        public void Validator_ReturnsTrue_ControlNumber()
        {
            string JMBG = "0101990712345";
            bool ok=JmbgValidation.ChecksumValid(JMBG);
            Assert.True(ok);
        }
        [Fact]
        public void Validator_ReturnsFalse_ControlNumber()
        {
            string JMBG = "0101990712349";
            bool ok = JmbgValidation.ChecksumValid(JMBG);
            Assert.False(ok);
        }

        [Fact]
        public void Validator_ReturnsTrue_RegionLooksSerbian()
        {
            string JMBG = "0101990712349";
            bool ok = JmbgValidation.RegionLooksSerbian(JMBG);
            Assert.True(ok);
        }

        [Fact]
        public void Validator_ReturnsFalse_RegionLooksSerbian()
        {
            string JMBG = "0101990102349";
            bool ok = JmbgValidation.RegionLooksSerbian(JMBG);
            Assert.False(ok);
        }

    }
}
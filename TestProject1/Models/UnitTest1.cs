using WebApplication1.Models;

namespace TestProject1.Models
{
    public class LoanProspectTests
    {
        [Fact]
        public void ParseName_WhenStandardNameEntered_ParsesBothNames()
        {
            // arrange
            var input = "test last";
            var toTest = new LoanProspect { Name = input };

            // act
            toTest.ParseName();

            // assert
            Assert.Equal("last", toTest.NameLast);
            Assert.Equal("test", toTest.NameFirst);
        }

        [Fact]
        public void ParseName_WhenSingleNameEntered_SavesAsLastName()
        {
            // arrange
            var input = "test";
            var toTest = new LoanProspect { Name = input };

            // act
            toTest.ParseName();

            // assert
            Assert.Equal(input, toTest.NameLast);
            Assert.True(string.IsNullOrEmpty(toTest.NameFirst));
        }

        [Fact]
        public void ParseName_WhenMultipleNameEntered_ParsesFirstAndLast()
        {
            // arrange
            var input = "test middle name";
            var toTest = new LoanProspect { Name = input };

            // act
            toTest.ParseName();

            // assert
            Assert.Equal("test", toTest.NameFirst);
            Assert.Equal("name", toTest.NameLast);
        }

        [Theory]
        [InlineData("first last", "first", "last")]
        [InlineData("test", "", "test")]
        [InlineData("test middle name", "test", "name")]
        [InlineData("", "", "")]
        [InlineData(null, "", "")]
        [InlineData("test m1, m2, name", "test", "name")]
        [InlineData("last, first", "first", "last")]
        public void ParseName_MultipleScenarios(string input, string expectedFirst, string expectedLast)
        {
            var toTest = new LoanProspect { Name = input };
            toTest.ParseName();
            Assert.Equal(expectedFirst, toTest.NameFirst);
            Assert.Equal(expectedLast, toTest.NameLast);
        }

        [Theory]
        [InlineData(100000,.05, 120, 835.44)]
        [InlineData(0, .24, 360, 0)]
        public void ComputePayment_CanCompute(double principal, double rate, int term, double expected)
        {
            var toTest = new LoanProspect
            {
                LoanAmount = principal,
                TermMonths = term,
                InterestRate = rate,
            };
            toTest.ComputePayment();

            Assert.Equal(expected, toTest.Payment, 2);

        }
    }
}
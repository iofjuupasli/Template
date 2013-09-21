namespace TemplateTest
{
    using Moq;

    using Template;

    using Xunit;
    using Xunit.Extensions;

    public class BracketNotCorrespondingTest
    {
        [Theory]
        [InlineData("only one [%open bracket")]
        [InlineData("only one %]close bracket")]
        [InlineData("open brackeet %]after [% close")]
        [InlineData("duplicated[%close %]bracket %]")]
        [InlineData("[%tduplicated [%open %]bracket")]
        [InlineData("nested[%brackets [%foo %]bar %]")]
        public void WrongBrackets(string templateCode)
        {
            // arrange
            var language = new Mock<IProgrammingLanguage>().Object;

            // act and assert
            Assert.Throws<BracketsNotCorrespondsException>(() =>
                new Template(language, templateCode, null));
        }
    }
}
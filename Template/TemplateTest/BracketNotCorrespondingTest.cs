namespace TemplateTest
{
    using Moq;

    using Template;

    using Xunit;

    public class BracketNotCorrespondingTest
    {
        [Fact]
        public void OneOpenCodeBracket()
        {
            // arrange
            var language = new Mock<IProgrammingLanguage>().Object;
            const string TemplateCode = "text before [%text after";

            // act and assert
            Assert.Throws<BracketsNotCorrespondsException>(() =>
                new Template(language, TemplateCode, null));
        }

        [Fact]
        public void OneCloseCodeBracket()
        {
            // arrange
            var language = new Mock<IProgrammingLanguage>().Object;
            const string TemplateCode = "text before %]text after";

            // act and assert
            Assert.Throws<BracketsNotCorrespondsException>(() =>
                new Template(language, TemplateCode, null));
        }

        [Fact]
        public void OpenBracketAfterClose()
        {
            // arrange
            var language = new Mock<IProgrammingLanguage>().Object;
            const string TemplateCode = "text before %]text [% after";

            // act and assert
            Assert.Throws<BracketsNotCorrespondsException>(() =>
                new Template(language, TemplateCode, null));
        }

        [Fact]
        public void OneOpenTwoClose()
        {
            // arrange
            var language = new Mock<IProgrammingLanguage>().Object;
            const string TemplateCode = "text bef[%ore %]text aft%]er";

            // act and assert
            Assert.Throws<BracketsNotCorrespondsException>(() =>
                new Template(language, TemplateCode, null));
        }

        [Fact]
        public void TwoOpenOneClose()
        {
            // arrange
            var language = new Mock<IProgrammingLanguage>().Object;
            const string TemplateCode = "tex[%t bef[%ore %]text after";

            // act and assert
            Assert.Throws<BracketsNotCorrespondsException>(() =>
                new Template(language, TemplateCode, null));
        }

        [Fact]
        public void NestedBrackets()
        {
            // arrange
            var language = new Mock<IProgrammingLanguage>().Object;
            const string TemplateCode = "tex[%t bef[%ore %]text a%]fter";

            // act and assert
            Assert.Throws<BracketsNotCorrespondsException>(() =>
                new Template(language, TemplateCode, null));
        }
    }
}
namespace TemplateTest
{
    using System;
    using System.IO;

    using Moq;

    using Template;

    using Xunit;

    public class TemplateTest
    {
        [Fact]
        public void PlainText_AlwaysRenderPlainText()
        {
            // arrange
            var language = new Mock<IProgrammingLanguage>().Object;
            const string TemplateCode = "abcd";
            var usings = new string[0];
            using (var template = new Template(language, TemplateCode, usings))
            using (var output = new StringWriter())
            {
                // act
                template.Render(output);

                // assert
                Assert.Equal(TemplateCode, output.ToString());
            }
        }

        [Fact]
        public void EmptyTemplateOnNullLanguage_RenderNothing()
        {
            // arrange
            using (var template = new Template(null, String.Empty, null))
            using (var output = new StringWriter())
            {
                // act
                template.Render(output);

                // assert
                Assert.Empty(output.ToString());
            }
        }

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
}

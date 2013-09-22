namespace CSharpTest
{
    using System;
    using System.IO;

    using CSharp;

    using Template;

    using Xunit;

    public class CSharpTest
    {
        [Fact]
        public void TextOutput()
        {
            // arrange
            var language = new CSharp();
            const string TemplateCode = "text[%%]";

            // act
            using (var template = new Template(language, TemplateCode, null))
            using (var output = new StringWriter())
            {
                template.Render(output);

                // assert
                Assert.Equal("text", output.ToString());
            }
        }

        [Fact]
        public void ErrorHandling()
        {
            // arrange
            var language = new CSharp();
            const string TemplateCode = "text[%error%]";

            var exception = Assert.Throws<CompileErrorException>(() => 
                    new Template(language, TemplateCode, null));
        }
    }
}

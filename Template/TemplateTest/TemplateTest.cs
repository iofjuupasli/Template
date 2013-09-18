namespace TemplateTest
{
    using System;
    using System.IO;

    using Moq;

    using Xunit;

    public class TemplateTest
    {
        [Fact]
        public void PlainText_AlwaysRenderPlainText()
        {
            // arrange
            IProgrammingLanguage language = new Mock<IProgrammingLanguage>().Object;
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
    }

    public class Template : IDisposable
    {
        private IScript script;

        public Template(IProgrammingLanguage language, string templateCode, string[] usings)
        {
            if (!templateCode.Contains("[%"))
            {
                this.script = new PlainTextOutputScript(templateCode);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            script.Dispose();
        }

        public void Render(TextWriter output)
        {
            script.Run(output);
        }
    }

    public class PlainTextOutputScript : IScript
    {
        private readonly string text;

        public PlainTextOutputScript(string text)
        {
            this.text = text;
        }

        public void Run(TextWriter output)
        {
            output.Write(this.text);
        }

        public void Dispose()
        {
        }
    }

    internal interface IScript : IDisposable
    {
        void Run(TextWriter output);
    }

    public interface IProgrammingLanguage
    {
    }
}

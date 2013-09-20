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

        [Fact]
        public void TemplateWithCodeBlock_InterpretedAsCode()
        {
            // arrange
            const string Code = "code";
            var templateText = string.Format("[%{0}%]", Code);
            var codeBuilderMock = new Mock<ICodeBuilder>(MockBehavior.Strict);
            const string ProgramStructure = "begin {0} end";
            codeBuilderMock.Setup(builder => builder.CoverAsProgram(It.IsAny<string>()))
                           .Returns<string>(templateCode => String.Format(ProgramStructure, templateCode));

            var echoLanguageMock = new Mock<IProgrammingLanguage>(MockBehavior.Strict);
            echoLanguageMock.Setup(language => language.GetCodeBuilder())
                            .Returns(codeBuilderMock.Object);

            echoLanguageMock.Setup(language => language.Compile(It.IsAny<string>()))
                            .Returns<string>(programmCode => new PlainTextOutputScript(programmCode));

            // act
            using (var template = new Template(echoLanguageMock.Object, templateText, null))
            using (var output = new StringWriter())
            {    
                template.Render(output);

                // assert
                Assert.Equal(String.Format(ProgramStructure, Code), output.ToString());
            }
        }

        [Fact]
        public void LanguageNull_TemplateDependOnLanguage_ThrowException()
        {
            // arrange
            const string TemplateCode = "[%code%]";

            // act
            var exception = Assert.Throws<ArgumentNullException>(() =>  
                    new Template(null, TemplateCode, null));

            // assert
            Assert.Equal("language", exception.ParamName);   
        }
    }
}

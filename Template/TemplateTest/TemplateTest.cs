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
            const string ProgramStructure = "begin {0} end";
            const string MethodStructure = "method{{ {0} }}";
            var codeBuilderMock = new Mock<ICodeBuilder>(MockBehavior.Strict);
            codeBuilderMock.Setup(builder => builder.WrapAsProgram(It.IsAny<string>()))
                           .Returns<string>(templateCode => String.Format(ProgramStructure, templateCode));

            codeBuilderMock.Setup<string>(builder => builder.WrapAsMethod(It.IsAny<string>()))
                           .Returns<string>(methodBody => String.Format(MethodStructure, methodBody));

            var echoLanguageMock = new Mock<IProgrammingLanguage>(MockBehavior.Strict);
            echoLanguageMock.Setup(language => language.GetCodeBuilder())
                            .Returns(codeBuilderMock.Object);

            echoLanguageMock.Setup(language => language.Compile(It.IsAny<string>()))
                            .Returns<string>(programmCode => new PlainTextOutputScript(programmCode));
            
            const string Code = "code";
            var templateText = string.Format("[%{0}%]", Code);

            // act
            using (var template = new Template(echoLanguageMock.Object, templateText, null))
            using (var output = new StringWriter())
            {    
                template.Render(output);

                // assert
                var expectedCode = String.Format(ProgramStructure, String.Format(MethodStructure, Code));
                Assert.Equal(expectedCode, output.ToString());
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

        [Fact]
        public void TextBeforeCode_ShouldBeInterpretedAsTextToOutput()
        {
            // arrange
            const string ProgramStructure = "begin {0} end";
            const string OutputStatementStructure = @"output(""{0}"");";
            const string MethodStructure = "method{{ {0} }}";

            var codeBuilderMock = new Mock<ICodeBuilder>(MockBehavior.Strict);
            codeBuilderMock.Setup(builder => builder.WrapAsProgram(It.IsAny<string>()))
                           .Returns<string>(templateCode => String.Format(ProgramStructure, templateCode));
            
            codeBuilderMock.Setup(builder => builder.WrapAsPlainTextOutputStatement(It.IsAny<string>()))
                           .Returns<string>(textToOutput => String.Format(OutputStatementStructure, textToOutput));

            codeBuilderMock.Setup<string>(builder => builder.WrapAsMethod(It.IsAny<string>()))
                           .Returns<string>(methodBody => String.Format(MethodStructure, methodBody));

            var echoLanguageMock = new Mock<IProgrammingLanguage>(MockBehavior.Strict);
            echoLanguageMock.Setup(language => language.GetCodeBuilder())
                            .Returns(codeBuilderMock.Object);

            echoLanguageMock.Setup(language => language.Compile(It.IsAny<string>()))
                            .Returns<string>(programmCode => new PlainTextOutputScript(programmCode));

            const string PlainText = "text";
            const string Code = "code";
            var templateText = String.Format("{0}[%{1}%]", PlainText, Code);

            // act
            using (var template = new Template(echoLanguageMock.Object, templateText, null))
            using (var output = new StringWriter())
            {
                template.Render(output);

                // assert
                var body = String.Format(OutputStatementStructure, PlainText) + Code;
                var method = String.Format(MethodStructure, body);
                var code = String.Format(ProgramStructure, method);
                Assert.Equal(code, output.ToString());
            }
        }
    }
}

namespace TemplateTest
{
    using System;
    using System.ComponentModel;
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
            const string TemplateCode = "abcd";
            using (var template = new Template(null, TemplateCode, null))
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

            var echoLanguage = new MockLanguageBuilder()
                    .WithProgramWrapper(ProgramStructure)
                    .WithMethodWrapper(MethodStructure)
                    .OutputSelfCode()
                    .GetObject();

            const string Code = "code";
            var templateText = string.Format("[%{0}%]", Code);

            // act
            using (var template = new Template(echoLanguage, templateText, null))
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

            var echoLanguage = new MockLanguageBuilder()
                    .WithProgramWrapper(ProgramStructure)
                    .WithMethodWrapper(MethodStructure)
                    .WithPlainTextWrapper(OutputStatementStructure)
                    .OutputSelfCode()
                    .GetObject();

            const string PlainText = "text";
            const string Code = "code";
            var templateText = String.Format("{0}[%{1}%]", PlainText, Code);

            // act
            using (var template = new Template(echoLanguage, templateText, null))
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

        private class MockLanguageBuilder
        {
            private readonly Mock<IProgrammingLanguage> languageMock;

            private readonly Mock<ICodeBuilder> codeBuilderMock;

            public MockLanguageBuilder()
            {
                this.codeBuilderMock = new Mock<ICodeBuilder>(MockBehavior.Strict);
                this.languageMock = new Mock<IProgrammingLanguage>(MockBehavior.Strict);
                this.languageMock.Setup(language => language.GetCodeBuilder())
                                 .Returns(this.codeBuilderMock.Object);
            }

            public MockLanguageBuilder WithProgramWrapper(string programStructure)
            {
                this.codeBuilderMock.Setup(builder => builder.WrapAsProgram(It.IsAny<string>()))
                               .Returns<string>(templateCode => String.Format(programStructure, templateCode));
                return this;
            }

            public MockLanguageBuilder WithMethodWrapper(string methodStructure)
            {
                this.codeBuilderMock.Setup<string>(builder => builder.WrapAsMethod(It.IsAny<string>()))
                               .Returns<string>(methodBody => String.Format(methodStructure, methodBody));
                return this;
            }

            public MockLanguageBuilder WithPlainTextWrapper(string outputStatementStructure)
            {
                this.codeBuilderMock.Setup(builder => builder.WrapAsPlainTextOutputStatement(It.IsAny<string>()))
                           .Returns<string>(textToOutput => String.Format(outputStatementStructure, textToOutput));
                return this;
            }

            public MockLanguageBuilder OutputSelfCode()
            {
                this.languageMock.Setup(language => language.Compile(It.IsAny<string>()))
                                .Returns<string>(programmCode => new PlainTextOutputScript(programmCode));
                return this;
            }

            public IProgrammingLanguage GetObject()
            {
                return this.languageMock.Object;
            }
        }
    }
}

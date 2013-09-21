namespace TemplateTest
{
    using System;
    using System.ComponentModel;
    using System.IO;

    using Moq;

    using Template;

    using Xunit;
    using Xunit.Extensions;

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

        [Theory]
        [InlineData("", "", "", "", "")]
        [InlineData("text1", "code1", "text2", "code2", "text3")]
        [InlineData("text1", "code1", "text2", "code2", "")]
        [InlineData("text1", "code1", "", "code2", "text3")]
        [InlineData("", "code1", "text2", "code2", "text3")]
        public void PlainText_ShouldBeInterpretedAsTextToOutput(
                string textBeforeCode, string firstCode, string textBw, string secondCode, string textAfterCode)
        {
            // arrange
            var templateText = String.Format("{0}[%{1}%]{2}[%{3}%]{4}",
                    textBeforeCode, firstCode, textBw, secondCode, textAfterCode);

            const string ProgramStructure = "begin {0} end";
            const string OutputStatementStructure = @"output(""{0}"");";
            const string MethodStructure = "method{{ {0} }}";

            var echoLanguage = new MockLanguageBuilder()
                    .WithProgramWrapper(ProgramStructure)
                    .WithMethodWrapper(MethodStructure)
                    .WithPlainTextWrapper(OutputStatementStructure)
                    .OutputSelfCode()
                    .GetObject();

            // act
            using (var template = new Template(echoLanguage, templateText, null))
            using (var output = new StringWriter())
            {
                template.Render(output);

                // assert
                var body = (String.IsNullOrEmpty(textBeforeCode) ? "" : String.Format(OutputStatementStructure, textBeforeCode))
                        + firstCode
                        + (String.IsNullOrEmpty(textBw) ? "" : String.Format(OutputStatementStructure, textBw))
                        + secondCode
                        + (String.IsNullOrEmpty(textAfterCode) ? "" : String.Format(OutputStatementStructure, textAfterCode));

                var method = String.Format(MethodStructure, body);
                var code = String.Format(ProgramStructure, method);

                Assert.Equal(code, output.ToString());
            }
        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData("text", "expression", "text2")]
        [InlineData("", "expression", "")]
        [InlineData("text", "", "text2")]
        public void ExpressionToOutput(
                string textBeforeCode, string expressionToOutput, string textAfterCode)
        {
            // arrange
            var templateText = String.Format("{0}[%={1}%]{2}",
                    textBeforeCode, expressionToOutput, textAfterCode);

            const string ProgramStructure = "begin {0} end";
            const string ExpressionOutputStructure = "output({0});";
            const string OutputStatementStructure = @"output(""{0}"");";
            const string MethodStructure = "method{{ {0} }}";

            var echoLanguage = new MockLanguageBuilder()
                    .WithProgramWrapper(ProgramStructure)
                    .WithMethodWrapper(MethodStructure)
                    .WithPlainTextWrapper(OutputStatementStructure)
                    .WithExpressionOutputWrapper(ExpressionOutputStructure)
                    .OutputSelfCode()
                    .GetObject();

            // act
            using (var template = new Template(echoLanguage, templateText, null))
            using (var output = new StringWriter())
            {
                template.Render(output);

                // assert
                var body = (String.IsNullOrEmpty(textBeforeCode) ? "" : String.Format(OutputStatementStructure, textBeforeCode))
                        + (String.IsNullOrEmpty(expressionToOutput) ? "" : String.Format(ExpressionOutputStructure, expressionToOutput))
                        + (String.IsNullOrEmpty(textAfterCode) ? "" : String.Format(OutputStatementStructure, textAfterCode));

                var method = String.Format(MethodStructure, body);
                var code = String.Format(ProgramStructure, method);

                Assert.Equal(code, output.ToString());
            }
        }

        [Theory]
        [InlineData("", "", "", "")]
        [InlineData("before", "expression", "toRepeat", "after")]
        [InlineData("before", "", "toRepeat", "after")]
        [InlineData("before", "expression", "", "after")]
        public void RepeatExpression(
                string textBefore, string repeatCountExpression, string textToRepeat, string textAfter)
        {
            // arrange
            var templateText = String.Format(@"{0}[%@{1}%]{2}[%@%]{3}",
                    textBefore, repeatCountExpression, textToRepeat, textAfter);

            const string ProgramStructure = "begin {0} end";
            const string RepeatStructure = "for(var i = 0; i < {0}; i++){{{1}}}";
            const string OutputStatementStructure = @"output(""{0}"");";
            const string MethodStructure = "method{{ {0} }}";

            var echoLanguage = new MockLanguageBuilder()
                    .WithProgramWrapper(ProgramStructure)
                    .WithMethodWrapper(MethodStructure)
                    .WithPlainTextWrapper(OutputStatementStructure)
                    .WithRepeatWrapper(RepeatStructure)
                    .OutputSelfCode()
                    .GetObject();

            // act
            using (var template = new Template(echoLanguage, templateText, null))
            using (var output = new StringWriter())
            {
                template.Render(output);
                var toRepeat = (String.IsNullOrEmpty(textToRepeat) ? "" : String.Format(OutputStatementStructure, textToRepeat));
                // assert
                var body = (String.IsNullOrEmpty(textBefore) ? "" : String.Format(OutputStatementStructure, textBefore))
                        + (String.IsNullOrEmpty(repeatCountExpression) || String.IsNullOrEmpty(toRepeat) ? "" : String.Format(RepeatStructure, repeatCountExpression, toRepeat))
                        + (String.IsNullOrEmpty(textAfter) ? "" : String.Format(OutputStatementStructure, textAfter));

                var method = String.Format(MethodStructure, body);
                var code = String.Format(ProgramStructure, method);

                Assert.Equal(code, output.ToString());
            }
        }

        [Theory]
        [InlineData("", "", "", "")]
        [InlineData("before", "expression", "on true", "after")]
        [InlineData("before", "", "on true", "after")]
        [InlineData("before", "expression", "", "after")]
        public void ConditionExpression(
                string textBefore, string conditionExpression, string textOnTrue, string textAfter)
        {
            // arrange
            var templateText = String.Format(@"{0}[%?{1}%]{2}[%?%]{3}",
                    textBefore, conditionExpression, textOnTrue, textAfter);

            const string ProgramStructure = "begin {0} end";
            const string ConditionStructure = "if({0}){{{1}}}";
            const string OutputStatementStructure = @"output(""{0}"");";
            const string MethodStructure = "method{{ {0} }}";

            var echoLanguage = new MockLanguageBuilder()
                    .WithProgramWrapper(ProgramStructure)
                    .WithMethodWrapper(MethodStructure)
                    .WithPlainTextWrapper(OutputStatementStructure)
                    .WithConditionWrapper(ConditionStructure)
                    .OutputSelfCode()
                    .GetObject();

            // act
            using (var template = new Template(echoLanguage, templateText, null))
            using (var output = new StringWriter())
            {
                template.Render(output);
                var onTrue = (String.IsNullOrEmpty(textOnTrue) ? "" : String.Format(OutputStatementStructure, textOnTrue));
                // assert
                var body = (String.IsNullOrEmpty(textBefore) ? "" : String.Format(OutputStatementStructure, textBefore))
                        + (String.IsNullOrEmpty(conditionExpression) || String.IsNullOrEmpty(onTrue) ? "" : String.Format(ConditionStructure, conditionExpression, onTrue))
                        + (String.IsNullOrEmpty(textAfter) ? "" : String.Format(OutputStatementStructure, textAfter));

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


            public MockLanguageBuilder WithExpressionOutputWrapper(string expressionOutputStructure)
            {
                this.codeBuilderMock.Setup(builder => builder.WrapAsExpressionOutput(It.IsAny<string>()))
                        .Returns<string>(expression => String.Format(expressionOutputStructure, expression));
                return this;
            }

            public MockLanguageBuilder WithRepeatWrapper(string repeatStructure)
            {
                this.codeBuilderMock.Setup(
                    builder => builder.WrapAsRepeatExpression(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string>(
                        (expression, toRepeat) => String.Format(repeatStructure, expression, toRepeat));
                return this;
            }

            public MockLanguageBuilder WithConditionWrapper(string conditionStructure)
            {
                this.codeBuilderMock.Setup(
                    builder => builder.WrapAsConditionExpression(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string>(
                        (condition, onTrue) => String.Format(conditionStructure, condition, onTrue));
                return this;
            }

            public IProgrammingLanguage GetObject()
            {
                return this.languageMock.Object;
            }
        }
    }
}

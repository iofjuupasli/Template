namespace TemplateTest
{
    using System;
    using System.ComponentModel;
    using System.IO;

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

        [Fact]
        public void RepeatExpression()
        {
            const string TextBefore = "before";
            const string RepeatCountExpression = "expression";
            const string TextToRepeat = "toRepeat";
            const string TextAfter = "after";
            // arrange
            var templateText = String.Format(@"{0}[%@{1}%]{2}[%@%]{3}",
                    TextBefore, RepeatCountExpression, TextToRepeat, TextAfter);

            const string ProgramStructure = "begin {0} end";
            const string OpenRepeatStructure = "for(var i = 0; i < {0}; i++){{";
            const string CloseRepeatStructure = "}";
            const string OutputStatementStructure = @"output(""{0}"");";
            const string MethodStructure = "method{{ {0} }}";

            var echoLanguage = new MockLanguageBuilder()
                    .WithProgramWrapper(ProgramStructure)
                    .WithMethodWrapper(MethodStructure)
                    .WithPlainTextWrapper(OutputStatementStructure)
                    .WithRepeatWrapper(OpenRepeatStructure, CloseRepeatStructure)
                    .OutputSelfCode()
                    .GetObject();

            // act
            using (var template = new Template(echoLanguage, templateText, null))
            using (var output = new StringWriter())
            {
                template.Render(output);
                
                // assert
                const string Expected = "begin method{ output(\"before\");for(var i = 0; i < expression; i++){output(\"toRepeat\");}output(\"after\"); } end";
                Assert.Equal(Expected, output.ToString());
            }
        }

        [Fact]
        public void ConditionExpression()
        {
            const string TextBefore = "before";
            const string ConditionExpression = "expression";
            const string TextOnTrue = "on true";
            const string TextAfter = "after";
            // arrange
            var templateText = String.Format(@"{0}[%?{1}%]{2}[%?%]{3}",
                    TextBefore, ConditionExpression, TextOnTrue, TextAfter);

            const string ProgramStructure = "begin {0} end";
            const string OpenCoditionStructure = "if({0}){{";
            const string CloseConditionStructure = "}";
            const string OutputStatementStructure = @"output(""{0}"");";
            const string MethodStructure = "method{{ {0} }}";

            var echoLanguage = new MockLanguageBuilder()
                    .WithProgramWrapper(ProgramStructure)
                    .WithMethodWrapper(MethodStructure)
                    .WithPlainTextWrapper(OutputStatementStructure)
                    .WithConditionWrapper(OpenCoditionStructure, CloseConditionStructure)
                    .OutputSelfCode()
                    .GetObject();

            // act
            using (var template = new Template(echoLanguage, templateText, null))
            using (var output = new StringWriter())
            {
                template.Render(output);
                
                // assert
                const string Expected = "begin method{ output(\"before\");if(expression){output(\"on true\");}output(\"after\"); } end";
                Assert.Equal(Expected, output.ToString());
            }
        }

        [Fact]
        public void UsingStatements()
        {
            // arrange
            var langugage = new EchoLanguageStub();
            var templateText = "text[%code;%]";
            var usings = new[] { "System", "My.Program.Using" };

            // act
            using (var template = new Template(langugage, templateText, usings))
            using (var output = new StringWriter())
            {
                template.Render(output);

                // assert
                var expected = "begin using System; using My.Program.Using; method(MyOutput output){ output(\"text\");code; } end";
                Assert.Equal(expected, output.ToString());
            }
        }
    }
}

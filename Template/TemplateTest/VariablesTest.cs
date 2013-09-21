namespace TemplateTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;

    using Moq;

    using Template;

    using Xunit;
    using Xunit.Extensions;

    public class VariablesTest
    {
        [Theory]
        [InlineData(ArgumentType.Integer, 42)]
        [InlineData(ArgumentType.String, "text")]
        [InlineData(ArgumentType.Long, 117)]
        [InlineData(ArgumentType.Boolean, true)]
        [InlineData(ArgumentType.Double, 3.1415)]
        [InlineData(ArgumentType.DateTime, "2010-01-01")]
        public void TemplateProcessVariable(ArgumentType type, object value)
        {
            // arrange
            var variable = new Variable("varName", type);
            var templateText = "[%code;%]text";

            const string ProgramStructure = "begin {0} end";
            const string ExpressionOutputStructure = "output({0});";
            const string OutputStatementStructure = @"output(""{0}"");";
            const string MethodStructure = "method({1}){{ {0} }}";
            var typeMapping = new Dictionary<ArgumentType, string>
                    {
                        { type, "anyType" }
                    };

            var echoLanguage = new MockLanguageBuilder()
                    .WithProgramWrapper(ProgramStructure)
                    .WithMethodWrapper(MethodStructure)
                    .WithPlainTextWrapper(OutputStatementStructure)
                    .WithTypeMapping(typeMapping)
                    .OutputSelfCode()
                    .GetObject();

            // act
            using (var template = new Template(echoLanguage, templateText, null, variable))
            using (var output = new StringWriter())
            {
                template.Render(output);

                // assert
                var expected = "begin method(anyType varName){ code;output(\"text\"); } end";
                Assert.Equal(expected, output.ToString());
                
            }
        }
    }
}

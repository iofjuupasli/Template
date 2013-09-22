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

            var echoLanguage = new EchoLanguageStub();

            // act
            using (var template = new Template(echoLanguage, templateText, null, variable))
            using (var output = new StringWriter())
            {
                template.Render(output, value);

                // assert
                var expected = "begin method(" + echoLanguage.GetCodeBuilder().ConvertType(type) + " varName){ code;output(\"text\"); } end";
                Assert.Equal(expected, output.ToString());
                
            }
        }
    }
}

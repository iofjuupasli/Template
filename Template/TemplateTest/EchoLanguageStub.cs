using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateTest
{
    using Template;

    internal class EchoLanguageStub : IProgrammingLanguage
    {
        public ICodeBuilder GetCodeBuilder()
        {
            return new SimpleCodeBuilderStub();
        }

        public IScript Compile(string code)
        {
            return new PlainTextOutputScript(code);
        }
    }

    internal class SimpleCodeBuilderStub : ICodeBuilder
    {
        public string WrapAsProgram(string method, string[] usings)
        {
            var usingsString = usings == null
                ? String.Empty
                : String.Concat(usings.Select(@using => String.Format("using {0}; ", @using)));
            return String.Format("begin {0}{1} end", usingsString, method);
        }

        public string WrapAsPlainTextOutputStatement(string text, string outputVariableName)
        {
            return String.Format("{0}(\"{1}\");", outputVariableName, text);
        }

        public string WrapAsMethod(string methodBody, params Variable[] variables)
        {
            return String.Format(
                "method({0}){{ {1} }}",
                String.Join(
                    ", ",
                    variables.Select(
                        variable => String.Format("{0} {1}", this.ConvertType(variable.Type), variable.Name))),
                methodBody);
        }

        public string OpenRepeatExpression(string repeatCountExpression)
        {
            return String.Format("for(var i = 0; i < {0}; i++){{", repeatCountExpression);
        }

        public string CloseRepeatExpression()
        {
            return "}";
        }

        public string OpenConditionExpression(string conditionExpression)
        {
            return String.Format("if({0}){{", conditionExpression);
        }

        public string CloseConditionExpression()
        {
            return "}";
        }

        public string WrapAsExpressionOutput(string expression, string outputVariableName)
        {
            return String.Format("{0}({1});", outputVariableName, expression);
        }
        
        public string ConvertType(ArgumentType type)
        {
            return "My" + type;
        }
    }
}

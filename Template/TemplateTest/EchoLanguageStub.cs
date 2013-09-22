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
        public string WrapAsProgram(string method)
        {
            return String.Format("begin {0} end", method);
        }

        public string WrapAsPlainTextOutputStatement(string text)
        {
            return String.Format("output(\"{0}\");", text);
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

        public string WrapAsExpressionOutput(string expression)
        {
            return String.Format("output({0});", expression);
        }

        public string WrapAsRepeatExpression(string expression, string codeToRepeat)
        {
            return String.Format("for(var i = 0; i < {0}; i++){{{1}}}", expression, codeToRepeat);
        }

        public string WrapAsConditionExpression(string condition, string codeOnTrue)
        {
            return String.Format("if({0}){{{1}}}", condition, codeOnTrue);
        }

        public string ConvertType(ArgumentType type)
        {
            return "My" + type;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp
{
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Reflection;
    using System.Xml.Xsl;

    using Microsoft.CSharp;
    using Microsoft.CSharp.RuntimeBinder;

    using Template;

    public class CSharp : IProgrammingLanguage
    {
        private const string ClassName = "MyClass";

        private const string MethodName = "Run";

        public ICodeBuilder GetCodeBuilder()
        {
            return new CSharpCodeBuilder();
        }

        public IScript Compile(string code)
        {
            var codeProvider = new CSharpCodeProvider();
            var assemblies = new[] { "System.dll", "System.Core.dll" };
            var compilerParameters = new CompilerParameters(assemblies);
            var compiled = codeProvider.CompileAssemblyFromSource(compilerParameters, code);
            var errors = compiled.Errors;
            if (errors.HasErrors)
            {
                var errorsString = String.Join(
                    Environment.NewLine,
                    errors.Cast<CompilerError>()
                        .Select(error => String.Format(
                                    "{0} line: {1} column: {2}: \"{3}\"",
                                    error.ErrorNumber,
                                    error.Line,
                                    error.Column,
                                    error.ErrorText)));

                throw new CompileErrorException(errorsString);
            }

            var assembly = compiled.CompiledAssembly;
            var type = assembly.GetType(ClassName);
            var method = type.GetMethod(MethodName);
            return new CSharpScript(method);
        }

        private class CSharpScript : IScript
        {
            private readonly MethodInfo method;

            public CSharpScript(MethodInfo method)
            {
                this.method = method;
            }

            public void Run(params object[] values)
            {
                method.Invoke(null, values);
            }

            public void Dispose()
            {
            }
        }

        private class CSharpCodeBuilder : ICodeBuilder
        {
            public string WrapAsProgram(string method, string[] usings)
            {
                return String.Format(@"
{0}

public static class " + ClassName + @"
{{
    {1}
}}", usings == null
                            ? String.Empty
                            : String.Concat(usings.Select(u => String.Format("using {0};" + Environment.NewLine, u))),
                    method);
            }

            public string WrapAsPlainTextOutputStatement(string text, string outputVariableName)
            {
                text = String.Concat(text.Select(c => String.Format(@"\u{0:x4}", (int)c)));
                return String.Format("{0}.Write(\"{1}\");", outputVariableName, text);
            }

            public string WrapAsMethod(string methodBody, params Variable[] variables)
            {
                return String.Format(@"
    public static void " + MethodName + @"({0})
    {{
        {1}
    }}", String.Join(", ", variables.Select(variable => String.Format("{0} {1}", this.ConvertType(variable.Type), variable.Name))),
                    methodBody);
            }

            public string WrapAsExpressionOutput(string expression, string outputVariableName)
            {
                return String.Format("{0}.Write({1});",outputVariableName, expression);
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

            public string ConvertType(ArgumentType type)
            {
                switch (type)
                {
                    case ArgumentType.Boolean:
                        return "System.Boolean";
                    case ArgumentType.DateTime:
                        return "System.DateTime";
                    case ArgumentType.Double:
                        return "System.Double";
                    case ArgumentType.Integer:
                        return "System.Int32";
                    case ArgumentType.Long:
                        return "System.Int64";
                    case ArgumentType.Output:
                        return "System.IO.TextWriter";
                    case ArgumentType.String:
                        return "System.String";
                }

                return null;
            }
        }
    }
}

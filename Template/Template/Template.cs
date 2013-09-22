namespace Template
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public class Template : IDisposable
    {
        private const string CodeExpressionOpenBracket = "[%";
        private const string CodeExpressionCloseBracket = "%]";

        private readonly IScript script;

        public Template(IProgrammingLanguage language, string templateCode, string[] usings, params Variable[] variables)
        {
            if (!IsBracketsCorresponding(templateCode))
            {
                throw new BracketsNotCorrespondsException();
            }

            if (IsTemplateCodeLanguageIndependent(templateCode))
            {
                this.script = new PlainTextOutputScript(templateCode);
                return;
            }

            if (language == null)
            {
                throw new ArgumentNullException("language");
            }

            {
                var vars = variables.ToList();
                vars.Add(new Variable("output", ArgumentType.Output));
                variables = vars.ToArray();
            }

            var code = new TemplateCodeBuilder(language.GetCodeBuilder(), templateCode, usings, variables).BuildCode();
            this.script = language.Compile(code);
        }

        private class TemplateCodeBuilder
        {
            private readonly ICodeBuilder codeBuilder;

            private string templateCode;

            private readonly string[] usings;

            private readonly Variable[] variables;

            public TemplateCodeBuilder(ICodeBuilder codeBuilder, string templateCode, string[] usings, params Variable[] variables)
            {
                this.codeBuilder = codeBuilder;
                this.templateCode = templateCode;
                this.usings = usings;
                this.variables = variables;
            }

            public override string ToString()
            {
                return this.BuildCode();
            }

            public string BuildCode()
            {
                ProcessTextOutputs();
                ProcessExpressionOutputs();
                ProcessRepeatExpressions();
                ProcessConditionExpressions();

                RemoveCodeBrackets();
                AsMethod();
                AsProgram();

                return this.templateCode;
            }

            private void ProcessTextOutputs()
            {
                this.templateCode = Regex.Replace(
                    this.templateCode,
                    @"(?:(?<=\A).*?(?=\[%))|(?:(?<=%\]).*?(?=\[%))|(?:(?!.*%\])(?!\]).*(?=\Z))",
                    match => String.IsNullOrEmpty(match.Value) 
                        ? String.Empty
                        : String.Format("[%{0}%]", this.codeBuilder.WrapAsPlainTextOutputStatement(match.Value)),
                    RegexOptions.Singleline);
            }

            private void ProcessExpressionOutputs()
            {
                this.templateCode = Regex.Replace(
                    this.templateCode,
                    @"\[%=(.*?)%\]",
                    match => String.IsNullOrEmpty(match.Groups[1].Value) 
                        ? String.Empty 
                        : codeBuilder.WrapAsExpressionOutput(match.Groups[1].Value),
                    RegexOptions.Singleline);
            }

            private void ProcessRepeatExpressions()
            {
                this.templateCode = Regex.Replace(
                    this.templateCode,
                    @"\[%\@(?!%)(.*?)%\]",
                    match => this.codeBuilder.OpenRepeatExpression(match.Groups[1].Value),
                    RegexOptions.Singleline);

                this.templateCode = Regex.Replace(
                    this.templateCode,
                    @"\[%\@%\]",
                    match => this.codeBuilder.CloseRepeatExpression(),
                    RegexOptions.Singleline);
            }

            private void ProcessConditionExpressions()
            {
                this.templateCode = Regex.Replace(
                    this.templateCode,
                    @"\[%\?(?!%)(.*?)%\]",
                    match => this.codeBuilder.OpenConditionExpression(match.Groups[1].Value),
                    RegexOptions.Singleline);

                this.templateCode = Regex.Replace(
                    this.templateCode,
                    @"\[%\?%\]",
                    match => this.codeBuilder.CloseConditionExpression(),
                    RegexOptions.Singleline);
            }

            private void RemoveCodeBrackets()
            {
                this.templateCode = this.templateCode.Replace("[%", String.Empty).Replace("%]", String.Empty);
            }

            private void AsMethod()
            {
                this.templateCode = this.codeBuilder.WrapAsMethod(this.templateCode, variables);
            }

            private void AsProgram()
            {
                this.templateCode = this.codeBuilder.WrapAsProgram(this.templateCode, usings);
            }
        }
/*
        private static string BuildCode(string code, ICodeBuilder codeBuilder, string[] usings, params Variable[] variables)
        {
            code = ProcessTextOutputs(code, codeBuilder.WrapAsPlainTextOutputStatement);
            code = ProcessExpressionOutputs(code, codeBuilder);
            code = ProcessRepeatExpressions(code, codeBuilder);
            code = ProcessConditionExpressions(code, codeBuilder);
            code = ProcessCodeBlocks(code);
            code = codeBuilder.WrapAsMethod(code, variables);
            code = codeBuilder.WrapAsProgram(code, usings);
            return code;
        }

        private static string ProcessConditionExpressions(string code, ICodeBuilder codeBuilder)
        {
            return Regex.Replace(
                code,
                @"\[%\?(.*?)%](.*?)\[%\?%\]",
                match =>
                    String.IsNullOrEmpty(match.Groups[1].Value) || String.IsNullOrEmpty(match.Groups[2].Value)
                        ? String.Empty
                        : conditionWrapper(match.Groups[1].Value, match.Groups[2].Value),
                RegexOptions.Singleline);
        }

        private static string ProcessRepeatExpressions(string code, ICodeBuilder codeBuilder)
        {
            return Regex.Replace(
                code,
                @"\[%@(.*?)%](.*?)\[%@%\]",
                match =>
                    String.IsNullOrEmpty(match.Groups[1].Value) || String.IsNullOrEmpty(match.Groups[2].Value)
                        ? String.Empty
                        : repeatExpressionWrapper(match.Groups[1].Value, match.Groups[2].Value),
                RegexOptions.Singleline);
        }

        private static string ProcessExpressionOutputs(string code, ICodeBuilder codeBuilder)
        {
            return Regex.Replace(
                code,
                @"\[%=(.*?)%\]",
                match => String.IsNullOrEmpty(match.Groups[1].Value) ? String.Empty : codeBuilder.WrapAsExpressionOutput(match.Groups[1].Value),
                RegexOptions.Singleline);
        }

        private static string ProcessCodeBlocks(string code)
        {
            return code.Replace("[%", string.Empty).Replace("%]", string.Empty);
        }

        private static string ProcessTextOutputs(string code, Func<string, string> wrapper)
        {
            return Regex.Replace(
                code,
                @"(?:(?<=\A).*?(?=\[%))|(?:(?<=%\]).*?(?=\[%))|(?:(?!.*%\])(?!\]).*(?=\Z))",
                match => String.IsNullOrEmpty(match.Value) ? String.Empty : wrapper(match.Value),
                RegexOptions.Singleline);
        }*/

        private static bool IsBracketsCorresponding(string templateCode)
        {
            var openBrackets = Regex.Matches(
                templateCode,
                Regex.Escape(CodeExpressionOpenBracket),
                RegexOptions.Singleline);

            var closeBrackets = Regex.Matches(
                templateCode,
                Regex.Escape(CodeExpressionCloseBracket),
                RegexOptions.Singleline);

            if (openBrackets.Count != closeBrackets.Count)
            {
                return false;
            }

            var allBrackets = openBrackets.Cast<Match>()
                    .Concat(closeBrackets.Cast<Match>())
                    .OrderBy(match => match.Index);

            for (var bracketIndex = 0; bracketIndex < allBrackets.Count(); bracketIndex++)
            {
                var bracket = allBrackets.ElementAt(bracketIndex).Value;
                var correctBracket = bracketIndex % 2 == 0
                        ? CodeExpressionOpenBracket
                        : CodeExpressionCloseBracket;

                if (bracket != correctBracket)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsTemplateCodeLanguageIndependent(string templateCode)
        {
            return !templateCode.Contains(CodeExpressionOpenBracket);
        }

        public void Dispose()
        {
            this.script.Dispose();
        }

        public void Render(TextWriter output, params object[] values)
        {
            this.script.Run(values.Concat(new[] { output }).ToArray());
        }
    }
}
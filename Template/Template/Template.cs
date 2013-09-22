namespace Template
{
    using System;
    using System.IO;
    using System.Linq;
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

            var code = BuildCode(templateCode, language.GetCodeBuilder(), usings, variables);
            this.script = language.Compile(code);
        }

        private static string BuildCode(string code, ICodeBuilder codeBuilder, string[] usings, params Variable[] variables)
        {
            code = ProcessTextOutputs(code, codeBuilder.WrapAsPlainTextOutputStatement);
            code = ProcessExpressionOutputs(code, codeBuilder.WrapAsExpressionOutput);
            code = ProcessRepeatExpressions(code, codeBuilder.WrapAsRepeatExpression);
            code = ProcessConditionExpressions(code, codeBuilder.WrapAsConditionExpression);
            code = ProcessCodeBlocks(code);
            code = codeBuilder.WrapAsMethod(code, variables);
            code = codeBuilder.WrapAsProgram(code, usings);
            return code;
        }

        private static string ProcessConditionExpressions(string code, Func<string, string, string> conditionWrapper)
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

        private static string ProcessRepeatExpressions(string code, Func<string, string, string> repeatExpressionWrapper)
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

        private static string ProcessExpressionOutputs(string code, Func<string, string> expressionWrapper)
        {
            return Regex.Replace(
                code,
                @"\[%=(.*?)%\]",
                match => String.IsNullOrEmpty(match.Groups[1].Value) ? String.Empty : expressionWrapper(match.Groups[1].Value),
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
        }

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
            this.script.Run(output, values);
        }
    }
}
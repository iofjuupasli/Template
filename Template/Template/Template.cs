namespace Template
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class Template : IDisposable
    {
        private readonly IProgrammingLanguage language;

        private const string CodeExpressionOpenBracket = "[%";
        private const string CodeExpressionCloseBracket = "%]";

        private readonly IScript script;

        public Template(IProgrammingLanguage language, string templateCode, string[] usings)
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

            this.language = language;

            var code = BuildCode(templateCode, language.GetCodeBuilder());
            this.script = this.language.Compile(code);
        }

        private static string BuildCode(string code, ICodeBuilder codeBuilder)
        {
            code = code.Replace("[%", string.Empty).Replace("%]", string.Empty);
            code = codeBuilder.CoverAsProgram(code);
            return code;
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

        public void Render(TextWriter output)
        {
            this.script.Run(output);
        }
    }
}
namespace Template
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class Template : IDisposable
    {
        private const string CodeExpressionOpenBracket = "[%";

        private readonly IScript script;

        public Template(IProgrammingLanguage language, string templateCode, string[] usings)
        {
            if (IsTemplateCodeLanguageIndependent(templateCode))
            {
                this.script = new PlainTextOutputScript(templateCode);
            }
            if (!IsBracketsCorresponding(templateCode))
            {
                throw new BracketsNotCorrespondsException();
            }
        }

        private static bool IsBracketsCorresponding(string templateCode)
        {
            var openBrackets = Regex.Matches(templateCode, @"\[%", RegexOptions.Singleline);
            var closeBrackets = Regex.Matches(templateCode, @"%\]", RegexOptions.Singleline);
            if (openBrackets.Count != closeBrackets.Count)
            {
                return false;
            }

            var allBrackets = openBrackets.Cast<Match>()
                    .Concat(closeBrackets.Cast<Match>())
                    .OrderBy(match => match.Index);

            for (var bracketIndex = 0; bracketIndex < allBrackets.Count(); bracketIndex++)
            {
                if (allBrackets.ElementAt(bracketIndex).Value != (bracketIndex % 2 == 0 ? "[%" : "%]"))
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
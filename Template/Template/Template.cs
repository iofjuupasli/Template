namespace Template
{
    using System;
    using System.IO;
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
            if (IsBracketsNotCorresponding(templateCode))
            {
                throw new BracketsNotCorrespondsException();
            }
        }

        private static bool IsBracketsNotCorresponding(string templateCode)
        {
            return Regex.IsMatch(templateCode, @"\[%(?!.*%])", RegexOptions.Multiline | RegexOptions.Singleline);
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
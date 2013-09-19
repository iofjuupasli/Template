namespace Template
{
    using System;
    using System.IO;

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
            if (templateCode.Contains(@"[%") && !templateCode.Contains(@"%]"))
            {
                throw new BracketsNotCorrespondsException();
            }
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
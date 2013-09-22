namespace Template
{
    using System.IO;

    public class PlainTextOutputScript : IScript
    {
        private readonly string text;

        public PlainTextOutputScript(string text)
        {
            this.text = text;
        }

        public void Run(TextWriter output, params object[] values)
        {
            output.Write(this.text);
        }

        public void Dispose()
        {
        }
    }
}
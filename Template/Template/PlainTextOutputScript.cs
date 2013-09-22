namespace Template
{
    using System.IO;
    using System.Linq;

    public class PlainTextOutputScript : IScript
    {
        private readonly string text;

        public PlainTextOutputScript(string text)
        {
            this.text = text;
        }

        public void Run(params object[] values)
        {
            var output = (TextWriter)values.Last();
            output.Write(this.text);
        }

        public void Dispose()
        {
        }
    }
}
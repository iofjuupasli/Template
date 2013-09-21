namespace Template
{
    public interface ICodeBuilder
    {
        string WrapAsProgram(string methodBody);

        string WrapAsPlainTextOutputStatement(string text);
    }
}
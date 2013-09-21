namespace Template
{
    public interface ICodeBuilder
    {
        string WrapAsProgram(string method);

        string WrapAsPlainTextOutputStatement(string text);

        string WrapAsMethod(string methodBody);
    }
}
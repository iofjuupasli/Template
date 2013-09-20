namespace Template
{
    public interface IProgrammingLanguage
    {
        ICodeBuilder GetCodeBuilder();

        IScript Compile(string code);
    }
}
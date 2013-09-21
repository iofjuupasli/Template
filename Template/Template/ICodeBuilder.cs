namespace Template
{
    public interface ICodeBuilder
    {
        string WrapAsProgram(string method);

        string WrapAsPlainTextOutputStatement(string text);

        string WrapAsMethod(string methodBody);

        string WrapAsExpressionOutput(string expression);

        string WrapAsRepeatExpression(string expression, string codeToRepeat);

        string WrapAsConditionExpression(string condition, string codeOnTrue);
    }
}
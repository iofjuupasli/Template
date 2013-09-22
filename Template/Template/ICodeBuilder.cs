namespace Template
{
    public interface ICodeBuilder
    {
        string WrapAsProgram(string method, string[] usings);

        string WrapAsPlainTextOutputStatement(string text);

        string WrapAsMethod(string methodBody, params Variable[] variables);

        string WrapAsExpressionOutput(string expression);

        string OpenRepeatExpression(string repeatCountExpression);

        string CloseRepeatExpression();

        string OpenConditionExpression(string conditionExpression);

        string CloseConditionExpression();

        string ConvertType(ArgumentType type);
    }
}
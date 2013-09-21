namespace TemplateTest
{
    using System;

    using Moq;

    using Template;

    internal class MockLanguageBuilder
    {
        private readonly Mock<IProgrammingLanguage> languageMock;

        private readonly Mock<ICodeBuilder> codeBuilderMock;

        public MockLanguageBuilder()
        {
            this.codeBuilderMock = new Mock<ICodeBuilder>(MockBehavior.Strict);
            this.languageMock = new Mock<IProgrammingLanguage>(MockBehavior.Strict);
            this.languageMock.Setup(language => language.GetCodeBuilder())
                .Returns(this.codeBuilderMock.Object);
        }

        public MockLanguageBuilder WithProgramWrapper(string programStructure)
        {
            this.codeBuilderMock.Setup(builder => builder.WrapAsProgram(It.IsAny<string>()))
                .Returns<string>(templateCode => String.Format(programStructure, templateCode));
            return this;
        }

        public MockLanguageBuilder WithMethodWrapper(string methodStructure)
        {
            this.codeBuilderMock.Setup<string>(builder => builder.WrapAsMethod(It.IsAny<string>()))
                .Returns<string>(methodBody => String.Format(methodStructure, methodBody));
            return this;
        }

        public MockLanguageBuilder WithPlainTextWrapper(string outputStatementStructure)
        {
            this.codeBuilderMock.Setup(builder => builder.WrapAsPlainTextOutputStatement(It.IsAny<string>()))
                .Returns<string>(textToOutput => String.Format(outputStatementStructure, textToOutput));
            return this;
        }

        public MockLanguageBuilder OutputSelfCode()
        {
            this.languageMock.Setup(language => language.Compile(It.IsAny<string>()))
                .Returns<string>(programmCode => new PlainTextOutputScript(programmCode));
            return this;
        }


        public MockLanguageBuilder WithExpressionOutputWrapper(string expressionOutputStructure)
        {
            this.codeBuilderMock.Setup(builder => builder.WrapAsExpressionOutput(It.IsAny<string>()))
                .Returns<string>(expression => String.Format(expressionOutputStructure, expression));
            return this;
        }

        public MockLanguageBuilder WithRepeatWrapper(string repeatStructure)
        {
            this.codeBuilderMock.Setup(
                builder => builder.WrapAsRepeatExpression(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>(
                    (expression, toRepeat) => String.Format(repeatStructure, expression, toRepeat));
            return this;
        }

        public MockLanguageBuilder WithConditionWrapper(string conditionStructure)
        {
            this.codeBuilderMock.Setup(
                builder => builder.WrapAsConditionExpression(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>(
                    (condition, onTrue) => String.Format(conditionStructure, condition, onTrue));
            return this;
        }

        public IProgrammingLanguage GetObject()
        {
            return this.languageMock.Object;
        }
    }
}
namespace TemplateTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
            this.codeBuilderMock.Setup(builder => builder.WrapAsProgram(It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns<string, string[]>((templateCode, usings) => String.Format(programStructure, templateCode));
            return this;
        }

        public MockLanguageBuilder WithMethodWrapper(string methodStructure)
        {
            this.codeBuilderMock.Setup<string>(builder => builder.WrapAsMethod(It.IsAny<string>(), It.IsAny<Variable[]>()))
                .Returns<string, Variable[]>(
                    (methodBody, variables) => variables.Any()
                        ? String.Format(
                            methodStructure,
                            methodBody,
                            String.Join(", ",
                                variables.Select(variable => String.Format("{0} {1}", variable.Type, variable.Name))))
                        : String.Format(methodStructure, methodBody));
            return this;
        }

        public MockLanguageBuilder WithPlainTextWrapper(string outputStatementStructure)
        {
            this.codeBuilderMock.Setup(builder => builder.WrapAsPlainTextOutputStatement(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((textToOutput, output) => String.Format(outputStatementStructure, textToOutput));
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
            this.codeBuilderMock.Setup(builder => builder.WrapAsExpressionOutput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((expression, output) => String.Format(expressionOutputStructure, expression));
            return this;
        }

        public MockLanguageBuilder WithRepeatWrapper(string openRepeatStructure, string closeRepeatStructure)
        {
            this.codeBuilderMock.Setup(
                builder => builder.OpenRepeatExpression(It.IsAny<string>()))
                .Returns<string>(
                    expression => String.Format(openRepeatStructure, expression));

            this.codeBuilderMock.Setup(
                builder => builder.CloseRepeatExpression())
                .Returns(closeRepeatStructure);

            return this;
        }

        public MockLanguageBuilder WithConditionWrapper(string openConditionStructure, string closeConditionStructure)
        {
            this.codeBuilderMock.Setup(
                builder => builder.OpenConditionExpression(It.IsAny<string>()))
                .Returns<string>(
                    condition => String.Format(openConditionStructure, condition));

            this.codeBuilderMock.Setup(
                builder => builder.CloseConditionExpression())
                .Returns(closeConditionStructure);

            return this;
        }

        public MockLanguageBuilder WithTypeMapping(Dictionary<ArgumentType, string> typeMapping)
        {
            this.codeBuilderMock.Setup<string>(builder => builder.ConvertType(It.IsAny<ArgumentType>()))
                .Returns<ArgumentType>(type => typeMapping[type]);
            return this;
        }

        public IProgrammingLanguage GetObject()
        {
            return this.languageMock.Object;
        }
    }
}
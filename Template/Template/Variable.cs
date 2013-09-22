namespace Template
{
    public class Variable
    {
        public string Name { get; set; }
        public ArgumentType Type { get; set; }

        public Variable(string name, ArgumentType type)
        {
            this.Name = name;
            this.Type = type;
        }
    }
}
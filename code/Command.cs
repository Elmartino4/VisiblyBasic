using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// contains a large set of all supported commands, as portions of the abstract syntax tree
// and instruction on thier conversion from VisiblyBasic code to Assembler code
namespace Pseudocode_Interpretter
{
    abstract class Command
    {
        static int iterator = -1;
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        // generates a reference to a position or value to be used in assembler code,
        // essentially a random string generator
        public static string GenerateReferenceRaw()
        {
            iterator++;
            StringBuilder stringBuilder = new StringBuilder();

            int diviser = 1;
            for (int i = 0; i < 3; i++)
            {
                stringBuilder.Insert(0, chars[(iterator/diviser) % chars.Length]);
                diviser *= chars.Length;
            }

            return stringBuilder.ToString();
        }

        public static string GenerateReference(string type, int? index, string randomVal)
        {
            return "__" + type.ToUpper() + (index != null ? ("_" + index + "_") : "_") + randomVal + "__";
        }

        // add VisiblyBasic code to Assembler code as comments
        public static List<string> DrawArrows(List<string> text, string raw)
        {
            for (int i = 0; i < text.Count; i++)
            {
                text[i] = text[i].PadRight(Compiler.Padding, ' ') + (i == 0 ? "' " + raw : "' ^");
            }

            return text;
        }

        public ContentCommand? parent { get; protected set; } = null;
        protected int indexInParent = 0;

        protected Command(ContentCommand parent)
        {
            this.parent = parent;
            parent.subcommandList.Add(this);
            this.indexInParent = parent.subcommandList.Count - 1;
        }

        protected Command()
        {

        }

        // translates tree to assembler code
        public abstract List<string> Render();
    }

    class GetCommand : Command
    {
        string name;
        SubRoutineCommand container;
        bool isArray;

        public GetCommand(ContentCommand parent, string name, SubRoutineCommand container, bool isArray) : base(parent)
        {
            this.name = name;
            this.container = container;
            this.isArray = isArray;
        }

        // extract paramater from subroutine, and store it to a standard variable
        public override List<string> Render()
        {
            StringBuilder line = new StringBuilder();
            line.Append("EQUATE ");
            line.Append(Command.GenerateReference("VAR", null, name)).Append(' ');
            line.Append(Command.GenerateReference("PARAM", container.GetParamIndex(name), container.name));
            
            line.Append(' ', Math.Max(Compiler.Padding - line.Length, 0));
            line.Append("' ").Append("GET ").Append(name);
            if (isArray)
            {
                line.Append("()");
            }
            

            return (new string[] { line.ToString() }).ToList();
        }
    }

    class ReturnCommand : Command
    {
        Expression value;
        SubRoutineCommand subroutineOfReturn;
        string raw;

        public ReturnCommand(ContentCommand parent, Expression value, SubRoutineCommand subroutineOfReturn, string raw) : base(parent)
        {
            this.value = value;
            this.subroutineOfReturn = subroutineOfReturn;
            this.raw = raw;
        }

        // exit subroutine, and send a certain value back
        public override List<string> Render()
        {
            string randomValue = GenerateReferenceRaw();

            List<string> output = new List<string>();

            output.AddRange(value.Render(GenerateReference("TEMP", null, randomValue)));
            output.Add("EQUATE " + Command.GenerateReference("RETURN", null, subroutineOfReturn.name) + " " + GenerateReference("TEMP", null, randomValue));
            output.Add("JUMP " + Command.GenerateReference("ANCHOR", 1, subroutineOfReturn.name) + " __CONSTANT_1__");

            return DrawArrows(output, raw);
        }
    }

    class SetCommand : Command
    {
        Expression value;
        VariableExpression variable;
        string raw;

        public SetCommand(ContentCommand parent, Expression value, VariableExpression variable, string raw) : base(parent)
        {
            this.value = value;
            this.variable = variable;
            this.raw = raw;
        }

        // set a variable to the value of another
        public override List<string> Render()
        {
            string randomValue = GenerateReferenceRaw();

            List<string> output = new List<string>();
            output.AddRange(value.Render(GenerateReference("TEMP", null, randomValue)));
            output.AddRange(variable.UpdateValue(GenerateReference("TEMP", null, randomValue)));

            return DrawArrows(output, raw);
        }
    }

    // refers to a command which contains more code within itself
    // a general abstract class to fulfill this purpose
    abstract class ContentCommand : Command
    {
        public List<Command> subcommandList = new List<Command>();
        public string? raw = null;

        public Command GetSubCommand(int depth)
        {
            return subcommandList[depth];
        }

        public ContentCommand(ContentCommand parent) : base(parent)
        {

        }

        public ContentCommand(ContentCommand parent, string raw) : base(parent)
        {
            this.raw = raw;
        }

        protected ContentCommand() : base()
        {

        }

        // render every line of code from within initially
        public override List<string> Render()
        {
            List<string> output = Initial();

            if (raw != null)
            {
                output = DrawArrows(output, raw);
            }

            for (int i = 0; i < subcommandList.Count; i++)
            {
                output.AddRange(subcommandList[i].Render());
            }

            output.AddRange(Final());

            return output;
        }

        protected abstract List<string> Initial();
        protected abstract List<string> Final();
    }


    // a typical for loop of format:
    // IF <variable> = <min> TO <max>
    class ForNumCommand : ContentCommand
    {
        public string varName;
        public Expression minVal;
        public Expression maxVal;
        string randVal = GenerateReferenceRaw();

        public ForNumCommand(string varName, Expression minVal, Expression maxVal, ContentCommand parent, string raw) : base(parent, raw)
        {
            this.varName = varName;
            this.minVal = minVal;
            this.maxVal = maxVal;
        }

        // updates <variable> and loops through to the Initial block
        protected override List<string> Final()
        {
            List<string> output = new List<string>();

            output.Add("ADD " + GenerateReference("VAR", null, varName) + " __CONSTANT_1__");
            output.Add("JUMP " + Command.GenerateReference("ANCHOR", 0, randVal) + " __CONSTANT_1__");
            output.Add(":" + GenerateReference("ANCHOR", 1, randVal));

            return DrawArrows(output, "NEXT " + varName);
        }

        // sets the <varibale> to the evaluation of the initial expression
        protected override List<string> Initial()
        {
            List<string> output = new List<string>();

            output.AddRange(minVal.Render(GenerateReference("VAR", null, varName)));

            output.Add(":" + GenerateReference("ANCHOR", 0, randVal));
            output.AddRange(maxVal.Render(GenerateReference("TEMP", 0, randVal)));
            output.Add("CSUB " + GenerateReference("TEMP", 0, randVal) + " " + GenerateReference("VAR", null, varName));
            output.Add("ATZERO " + GenerateReference("TEMP", 0, randVal));
            output.Add("JUMP " + Command.GenerateReference("ANCHOR", 1, randVal) + " " + GenerateReference("TEMP", 0, randVal));

            return output;
        }
    }

    // WHILE loop, where the loop is continued each time the condition is met
    class WhileLoopCommand : ContentCommand
    {
        public Expression condition;
        public string randVal;

        public WhileLoopCommand(Expression condition, ContentCommand parent, string raw) : base(parent)
        {
            this.condition = condition;
            this.randVal = GenerateReferenceRaw();
            this.raw = raw;
        }

        // initiallises anchors for the exit condition
        protected override List<string> Final()
        {
            List<string> output = new List<string>();

            output.Add("JUMP " + GenerateReference("ANCHOR", 0, randVal) + " __CONSTANT_1__");
            output.Add(":" + GenerateReference("ANCHOR", 1, randVal));
            return DrawArrows(output, "ENDWHILE");
        }

        // evaluates loop condition, jumps to exit label is condition not met
        protected override List<string> Initial()
        {
            List<string> output = new List<string>();
            output.Add(":" + GenerateReference("ANCHOR", 0, randVal));
            output.AddRange(condition.Render(GenerateReference("TEMP", null, randVal)));

            output.Add("ATZERO " + GenerateReference("TEMP", null, randVal));
            output.Add("JUMP " + GenerateReference("ANCHOR", 1, randVal) + " " + GenerateReference("TEMP", null, randVal));

            return output;
        }
    }

    class IfCommand : ContentCommand
    {
        // subCommandList is ignored

        public List<SubConditionalIf> subConditionalIfs = new List<SubConditionalIf>();
        ArtificalContentCommand? elseStatement = null;

        public IfCommand(Expression baseCondition, ContentCommand parent, string raw) : base(parent, raw)
        {
            SubConditionalIf subConditionalIf = new SubConditionalIf(baseCondition, this);
            subConditionalIfs.Add(subConditionalIf);
        }

        public ArtificalContentCommand AddCondition(Expression condition)
        {
            SubConditionalIf subConditionalIf = new SubConditionalIf(condition, this);
            subConditionalIfs.Add(subConditionalIf);
            return subConditionalIf.command;
        }

        public ArtificalContentCommand GenerateElseCommand(string raw)
        {
            elseStatement = new ArtificalContentCommand(this, raw);
            return elseStatement;
        }

        /*
            IF [condition 1] THEN
                [process 1]
            ELSEIF [condition 2] THEN
                [process 2]
            ELSE
                [process]
            ENDIF

        Becomes:

            JUMP [anchor 1] [not condition 1]
            [process 1]
            JUMP [anchor 3] always
            'REM anchor 1

            JUMP [anchor 2] [not condition 2]
            [process 2]
            JUMP [anchor 3] always
            'REM anchor 2

            [process 3]
            'REM anchor 3
        */

        public override List<string> Render()
        {
            string referenceRandomVal = Command.GenerateReferenceRaw();
            List<string> output = new List<string>();
            int anchorCount = subConditionalIfs.Count - 1;
            if (elseStatement != null)
                anchorCount++;

            // convert if/elseif statements
            int currLen = 0;
            for (int i = 0; i < subConditionalIfs.Count; i++)
            {
                output.AddRange(subConditionalIfs[i].condition.Render(Command.GenerateReference("CONDITION", i, referenceRandomVal)));
                output.Add("ATZERO " + Command.GenerateReference("CONDITION", i, referenceRandomVal));
                output.Add("JUMP " + Command.GenerateReference("ANCHOR", i, referenceRandomVal) + " " + Command.GenerateReference("CONDITION", i, referenceRandomVal));
                for (int j = currLen + 1; j < output.Count; j++)
                {
                    output[j] = output[j].PadRight(Compiler.Padding, ' ') + "' ^";
                }
                output.AddRange(subConditionalIfs[i].command.Render());
                output[currLen] = output[currLen].PadRight(Compiler.Padding, ' ') + "' " + (i == 0 ? raw : subConditionalIfs[i].command.raw);
                currLen = output.Count;
                output.Add("JUMP " + Command.GenerateReference("ANCHOR", anchorCount, referenceRandomVal) + " __CONSTANT_1__");
                output.Add(":" + Command.GenerateReference("ANCHOR", i, referenceRandomVal));
            }

            // convert else statmeent
            if (elseStatement != null)
            {
                output[currLen] = output[currLen].PadRight(Compiler.Padding, ' ') + "' ELSE";
                for (int j = currLen + 1; j < output.Count; j++)
                {
                    output[j] = output[j].PadRight(Compiler.Padding, ' ') + "' ^";
                }
                output.AddRange(elseStatement.Render());
                output.Add(":" + Command.GenerateReference("ANCHOR", anchorCount, referenceRandomVal));

                output[output.Count - 1] = output[output.Count - 1].PadRight(Compiler.Padding, ' ') + "' ENDIF";

                currLen = output.Count;
            }
            else
            {
                for (int i = currLen; i < output.Count; i++)
                {
                    output[i] = output[i].PadRight(Compiler.Padding, ' ') + (i == currLen ? "' ENDIF" : "' ^");
                }
            }

            

            return output;
        }

        protected override List<string> Initial() { throw new NotImplementedException(); } // intentional

        protected override List<string> Final() { throw new NotImplementedException(); } // intentional

        internal class SubConditionalIf
        {
            public Expression condition { protected set; get; }
            public ArtificalContentCommand command;

            public SubConditionalIf(Expression condition, IfCommand initialiser)
            {
                this.condition = condition;
                this.command = new ArtificalContentCommand(initialiser);
                
            }
        }
    }

    // used for if statements
    class ArtificalContentCommand : ContentCommand
    {
        public ArtificalContentCommand(ContentCommand parent) : base(parent)
        {

        }

        public ArtificalContentCommand(ContentCommand parent, string raw) : base(parent, raw)
        {
        }

        protected override List<string> Final()
        {
            return new List<string>();
        }

        protected override List<string> Initial()
        {
            return new List<string>();
        }
    }

    // converts a sub-routine
    class SubRoutineCommand : ContentCommand
    {
        public string name;

        List<string> supportedParams = new List<string>();

        public SubRoutineCommand(string name)
        {
            this.name = name;
        }

        public void AddParam(string param)
        {
            supportedParams.Add(param);
        }

        public int GetParamIndex(string param)
        {
            return supportedParams.IndexOf(param);
        }

        protected override List<string> Final()
        {
            return (new string[] { ":" + Command.GenerateReference("ANCHOR", 1, name).PadRight(Compiler.Padding - 1) + "' END " + name }).ToList();
        }

        protected override List<string> Initial()
        {
            return (new string[] { ":" + Command.GenerateReference("ANCHOR", 0, name).PadRight(Compiler.Padding - 1) + "' BEGIN " + name }).ToList();
        }
    }
}

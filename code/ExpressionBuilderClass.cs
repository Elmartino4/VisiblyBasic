using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pseudocode_Interpretter
{
    class Expression
    {
        static string[][] operatorArrays =
        {
            new string[] { "||" },
            new string[] { "&&" },
            new string[] { "|" },
            new string[] { "^" },
            new string[] { "&" },
            new string[] { "==", "!=" },
            new string[] { ">", "<", "<=", ">=" },
            new string[] { "<<", ">>" },
            new string[] { "+", "-" },
            new string[] { "*", "/", "%" },
        };

        static string[] specialOperators =
        {
            "!", "(", ")", "."
        };

        static Dictionary<string, int> constants = new Dictionary<string, int>()
        {
            { "True", 1 },
            { "False", 0 },
        };
        

        string[] components;
        public string Reference;
        protected List<ExpressionObject> expressionObjects;

        // convert expression to unsimplified tree (abstract syntax tree)
        public Expression(string[] components)
        {
            this.components = components;
            expressionObjects = new List<ExpressionObject>(components.Length);

            foreach (string component in components)
            {
                expressionObjects.Add(new RawExpressionToken(component));
            }

            Simplify();
        }

        public void Simplify()
        {
            Console.WriteLine("   Simplyfing Expression Constants: {'" + string.Join("', '", components) + "'}");
            // precheck for variables in expression
            for (int i = 0; i < expressionObjects.Count; i++)
            {
                if (expressionObjects[i] is not RawExpressionToken)
                    continue;

                string current = ((RawExpressionToken)expressionObjects[i]).rawValue;


                if (Regex.IsMatch(current, "^[a-zA-Z_][a-zA-Z_0-9]*$"))   // current token is variable or constant
                {
                    string next = "", previous = "";
                    if (i > 0)
                    {
                        if (expressionObjects[i - 1] is not RawExpressionToken)
                            continue;

                        previous = ((RawExpressionToken)expressionObjects[i - 1]).rawValue;
                    }


                    if (i < expressionObjects.Count - 1)
                    {
                        if (expressionObjects[i + 1] is not RawExpressionToken)
                            continue;

                        next = ((RawExpressionToken)expressionObjects[i + 1]).rawValue;
                    }

                    if ((next == "" || isInOperatorArrays(next)) &&                             // next value is operator
                        (previous == "" || isInOperatorArrays(previous) && previous != "."))    // previous value is operator
                    {
                        if (constants.ContainsKey(current))
                        {
                            expressionObjects[i] = new NumericalConstant(constants[current].ToString());
                        }
                        else
                        {
                            expressionObjects[i] = new LiteralVariableExpressionObject(current);
                        }
                        
                    }
                }
                else if (Regex.IsMatch(current, @"^[0-9][0-9_]*(\.[0-9_]+)?$"))
                {
                    expressionObjects[i] = new NumericalConstant(current);
                }
            }

            Console.WriteLine("   Simplyfing Expression");
            int index = 0;
            while (expressionObjects.Count > 1)
            {
                Console.WriteLine("       Round " + index + ": " + expressionObjects.Count);
                index++;
                int prevCount = expressionObjects.Count;

                // find object child references
                for (int i = 0; i < expressionObjects.Count - 2; i++)
                {
                    if (expressionObjects[i] is VariableContainerExpressionObject &&
                        expressionObjects[i + 1] is RawExpressionToken && ((RawExpressionToken)expressionObjects[i + 1]).rawValue == "." &&
                        expressionObjects[i + 2] is RawExpressionToken)
                    {
                        ExpressionObject final = new VariableChildGetterObject(((RawExpressionToken)expressionObjects[i + 2]).rawValue,
                            (VariableContainerExpressionObject) expressionObjects[i]);

                        expressionObjects.RemoveAt(i + 2);
                        expressionObjects.RemoveAt(i + 1);
                        expressionObjects[i] = final;
                        i--;
                    }
                }

                // find the not operator
                for (int i = 1; i < expressionObjects.Count; i++)
                {
                    if (expressionObjects[i - 1] is RawExpressionToken && ((RawExpressionToken)expressionObjects[i - 1]).rawValue == "!" &&
                        expressionObjects[i] is not RawExpressionToken)
                    {
                        ExpressionObject token = new NotOperator(expressionObjects[i]);
                        expressionObjects.RemoveAt(i);
                        expressionObjects[i - 1] = token;
                    }
                }

                // find array references
                for (int i = 0; i < expressionObjects.Count - 3; i++)
                {
                    if (expressionObjects[i] is VariableContainerExpressionObject && expressionObjects[i + 1] is RawExpressionToken && expressionObjects[i + 3] is RawExpressionToken &&
                        ((RawExpressionToken)expressionObjects[i + 1]).rawValue == "(" && ((RawExpressionToken)expressionObjects[i + 3]).rawValue == ")" &&
                        expressionObjects[i + 2] is not RawExpressionToken)
                    {
                        ExpressionObject indexExpression = expressionObjects[i + 2];
                        VariableContainerExpressionObject container = (VariableContainerExpressionObject)expressionObjects[i];

                        expressionObjects.RemoveAt(i + 3);
                        expressionObjects.RemoveAt(i + 2);
                        expressionObjects.RemoveAt(i + 1);
                        expressionObjects[i] = new ArrayExpressionObject(indexExpression, container);
                    }
                }

                // find operators such as +, -, *, /
                for (int j = 0; j < operatorArrays.Length; j++)
                {
                    List<string> operators = operatorArrays[j].ToList();

                    for (int i = 1; i < expressionObjects.Count - 1; i++)
                    {

                        if (expressionObjects[i] is RawExpressionToken && operators.Contains(((RawExpressionToken)expressionObjects[i]).rawValue)
                            && expressionObjects[i - 1] is not RawExpressionToken && expressionObjects[i + 1] is not RawExpressionToken
                            && (i + 2 == expressionObjects.Count || expressionObjects[i + 2] is not RawExpressionToken || ((RawExpressionToken)expressionObjects[i + 2]).rawValue != "("))
                        {
                            OperatorObject operatorObject = new OperatorObject(
                                expressionObjects[i - 1],
                                expressionObjects[i + 1],
                                ((RawExpressionToken)expressionObjects[i]).rawValue
                            );

                            expressionObjects.RemoveAt(i + 1);
                            expressionObjects.RemoveAt(i);
                            expressionObjects[i - 1] = operatorObject;
                            i--;
                        }
                    }
                }

                // find simplified expressions in brackets
                for (int i = 1; i < expressionObjects.Count - 1; i++)
                {
                    if ((i == 1 || expressionObjects[i - 2] is not VariableContainerExpressionObject) && expressionObjects[i - 1] is RawExpressionToken && expressionObjects[i + 1] is RawExpressionToken &&
                        ((RawExpressionToken)expressionObjects[i - 1]).rawValue == "(" && ((RawExpressionToken)expressionObjects[i + 1]).rawValue == ")" &&
                        expressionObjects[i] is not RawExpressionToken)
                    {
                        ExpressionObject inner = expressionObjects[i];

                        expressionObjects.RemoveAt(i + 1);
                        expressionObjects.RemoveAt(i);
                        expressionObjects[i - 1] = inner;
                    }
                }


                if (prevCount == expressionObjects.Count)
                {
                    throw new Exception("failed to parse expression");
                    // 
                }
            }
        }


        // render such that the output asm retunrs the value to the specific reference
        public List<string> Render(string output)
        {
            List<string> rendered = Render();

            rendered.Add("EQUATE " + output + " " + expressionObjects[0].reference);
            return rendered;
        }

        // render base operation in tree
        public List<string> Render()
        {
            this.Reference = expressionObjects[0].reference;
            return expressionObjects[0].After();
        }

        // check if special character is a known operator.
        public static bool isInOperatorArrays(string token)
        {
            foreach (string[] operators in operatorArrays)
            {
                foreach (string operatorVal in operators)
                {
                    if (token == operatorVal)
                    {
                        return true;
                    }
                }
            }

            foreach (string specialOperator in specialOperators)
            {
                if (token == specialOperator)
                {
                    return true;
                }
            }

            return false;
        }
    }

    abstract class ExpressionObject
    {

        public string reference = "";

        public abstract List<string> After();
        // call "After" of sub-expressions
        // render assembler code
        // generate randomised reference

        public static string GenerateReferenceNum()
        {
            return Command.GenerateReference("TEMP", null, Command.GenerateReferenceRaw());
        }

        public void GenerateReference()
        {
            if (reference == "")
                reference = GenerateReferenceNum();
        }
    }

    class RawExpressionToken : ExpressionObject
    {
        public string rawValue { get; protected set; }

        public RawExpressionToken(string rawValue)
        {
            this.rawValue = rawValue;
        }

        public override List<string> After()
        {
            // with correct operation, this code shouldnt ever be reached
            throw new NotImplementedException(); // intentional
        }
    }

    class LiteralVariableExpressionObject : VariableContainerExpressionObject
    {
        public string varName { get; protected set; }

        public LiteralVariableExpressionObject(string varName)
        {
            this.varName = varName;
        }

        // read from simple variable
        public override List<string> After()
        {
            this.reference = Command.GenerateReference("TEMP", null, Command.GenerateReferenceRaw());
            
            return (new string[] { "EQUATE " + reference + " " + Command.GenerateReference("VAR", null, varName) }).ToList();
        }

        // write to simple variable
        public override List<string> UpdateValue(string tempValStore)
        {
            return (new string[] { "EQUATE " + Command.GenerateReference("VAR", null, varName) + " " + tempValStore }).ToList();
        }
    }

    class ArrayExpressionObject : VariableContainerExpressionObject
    {
        ExpressionObject index;
        VariableContainerExpressionObject caller;

        public ArrayExpressionObject(ExpressionObject index, VariableContainerExpressionObject caller)
        {
            this.index = index;
            this.caller = caller;
        }

        public override List<string> After()
        {
            List<string> output = new List<string>();

            output.AddRange(index.After());
            output.AddRange(caller.After());
            
            GenerateReference();

            // extract/read value in array
            output.AddRange(new string[] {
                "ADD " + index.reference + " " + caller.reference,
                "PNTSET " + reference + " " + index.reference
            });
            return output;
        }

        public override List<string> UpdateValue(string tempValStore)
        {
            List<string> output = new List<string>();
            output.AddRange(index.After());
            output.AddRange(caller.After());

            // indicates no support for inline 2d array access
            // this isn't implemented intentionally
            if (caller is not LiteralVariableExpressionObject)
                throw new NotImplementedException("2d arrays and similar are not supported"); 

            output.Add("ADD " + index.reference + " " + caller.reference);
            output.Add("LOCSET " + index.reference + " " + tempValStore);

            return output;
        }
    }

    class VariableChildGetterObject : VariableContainerExpressionObject
    {
        string key;
        VariableContainerExpressionObject caller;

        public VariableChildGetterObject(string key, VariableContainerExpressionObject caller)
        {
            this.key = key;
            this.caller = caller;
        }

        public override List<string> After()
        {
            List<string> output = new List<string>();

            output.AddRange(caller.After());

            GenerateReference();

            // get the length of an array
            if (key.ToLower() == "length")
            {
                output.Add("CSUB " + caller.reference + " " + Command.GenerateReference("CONSTANT", null, "1"));
                output.Add("PNTSET " + this.reference + " " + caller.reference);
                return output;
            }
            else
            {
                // intentional since Visibly is not object oriented
                throw new NotImplementedException("The only variable child that can be requested is the .length operator");
            }
        }

        public override List<string> UpdateValue(string tempValStore)
        {
            // intentional since "VisiblyBasic" is not object oriented
            // a variable's child cannot be updated
            throw new NotImplementedException();
        }
    }

    abstract class VariableContainerExpressionObject : ExpressionObject
    {
        public string? Pointer { get; protected set; } = null;

        public abstract List<string> UpdateValue(string tempValStore);
    }

    class NumericalConstant : ExpressionObject
    {
        string literal;

        public NumericalConstant(string literal)
        {
            this.literal = literal;
        }

        // load numerical literal into a variable (reference)
        public override List<string> After()
        {
            GenerateReference();
            return (new string[] { "SET " + reference + " " + literal }).ToList();
        }
    }

    class NotOperator : ExpressionObject
    {
        ExpressionObject a;

        public NotOperator(ExpressionObject a)
        {
            this.a = a;
        }

        // convert not operator to asm
        public override List<string> After()
        {
            List<string> output = new List<string>();

            output.AddRange(a.After());

            this.reference = a.reference;
            output.Add("ATZERO " + a.reference);

            return output;
        }
    }

    class OperatorObject : ExpressionObject
    {
        ExpressionObject a, b;
        string expressionLiteral;

        public OperatorObject(ExpressionObject a, ExpressionObject b, string expressionLiteral)
        {
            this.a = a;
            this.b = b;
            this.expressionLiteral = expressionLiteral;
        }

        /*
            new string[] { "||" },
            new string[] { "&&" },
            new string[] { "|" },
            new string[] { "^" },
            new string[] { "&" },
            new string[] { "==", "!=" },
            new string[] { ">", "<", "<=", ">=" },
            new string[] { "<<", ">>" },
            new string[] { "+", "-" },
            new string[] { "*", "/", "%" },
            new string[] { "~", "!" },
        */

        public override List<string> After()
        {
            List<string> output = new List<string>();

            output.AddRange(a.After());
            output.AddRange(b.After());

            this.reference = a.reference;

            // convert expression to asm code
            switch (expressionLiteral)
            {
                case "||":
                case "|":
                    output.Add("OR " + a.reference + " " + b.reference);
                    break;

                case "&&":
                case "&":
                    output.Add("AND " + a.reference + " " + b.reference);
                    break;

                case "!=":
                case "^":
                    output.Add("XOR " + a.reference + " " + b.reference);
                    break;

                case "==":
                    output.Add("XOR " + a.reference + " " + b.reference);
                    output.Add("ATZERO " + a.reference);
                    break;

                case "<":
                    output.Add("CSUB " + b.reference + " " + a.reference);
                    output.Add("EQUATE " + a.reference + " " + b.reference);
                    break;

                case ">=":
                    output.Add("CSUB " + b.reference + " " + a.reference);
                    output.Add("ATZERO " + b.reference);
                    output.Add("EQUATE " + a.reference + " " + b.reference);
                    
                    break;

                case ">":
                    output.Add("CSUB " + a.reference + " " + b.reference);
                    break;

                case "<=":
                    output.Add("CSUB " + a.reference + " " + b.reference);
                    output.Add("ATZERO " + a.reference);
                    break;

                case ">>":
                    output.Add("SAR " + a.reference + " " + b.reference);
                    break;

                case "<<":
                    output.Add("SAL " + a.reference + " " + b.reference);
                    break;

                case "+":
                    output.Add("ADD " + a.reference + " " + b.reference);
                    break;

                case "-":
                    output.Add("CSUB " + a.reference + " " + b.reference);
                    break;

                case "*":
                    output.Add("MULT " + a.reference + " " + b.reference);
                    break;

                case "/":
                    output.Add("DIV " + a.reference + " " + b.reference);
                    break;

                case "%":
                    output.Add("MOD " + a.reference + " " + b.reference);
                    break;

                default:
                    break;
            }

            return output;
        }
    }

    class VariableExpression : Expression
    {
        // parses an example string[] of {"abc", "(", "abc", ".", "def", ")"}

        public VariableExpression(string[] components) : base(components)
        {

        }

        // updates value within variable expression to be equal to the value stored at name
        public List<string> UpdateValue(string name)
        {
            return ((VariableContainerExpressionObject)expressionObjects[0]).UpdateValue(name);
        }
    }
}

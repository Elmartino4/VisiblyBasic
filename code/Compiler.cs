using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// contains the nessecary data to convert a piece of VsibilyBasic Code to Asembler code
namespace Pseudocode_Interpretter
{
    
    internal class Compiler
    {
        Dictionary<string, SubRoutineCommand> subroutineDict = new Dictionary<string, SubRoutineCommand>();
        string FileName;
        public const int Padding = 55;

        public Compiler(string filename)
        {
            FileName = filename;
        }

        public void RunCompile()
        {
            int lineIndex = 0;
            List<string[]> bigTokensArray = new List<string[]>();
            List<string> metadata = new List<string>();

            
            // loops through pseudocode line by line, and converts each line into a list of tokens.
            foreach (string line in File.ReadLines("../../../tests/" + FileName + ".pseudo"))
            {
                if (lineIndex == 0)
                    Console.WriteLine("Spliting Tokens");

                // check for metadata in pseudocode block
                if (Regex.IsMatch(line, @"^#-- (type|input|main)*: [a-zA-Z_][a-zA-Z_0-9\-]*(, [a-zA-Z_][a-zA-Z_0-9\-]*)*$"))
                {
                    metadata.Add(line.Replace("#--", "'--"));
                }

                lineIndex++;
                string trimmedLine = line.Trim(); // remove white space from around line

                trimmedLine = Regex.Replace(trimmedLine, @"#.*$", ""); // removes comments
                trimmedLine = Regex.Replace(trimmedLine, @":$", ""); // removes a singular colon which is included in some pseudocode examples

                if (trimmedLine == "")
                    continue; // keep going if the line is blank or contains only comments


                MatchCollection tokens = Regex.Matches(trimmedLine, "([a-zA-Z_][a-zA-Z_0-9]*|" +
                    @"[0-9][0-9_]*(\.[0-9_]+)?|" +
                    "\\\"([^\\\\\\\"\\n]|\\\\(\\\\\\\\)*.)*\\\"|" +
                    @"'(\\.|[^\\'\n])'|" +
                    @"(<=|>=|==|!=|<>|<<|>>|\+\+|--)|" +
                    "[!&()*+,\\-./<=>'\":^|])");

                string[] tokensArray = new string[tokens.Count];

                for (int i = 0; i < tokens.Count; i++)
                {
                    tokensArray[i] = tokens[i].Value;
                }

                // prints the parsed code to console.
                Console.WriteLine("   {0}: ['{1}']", lineIndex, string.Join("\', \'", tokensArray));

                bigTokensArray.Add(tokensArray);
            }

            string currentSubroutine = "";

            ContentCommand? currentContentCommand = null;


            Console.WriteLine("Simplifying Tokens");
            int index = 0;
            foreach (string[] tokens in bigTokensArray)
            {
                Console.WriteLine("   Line " + index + ": \"'" + string.Join("\', \'", tokens) + "'\"");
                PrintStack(currentContentCommand);
                index++;
                if (currentContentCommand == null)
                {
                    // check top-level statement is subroutine
                    if (tokens[0] == "BEGIN" && tokens.Length == 2)
                    {
                        currentSubroutine = tokens[1];
                        subroutineDict.Add(currentSubroutine, new SubRoutineCommand(currentSubroutine));
                        currentContentCommand = subroutineDict[currentSubroutine];
                    }
                    else
                    {
                        throw new CompilerException("Bad Top Level Statement", index);
                    }
                }
                else
                {
                    switch (tokens[0])
                    {
                        case "END":
                            
                            if (tokens[1] != currentSubroutine)
                                throw new CompilerException("END statement refers to wrong subroutine", index);

                            if (tokens.Length != 2)
                                throw new CompilerException("Bad END statement", index);

                            currentSubroutine = "";
                            break;

                        case "GET": // find get statement and parse contents
                            subroutineDict[currentSubroutine].AddParam(tokens[1]);
                            

                            if (!(tokens.Length == 2 || tokens.Length == 4 && tokens[2] == "(" && tokens[3] == ")"))
                                throw new CompilerException("Bad GET statement", index);

                            new GetCommand(currentContentCommand, tokens[1], subroutineDict[currentSubroutine],
                                tokens.Length == 4);

                            break;

                        case "FOR":
                            int toKeywordIndex = tokens.ToList().FindIndex((str) => str == "TO");
                            if (tokens[2] == "=" && toKeywordIndex > 3 && tokens.Length > toKeywordIndex)
                            {
                                // generates expressions in the for command.
                                string[] minStr = new string[toKeywordIndex - 3], maxStr = new string[tokens.Length - toKeywordIndex - 1];

                                Array.Copy(tokens, 3, minStr, 0, minStr.Length);
                                Array.Copy(tokens, toKeywordIndex + 1, maxStr, 0, maxStr.Length);

                                Expression minExpr = new Expression(minStr), maxExpr = new Expression(maxStr);


                                // create fornum command, and add this to the call tree.
                                ForNumCommand forNumCommand = new ForNumCommand(tokens[1], minExpr, maxExpr, currentContentCommand, string.Join(" ", tokens));

                                currentContentCommand = forNumCommand;
                            }
                            else
                            {
                                throw new CompilerException("Bad For Loop Definition", index);
                            }
                            break;

                        case "IF":
                            {
                                // ignore last token if "IF" statement ends with "THEN"
                                string[] conditionExpression = new string[tokens.Length - 1];
                                if (tokens[tokens.Length - 1] == "THEN")
                                {
                                    conditionExpression = new string[tokens.Length - 2];
                                }

                                Array.Copy(tokens, 1, conditionExpression, 0, conditionExpression.Length);

                                IfCommand command = new IfCommand(new Expression(conditionExpression), currentContentCommand, string.Join(" ", tokens));

                                currentContentCommand = command.subConditionalIfs[0].command;
                            }
                            break;

                        case "ELSEIF": // search for elseif statement
                            {
                                // add this elseif statement to the latest if statement
                                currentContentCommand = currentContentCommand.parent;
                                if (currentContentCommand is IfCommand)
                                {
                                    string[] conditionExpression = new string[tokens.Length - 1];
                                    if (tokens[tokens.Length - 1] == "THEN")
                                    {
                                        conditionExpression = new string[tokens.Length - 2];
                                    }
                                    Array.Copy(tokens, 1, conditionExpression, 0, conditionExpression.Length);

                                    currentContentCommand = ((IfCommand)currentContentCommand).AddCondition(new Expression(conditionExpression));

                                    currentContentCommand.raw = string.Join(' ', tokens);
                                }
                                else
                                {
                                    throw new CompilerException("Corresponding stack layer isn't an IF statement", index);
                                }

                            }
                            break;

                        case "ELSE": // close off if command (with else)
                            currentContentCommand = currentContentCommand.parent;
                            if (currentContentCommand is IfCommand)
                            {
                                currentContentCommand = ((IfCommand)currentContentCommand).GenerateElseCommand(string.Join(" ", tokens));
                            }
                            else
                            {
                                throw new CompilerException("Corresponding stack layer isn't an IF statement", index);
                            }
                            break;

                        case "NEXT": // close for statmenet
                            if (currentContentCommand is not ForNumCommand)
                                throw new CompilerException("Corresponding stack layer isn't an FOR statement", index);

                            currentContentCommand = currentContentCommand.parent;

                            break;

                        case "ENDIF":
                            currentContentCommand = currentContentCommand.parent;
                            if (currentContentCommand is IfCommand)
                            {
                                currentContentCommand = currentContentCommand.parent;
                            }
                            else
                            {
                                
                                throw new CompilerException("Stack Layer isn't if statement", index);
                            }

                            break;
                        case "RETURN": // find, close subroutine with return statement
                            string[] returnExpression = new string[tokens.Length - 1];
                            Array.Copy(tokens, 1, returnExpression, 0, returnExpression.Length);

                            new ReturnCommand(currentContentCommand, new Expression(returnExpression), subroutineDict[currentSubroutine], string.Join(' ', tokens));
                            break;

                        case "SET": // find all set statements
                            int equalsIndex = tokens.ToList().FindIndex((str) => str == "=");

                            string[] variable = new string[equalsIndex - 1], expression = new string[tokens.Length - equalsIndex - 1];

                            Array.Copy(tokens, 1, variable, 0, variable.Length);
                            Array.Copy(tokens, equalsIndex + 1, expression, 0, expression.Length);

                            new SetCommand(currentContentCommand, new Expression(expression), new VariableExpression(variable), string.Join(' ', tokens));
                            break;

                        case "WHILE": // find while statements
                            string[] condition = new string[tokens.Length - 1];
                            Array.Copy(tokens, 1, condition, 0, tokens.Length - 1);
                            WhileLoopCommand loop = new WhileLoopCommand(new Expression(condition), currentContentCommand, string.Join(' ', tokens));
                            currentContentCommand = loop;
                            break;

                        case "ENDWHILE":
                            currentContentCommand = currentContentCommand.parent;
                            break;

                        default:
                            throw new CompilerException("Bad Token", index);
                    }
                }
            }

            // write tokens to .asm file
            Console.WriteLine("Rendering Tokens");
            File.Delete("../../../tests/" + FileName + ".asm");
            using (StreamWriter writer = new StreamWriter("../../../tests/" + FileName + ".asm"))
            {
                // load metadata
                foreach (string metadataLine in metadata)
                {
                    writer.WriteLine(metadataLine);
                }

                if (metadata.Count != 0)
                    writer.WriteLine();

                // load subroutines into asm file
                foreach (KeyValuePair<string, SubRoutineCommand> subroutine in subroutineDict)
                {
                    List<string> rendered = subroutine.Value.Render();


                    Console.WriteLine("   " + string.Join("\n   ", rendered));

                    foreach (string line in rendered)
                    {
                        writer.WriteLine(line);
                    }
                    writer.WriteLine();
                }
                writer.Close();
            }
        }

        // list abstract syntax tree location
        public void PrintStack(ContentCommand? contentCommand)
        {
            while(contentCommand != null)
            {
                Console.Write(contentCommand.ToString() + "->");
                contentCommand = contentCommand.parent;
            }
            Console.WriteLine();
        }

        internal class CompilerException : Exception
        {
            public CompilerException(string message, int line) : base($"Compiler { line }:{{{  message  }}}")
            {

            }
        }
    }
}

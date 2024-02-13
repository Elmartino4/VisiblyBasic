using Pseudocode_Interpretter;
using System.Text;
using System.Text.RegularExpressions;

namespace Cli
{
    public class Cli {
        protected Dictionary<string, CliPage> pages = new Dictionary<string, CliPage>();
        public static Cli MainInstance = new Cli();

        public static Cli GetInstance()
        {
            return MainInstance;
        }

        private Cli()
        { 
            pages.Add("error", new ErrorPage());
            pages.Add("help", new HelpPage(pages));
            pages.Add("run", new RunPage());
            pages.Add("compile", new CompilePage());
            pages.Add("exit", new ExitPage());
        }

        public string Execute(string[] args)
        {
            string output = "";
            if (!pages.ContainsKey(args[0]))
            {
                // executes the error page if the command being executed is not found
                output = pages["error"].Execute(args);
            } else
            {
                // otherwise execute the command given
                output = pages[args[0]].Execute(args);
            }


            // corrects paragraph spacing whilst printing page output
            if (!output.EndsWith("\n"))
                output += "\n";

            if (!output.EndsWith("\n\n"))
                output += "\n";

            if (!output.StartsWith("\n"))
                output = "\n" + output;
            return output;
        }

        // abstract page - used for templating other pages
        public abstract class CliPage
        {
            public abstract string Execute(string[] args);

            public abstract List<string>? HelpData();
        }

        // the default/error page, executed when the user attempts to run an unknown command
        public class ErrorPage : CliPage
        {
            // ErrorPage Text output on execution
            public override string Execute(string[] args)
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append("    could not find command \"").Append(args[0]).AppendLine("\"");
                stringBuilder.Append("    call the \"help\" command for help");

                return stringBuilder.ToString();
            }

            public override List<string>? HelpData()
            {
                // since ErrorPage implements the CliPage abstract this must be included, however should never be reached
                throw new NotImplementedException(); // intentional
            }
        }

        public class ExitPage : CliPage
        {
            public override string Execute(string[] args)
            {
                // closes the program
                Environment.Exit(0);
                return "";
            }

            public override List<string>? HelpData()
            {
                return (new string[] { "exits the shell" }).ToList();
            }
        }

        public class HelpPage : CliPage
        {
            Dictionary<string, CliPage> pages;

            // custom initialiser
            public HelpPage(Dictionary<string, CliPage> pages)
            {
                this.pages = pages;
            }

            public override List<string>? HelpData()
            {
                return (new string[] { "prints commands and options" }).ToList();
            }
    
            // loops through each command, and prints related "HelpData()" function output
            // includes many StringBuilder Commands for text formatting
            public override string Execute(string[] args)
            {
                StringBuilder output = new StringBuilder();
                foreach (KeyValuePair<string, CliPage> page in pages)
                {
                    // unless errorpage
                    if (page.Key == "error")
                        continue;

                    output.Append("\n    ").Append(page.Key);
                    List<string>? helpData = page.Value.HelpData();

                    if (helpData == null || helpData.Count == 0)
                    {
                        output.AppendLine();
                    } else
                    {
                        output.AppendLine(":");

                        foreach (string line in helpData)
                        {
                            output.Append(' ', 7).AppendLine(line); // spacing
                        }
                    }
                    
                }

                return output.Append('\n').ToString();
            }
        }

        // executes the compile command when the page is executed
        // includes a catch statement for incorrect file location
        public class CompilePage : CliPage
        {
            public override string Execute(string[] args)
            {
                Console.WriteLine();
                try
                {
                    Compiler compiler = new Compiler(args[1]);

                    compiler.RunCompile();
                }
                catch (FileNotFoundException)
                {
                    return "File \"" + args[1] + "\" not found";
                }
                

                return "";
            }

            public override List<string>? HelpData()
            {
                return new List<string>() { "compile the program",
                    "usage: compile <filename>",
                    "filename should be a .pseudo file" };
            }
        }

        // executes the given file&".asm"
        // includes a check for file not found, and for incorrect arguments
        // help data includes information about said arguments
        public class RunPage : CliPage
        {
            public override string Execute(string[] args)
            {
                bool doDeskChecks = false;
                int testDifficultyLevel = 20;
                int testCount = 10;
                int verbosity = 0;

                for (int i = 1; i < args.Length - 1; i++)
                {
                    switch (args[i].ToLower())
                    {
                        case "--desk-check":
                        case "-d": // should run desk checks
                            doDeskChecks = true;
                            break;
                        case "--difficulty":
                        case "-t": // should run N difficulty tests
                            if (i + 2 == args.Length || !Regex.IsMatch(args[i+1], @"^\d+$"))
                                return $"{i + 1}th argument should be an integer";
                            testDifficultyLevel = int.Parse(args[i + 1]);
                            i++;
                            break;
                        case "--count":
                        case "-c": // should run N tests
                            if (i + 2 == args.Length || !Regex.IsMatch(args[i + 1], @"^\d+$"))
                                return $"{i + 1}th argument should be an integer";
                            testCount = int.Parse(args[i + 1]);
                            i++;
                            break;
                        case "--verbosity":
                        case "-v": // verbosity level
                            if (i + 2 == args.Length || !Regex.IsMatch(args[i + 1], @"^\d+$"))
                                return $"{i + 1}th argument should be an integer";
                            verbosity = int.Parse(args[i + 1]);
                            i++;
                            break;
                        default:
                            return $"{i}th argument in command invalid";
                    }
                }

                Console.WriteLine();
                try
                {
                    Runtime runtime = new Runtime(args[^1], doDeskChecks, verbosity);
                    
                    int passCount = 0;
                    int failCount = 0;
                    long duration = 0;
                    List<Runtime.Results> resultsList = new List<Runtime.Results>();

                    // runs the test "testCount" times
                    for (int i = 0; i < testCount; i++)
                    {
                        runtime.Prebuild();

                        int validationFlag = runtime.SmartDataFiller(testDifficultyLevel);

                        Runtime.Results results = runtime.Execute(validationFlag, i, testDifficultyLevel);

                        if (results.pass)
                        {
                            passCount++;
                        }
                        else
                        {
                            failCount++;
                        }

                        duration += results.operations;
                        resultsList.Add(results);
                    }

                    Console.WriteLine($"Passed {passCount} of {failCount + passCount}");
                    Console.WriteLine($"took {((float)duration) / testCount:F1} operations on average per test");
                    
                    
                }
                catch (FileNotFoundException)
                {
                    return "File \"" + args[1] + "\" not found";
                }


                return "";
            }

            public override List<string>? HelpData()
            {
                return new List<string>() { "execute assembler code",
                    "usage: run <filename>",
                    "filename should be a .asm file compiled using this tool",
                    "            -d/--desk-check: prints a desk check table for each test to a .csv file in the tests directory",
                    "   -t/--difficulty [number]: changes the difficulty of the tests being executed, default 20",
                    "        -c/--count [number]: changes the number of tests being executed, default 10",
                    "   -v/--verbosity (0|1|2|3): changes the amount of output being written to the screen during tests, default 0"};
            }
        }
    }
}
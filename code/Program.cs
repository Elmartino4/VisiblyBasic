using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Pseudocode_Interpretter
{
    internal class Program
    {
        static void Main(string[] args) 
        {
            Console.WriteLine("VisiblyBasic V1.0");
            // generate an internal shell
            if (args.Length == 1 && args[0] == "shell")
            {
                BeginShell();
            }
            else if (args.Length > 0)
            {
                string output = Cli.Cli.GetInstance().Execute(args);
                Console.Write(output);
            } else
            {
                Console.WriteLine("No arguments provided, shell will be started automatically.");
                Console.WriteLine("Include the 'shell' argument to hide this message");

                BeginShell();
            }
        }

        static void BeginShell() {
            // colour the ui aesthetically using cmd colouring options
            Console.ResetColor();
            ConsoleColor foreground = Console.ForegroundColor;
            Console.WriteLine("Type \"exit\" or press Ctrl+C to quit the shell.\nType \"help\" for a list of commands.\n");

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(" >> ");
                Console.ForegroundColor = foreground;

                string? inputLine = Console.ReadLine();
                if (inputLine == null)
                    continue;

                string output = Cli.Cli.GetInstance().Execute(inputLine.Split(' '));
                Console.Write(output);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// executes assembler code
namespace Pseudocode_Interpretter
{
    internal class Runtime
    {
        static Dictionary<string, DataFiller> dataFillers = new Dictionary<string, DataFiller>()
        {
            { "sorted-search", new SortedSearchDataFiller(20) },
            { "unsorted-search", new UnsortedSearchDataFiller(20) },
            { "sorting", new SortingDataFiller(20) },
            { "generic", new GenericDataFiller() }
        };

        // command line options
        bool doDeskCheck;
        string FileName;
        int verbosity;
        DeskCheck deskCheck;

        // internal data and code values
        Dictionary<string, int> pntrs;
        List<uint> vals;
        List<string[]> allTokens;
        Dictionary<string, List<string>> metadata;

        public Runtime(string filename, bool doDeskCheck, int verbosity)
        {
            FileName = filename;
            this.doDeskCheck = doDeskCheck;
            this.verbosity = verbosity;
        }

        public void Prebuild()
        {
            allTokens = new List<string[]>();
            pntrs = new Dictionary<string, int>();
            vals = new List<uint>();
            metadata = new Dictionary<string, List<string>>();
            if (doDeskCheck)
            {
                deskCheck = new DeskCheck();
            }

            int index = 0;

            // load assembler code from .asm file
            foreach (string line in File.ReadLines("../../../tests/" + FileName + ".asm"))
            {
                // extract metadata
                if (Regex.IsMatch(line, @"^'-- (type|input|main)*: [a-zA-Z_][a-zA-Z_0-9\-]*(, [a-zA-Z_][a-zA-Z_0-9\-]*)*$"))
                {
                    string metadataLine = line.Replace("'-- ", "");
                    metadata.Add(Regex.Match(metadataLine, "^(type|input|main)").Value,
                        Regex.Matches(metadataLine, @"(?<=(: |, ))[a-zA-Z_][a-zA-Z_0-9\-]*")
                        .Cast<Match>().Select(m => m.Value).ToList()); // convert matches to a List of `string`s
                }

                string trimmedLine = line.Trim();

                trimmedLine = Regex.Replace(trimmedLine, "'.*", "");

                MatchCollection tokens = Regex.Matches(trimmedLine, ":?[a-zA-Z0-9_]+");

                string[] tokensArray = new string[tokens.Count];

                // extract tokens
                for (int i = 0; i < tokens.Count; i++)
                {
                    tokensArray[i] = tokens[i].Value;
                }

                // extract labels
                if (tokensArray.Length != 0 && tokensArray[0].StartsWith(':'))
                {
                    SetString(tokensArray[0].Substring(1), (uint)index);
                }

                if (tokensArray.Length > 0)
                {
                    allTokens.Add(tokensArray);
                    index++;
                }
            }
        }

        // fill test data using metadata
        public int SmartDataFiller(int difficulty)
        {
            if (!metadata.ContainsKey("main"))
                return DataFiller.Generic;

            // writes generic data to pointers and values
            dataFillers["generic"].FillData(pntrs, vals, metadata["main"][0]);

            if (!metadata.ContainsKey("type"))
                return DataFiller.Generic;

            if (!dataFillers.ContainsKey(metadata["type"][0]))
                return DataFiller.Generic;

            DataFiller specificDataFiller = dataFillers[metadata["type"][0]];

            if (specificDataFiller is TestDataFiller)
            {
                ((TestDataFiller)specificDataFiller).level = difficulty;
            }
            int validationFlag = specificDataFiller.FillData(pntrs, vals, metadata["main"][0]);

            return validationFlag;
        }

        public Results Execute(int validationFlag, int index, int difficulty)
        {
            Stopwatch stopwatch = new Stopwatch();

            /*
             * Uses the following instruction set:
             
                `ADD a b`			`a = a + b`
                `CSUB a b`			`a = MAX(a-b, 0)`
                `MULT a b`			`a = a * b`
                `DIV a b`			`a = FLOOR(a / b)`
                `MOD a b`			`a = a % b`
                `ATZERO a`			`a = (a == 0) ? 1 : 0`
                `OR a b`			`a = a | b`
                `AND a b`			`a = a & b`
                `XOR a b`			`a = a ^ b`
                `SAL a b`			`a = a << b`
                `SAR a b`			`a = a >> b`

                `SET *a b`      ->  *a = b    the variable a is set to the constant b
                `EQUATE *a *b`  ->  *a = *b   the variable a is set to the value of variable b
                `PNTSET *a **b` ->  *a = **b  the variable a is set to the value at variable b
                `LOCSET **a *b` -> **a = *b   the variable pointed to by variable a is set to the value of variable b

                PNTSET and LOCSET are included for use with maps, arrays, and for Turing completeness.
                they arent nessecary for simpler programs
            */

            stopwatch.Start();

            // loop through the program, from begin-subroutine to finish
            int operations = 0;
            for (int progCounter = (int)GetString("__ANCHOR_0_" + metadata["main"][0] + "__"); progCounter < allTokens.Count; progCounter++)
            {
                operations++;
                string[] line = allTokens[progCounter];

                if ((verbosity & 1) == 1)
                    Console.WriteLine(progCounter.ToString().PadLeft(3, '0') + ": EXECUTING " + string.Join(" ", line));
                

                // execute operation based on instruction
                switch (line[0])
                {
                    case "EQUATE":
                        SetString(line[1], GetString(line[2]));
                        break;
                    case "SET":
                        SetString(line[1], uint.Parse(line[2]));
                        break;
                    case "PNTSET":
                        SetString(line[1], vals[(int)GetString(line[2])]);
                        break;
                    case "LOCSET":
                        vals[(int)GetString(line[1])] = GetString(line[2]);
                        break;
                    case "ADD":
                        SetString(line[1], GetString(line[1]) + GetString(line[2]));
                        break;
                    case "CSUB":
                        SetString(line[1], GetString(line[2]) > GetString(line[1]) ? 0 : GetString(line[1]) - GetString(line[2]));
                        break;
                    case "MULT":
                        SetString(line[1], GetString(line[1]) * GetString(line[2]));
                        break;
                    case "DIV":
                        SetString(line[1], GetString(line[1]) / GetString(line[2]));
                        break;
                    case "MOD":
                        SetString(line[1], GetString(line[1]) % GetString(line[2]));
                        break;
                    case "ATZERO":
                        SetString(line[1], GetString(line[1]) == 0 ? 1u : 0);
                        break;
                    case "OR":
                        SetString(line[1], GetString(line[1]) | GetString(line[2]));
                        break;
                    case "AND":
                        SetString(line[1], GetString(line[1]) & GetString(line[2]));
                        break;
                    case "XOR":
                        SetString(line[1], GetString(line[1]) ^ GetString(line[2]));
                        break;
                    case "SAL":
                        SetString(line[1], GetString(line[1]) << (int)GetString(line[2]));
                        break;
                    case "SAR":
                        SetString(line[1], GetString(line[1]) >> (int)GetString(line[2]));
                        break;
                    case "JUMP":
                        if (GetString(line[2]) != 0)
                            progCounter = (int) GetString(line[1]);
                        break;
                    default:
                        break;
                }
                if ((verbosity & 1) == 2)
                    Console.WriteLine(DebugState());
                    
                if (deskCheck != null)
                    deskCheck.UpdateValues(pntrs, vals);
            }

            stopwatch.Stop();

            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);

            uint output = 0;
            if (metadata.ContainsKey("main") && metadata["main"] != null && metadata["main"].Count > 0)
            {
                output = GetString(Command.GenerateReference("RETURN", null, metadata["main"][0]));
            }
            

            Console.WriteLine($"Got output { output }");

            if (deskCheck != null)
            {
                File.Delete($"../../../tests/{ FileName }-{ index }.desk.csv");
                using (StreamWriter writer = new StreamWriter($"../../../tests/{FileName}-{index}.desk.csv"))
                {
                    foreach (string line in deskCheck.ToCSV())
                    {
                        writer.WriteLine(line);
                    }
                }
            }

            bool pass = true;
            if (validationFlag == DataFiller.False || validationFlag == DataFiller.True)
            {
                if (validationFlag != output)
                    pass = false;
            }
            else if (validationFlag == DataFiller.CustomCheck)
            {
                if (dataFillers[metadata["type"][0]] is not CheckedTestDataFiller)
                {
                    pass = false;
                } 
                else
                {
                    pass = ((CheckedTestDataFiller)dataFillers[metadata["type"][0]]).Validater(vals, output);
                }
            }

            return new Results(pass, operations, difficulty);
        }

        // essentially commandline-print deskchecking
        private string DebugState()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (KeyValuePair<string, int> variable in pntrs)
            {
                if (variable.Key.Contains("VAR"))
                    stringBuilder.Append(variable.Key).Append("..").Append(variable.Value).Append("=")
                        .Append(vals[variable.Value]).Append(",");
            }

            return stringBuilder.ToString();
        }

        // for manipulating variables as pointers

        private void SetString(string varName, uint value)
        {
            if (!pntrs.ContainsKey(varName))
            {
                pntrs.Add(varName, vals.Count);
                vals.Add(value);
            }
            else
            {
                vals[pntrs[varName]] = value;
            }
        }

        private uint GetString(string varName)
        {
            return vals[pntrs[varName]];
        }

        // store metadata + test configuration
        internal class Config
        {
            string mainSubroutine;
            List<Input> inputList;
            Type? returnType;
            Algorithm? algorithm;

            internal class Input
            {
                string name;
                Type type;

                public Input(string name, Type type)
                {
                    this.name = name;
                    this.type = type;
                }
            }

            public Config(string mainSubroutine, List<Input> inputList, Type? returnType, Algorithm? algorithm)
            {
                this.mainSubroutine = mainSubroutine;
                this.inputList = inputList;
                this.returnType = returnType;
                this.algorithm = algorithm;
            }

            public static Type? ParseType(string input)
            {
                switch (input.ToLower().Trim())
                {
                    case "numerical":
                        return Type.Numerical;
                    case "array":
                        return Type.Array;
                    default:
                        return null;
                }
            }

            public static Algorithm? ParseAlgorithm(string input)
            {
                switch (input.ToLower().Trim())
                {
                    case "search":
                        return Algorithm.Search;
                    case "sorting":
                        return Algorithm.Sorting;
                    case "sorted_search":
                        return Algorithm.SortedSearch;
                    default:
                        return null;
                }
            }

            public enum Type
            {
                Numerical, Array
            }

            public enum Algorithm
            {
                SortedSearch, Search, Sorting
            }
        }

        // store/retrieve test results
        internal class Results
        {
            public bool pass { private set; get; }
            public int operations { private set; get; }
            public int n { private set; get; }

            public Results(bool pass, int operations, int n)
            {
                this.pass = pass;
                this.operations = operations;
                this.n = n;
            }
        }
    }
}

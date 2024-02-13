using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pseudocode_Interpretter
{
    internal class DeskCheck
    {
        // stores checked values
        Dictionary<string, List<uint?>> variableValues = new Dictionary<string, List<uint?>>();

        // writes to checked values list using the pntrs and the vals
        public void UpdateValues(Dictionary<string, int> pntrs, List<uint> vals)
        {
            foreach (KeyValuePair<string, int> pair in pntrs)
            {
                // check that variable isnt just a temporary value
                if (pair.Key.StartsWith("__VAR_"))
                {
                    if (!variableValues.ContainsKey(pair.Key))
                    {
                        variableValues.Add(pair.Key, new List<uint?>());
                    }

                    variableValues[pair.Key].Add(vals[pair.Value]);
                }
            }
        }

        // compress and convert variableValues to comma-seperated-values file

        public List<string> ToCSV()
        {
            List<string> csv = new List<string>();
            List<string> keys = variableValues.Keys.ToList();
            Dictionary<string, uint?> previous = new Dictionary<string, uint?>();

            int length = 0;

            // convert formatted pointer names back to variable names
            StringBuilder columnNames = new StringBuilder();
            bool first = true;
            foreach (string key in keys)
            {
                previous[key] = null;
                if (variableValues[key].Count > length)
                    length = variableValues[key].Count;
                if (!first)
                    columnNames.Append(',');

                columnNames.Append(key.Replace("__VAR_", "").Replace("__", ""));
                first = false;
            }

            // add these names to the csv as a heading
            csv.Add(columnNames.ToString());

            uint?[] flattened = new uint?[variableValues.Count];
            int latestKeyIndex = -1;

            // compress "desk check" as typically formatted in davis' textbooks
            for (int i = 0; i < length; i++)
            {
                int keyIndex = 0;
                foreach (string key in keys)
                {
                    // algorithm for compression
                    int trueIndex = i - length + variableValues[key].Count;
                    uint? value = null;
                    if (trueIndex >= 0)
                    {
                        value = variableValues[key][trueIndex];
                    }

                    if (value != previous[key])
                    {
                        if (latestKeyIndex >= keyIndex)
                        {
                            // add line to lines array when no further compression is possible
                            csv.Add(DrawLine(flattened));
                            flattened = new uint?[variableValues.Count];
                        }
                        flattened[keyIndex] = value;
                        previous[key] = value;
                        latestKeyIndex = keyIndex;

                    }

                    keyIndex++;
                }
            }

            // return csv file formatted as list of strings (one string per line)
            return csv;
        }

        // convert list of numpers to a csv string
        private static string DrawLine(uint?[] flattened)
        {
            StringBuilder lineBuilder = new StringBuilder();

            for (int j = 0; j < flattened.Length; j++)
            {
                if (j != 0)
                    lineBuilder.Append(',');

                if (flattened[j] != null)
                    lineBuilder.Append(flattened[j]);
            }

            return lineBuilder.ToString();
        }
    }
}

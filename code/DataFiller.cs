using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pseudocode_Interpretter
{
    public abstract class DataFiller
    {
        public const int CustomCheck = -2;
        public const int Generic = -1;
        public const int True = 1;
        public const int False = 0;

        public abstract int FillData(Dictionary<string, int> pntrs, List<uint> vals, string main);
    }

    public class GenericDataFiller : DataFiller
    {
        // add generic constants used occassionally for fast access by pointers

        public override int FillData(Dictionary<string, int> pntrs, List<uint> vals, string main)
        {
            pntrs.Add("__CONSTANT_0__", 0 + vals.Count);
            pntrs.Add("__CONSTANT_1__", 1 + vals.Count);

            vals.AddRange(new uint[] { 0, 1 });

            return Generic;
        }
    }

    // abstract for filling executions with test data
    public abstract class TestDataFiller : DataFiller
    {
        public static Random random = new Random();
        public int level = 10;

        public TestDataFiller(int level)
        {
            this.level = level; 
        }
    }

    // for when custom data is being returned (such as a sorted array)
    public abstract class CheckedTestDataFiller : TestDataFiller
    {
        protected CheckedTestDataFiller(int level) : base(level)
        {
        }

        public abstract bool Validater(List<uint> vals, uint returnValue);
    }

    public class UnsortedSearchDataFiller : TestDataFiller
    {
        public UnsortedSearchDataFiller(int level) : base(level)
        {
        }

        public override int FillData(Dictionary<string, int> pntrs, List<uint> vals, string main)
        {
            pntrs.Add($"__PARAM_0_{main}__", 0 + vals.Count); // pointer to array of unsorted values
            pntrs.Add($"__PARAM_1_{main}__", 1 + vals.Count); // value to be found

            uint returnValue = (uint)random.Next(level);

            int count = (int)(level * 1.44269);
            vals.AddRange(new uint[] { (uint)vals.Count + 3, returnValue, (uint)count });

            int output = False;

            // fill array with some random numbers
            for (int i = 0; i < count; i++)
            {
                uint nextRandom = (uint)random.Next(level);
                if (nextRandom == returnValue)
                    output = True;
                vals.Add(nextRandom);
            }

            return output;
        }
    }

    public class SortedSearchDataFiller : TestDataFiller
    {
        public SortedSearchDataFiller(int level) : base(level)
        {
        }

        public override int FillData(Dictionary<string, int> pntrs, List<uint> vals, string main)
        {
            pntrs.Add($"__PARAM_0_{main}__", 0 + vals.Count); // pointer to the array of sorted values
            pntrs.Add($"__PARAM_1_{main}__", 1 + vals.Count); // value to be found

            int count = (int)(level * 1.44269);
            uint returnValue = (uint)random.Next(level);
            List<uint> randomVals = new List<uint>();

            int output = False;

            // fill array with random values
            for (int i = 0; i < count; i++)
            {
                uint nextRandom = (uint)random.Next(level);
                if (nextRandom == returnValue)
                    output = True;
                randomVals.Add(nextRandom);
            }

            // sort random values
            randomVals.Sort();


            // add values to prog-mem
            vals.AddRange(new uint[] { (uint)vals.Count + 3, returnValue, (uint)count });
            vals.AddRange(randomVals);

            return output;
        }
    }

    public class SortingDataFiller : CheckedTestDataFiller
    {
        public SortingDataFiller(int level) : base(level)
        {
        }

        public override int FillData(Dictionary<string, int> pntrs, List<uint> vals, string main)
        {
            pntrs.Add($"__PARAM_0_{main}__", 0 + vals.Count); // pointer to array of unsorted values

            Random random = new Random();

            int count = (int)(level * 1.44269);
            vals.AddRange(new uint[] { (uint)vals.Count + 2, (uint)count });

            int output = False;

            // fill array with some random numbers
            for (int i = 0; i < count; i++)
            {
                vals.Add((uint)random.Next(level));
            }

            return CustomCheck;
        }

        // checks if array has been sorted
        public override bool Validater(List<uint> vals, uint returnValue)
        {
            if (returnValue == 0 || returnValue >= vals.Count)
                return false;

            uint count = vals[(int)returnValue - 1];

            if (count + returnValue >= vals.Count)
                return false;

            uint previous = 0;
            for (int i = (int)returnValue; i < count + returnValue; i++)
            {
                if (vals[i] < previous)
                    return false;

                previous = vals[i];
            }

            return true;
        }
    }
}

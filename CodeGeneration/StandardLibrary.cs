using Deslang.Errors;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deslang.CodeGeneration
{ 
    public static class StandardLibrary
    {
        private static string[] _standardMethods = new string[] { "print", "printLine", "length", "discreteUniform", "continuousUniform", "exponential", "normal" };
        private static SystemRandomSource _randomSource = new SystemRandomSource();

        public static void Print(object o, string sourceLocation)
        {
            Console.Write(o);
        }

        public static void PrintLine(object o, string sourceLocation)
        {
            Console.WriteLine(o);
        }

        public static int Length(ICollection arr, string sourceLocation)
        {
            return arr.Count;
        }

        public static int DiscreteUniform(int lower, int upper, string sourceLocation)
        {
            try
            {
                DiscreteUniform discreteUniform = new DiscreteUniform(lower, upper, _randomSource);
                return discreteUniform.Sample();
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.Message} {sourceLocation}");
                Environment.Exit(1);
                return 0;
            }
        }

        public static double ContinuousUniform(double lower, double upper, string sourceLocation)
        {
            try
            {
                ContinuousUniform continuousUniform = new ContinuousUniform(lower, upper, _randomSource);
                return continuousUniform.Sample();
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.Message} {sourceLocation}");
                Environment.Exit(1);
                return 0;
            }
        }

        public static double Exponential(double rate, string sourceLocation)
        {
            try
            {
                Exponential exponential = new Exponential(rate, _randomSource);
                return exponential.Sample();
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.Message} {sourceLocation}");
                Environment.Exit(1);
                return 0;
            }         
        }

        public static double Normal(double mean, double stddev, string sourceLocation)
        {
            try
            {
                Normal normal = new Normal(mean, stddev, _randomSource);
                return normal.Sample();
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.Message} {sourceLocation}");
                Environment.Exit(1);
                return 0;
            }
        }

        public static bool IsStdlibMethod(string name)
        {
            return Array.Exists(_standardMethods, e => e == name);
        }
    }
}

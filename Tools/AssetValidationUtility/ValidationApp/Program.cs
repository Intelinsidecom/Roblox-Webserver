using System;
using System.Collections.Generic;
using System.Text;
using RobloxFiles;
using System.IO;
using RobloxAssetValidation;

namespace ValidationApp
{
    class Program
    {
        
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                {
                    //do nothing, i want this code to just check if all files exist
                }
                else
                {
                    throw new InvalidOperationException("Mentioned file(s) must exist. did you made a typo?");
                }
            }
            // now we do analysis
            for (int i = 0; i < args.Length; i++)
            {
                Analysis.AnalyzeFile(args[i]);
            }

            // perhaps update the program to work as a http service later?

        }
    }
}

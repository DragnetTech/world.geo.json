using System;
using System.Collections.Generic;
using CommandLine;

namespace combiner
{
    public class Program
    {
        [CommandLine.Verb("combine")]
        public class CombineOptions
        {
        }

        [CommandLine.Verb("uncombine")]
        public class UncombineOptions {
            
        }



        public static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<CombineOptions, UncombineOptions>(args)
                .MapResult(
                    (CombineOptions opts) => { return CombinerProcessor.Combine(opts); },
                    (UncombineOptions opts) => { return UncombineProcessor.Uncombine(opts); },
                    errs => 1
                );
        }
    }
}

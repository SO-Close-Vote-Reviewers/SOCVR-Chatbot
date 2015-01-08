using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Logging;

namespace TheCommonLibrary.CommandLine
{
    /// <summary>
    /// A self-contained task that will do the core work.
    /// </summary>
    /// <typeparam name="TOptions">The command line options that will be passed to the program. Must inherit from CommandLineOptions.</typeparam>
    public abstract class CommandLineProgram<TOptions>
        where TOptions : CommandLineOptions, new()
    {
        protected abstract int DoWork(TOptions options);

        public int RunProgram(string[] args, bool suppressInternalLogging)
        {
            TOptions options = new TOptions();
            int doWorkExitCode;

            if (!suppressInternalLogging)
                ConsoleLogger.LogLine("Parsing Command Line Arguments", ConsoleLogger.LogType.Detail);

            if (Parser.Default.ParseArguments(args, options))
            {
                if (!suppressInternalLogging)
                    ConsoleLogger.LogLine("Command Line Arguments Parsed", ConsoleLogger.LogType.Success);

                try
                {
                    if (!suppressInternalLogging)
                    {
                        ConsoleLogger.LogLine("Running Program Work", ConsoleLogger.LogType.Detail);
                        ConsoleLogger.LogLine("-----", ConsoleLogger.LogType.Detail);
                    }

                    doWorkExitCode = DoWork(options);
                }
                catch (Exception ex)
                {
                    //error happened in DoWork that was not handled, exit code 4

                    Console.Error.WriteLine("Exception in Program's Work method that was not handled.");
                    ConsoleLogger.LogException(ex);

                    return 4;
                }
            }
            else
            {
                //unable to parse options, exit code 3

                Console.Error.WriteLine("Unable to parse command line options.");

                return 3;
            }

            return doWorkExitCode;
        }
    }
}

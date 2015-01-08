using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Logging;
using TheCommonLibrary.Extensions;
using System.Reflection;
using System.Diagnostics;

namespace TheCommonLibrary.CommandLine
{
    public static class CommandLineProgramRunner
    {
        /// <summary>
        /// Runs a command line program and returns the exit code of the program.
        /// </summary>
        /// <typeparam name="TProgram">The type of the program to run.</typeparam>
        /// <typeparam name="TOptions">The type of the program's options.</typeparam>
        /// <param name="args">The raw command line arguments, which the program will parse.</param>
        /// <returns></returns>
        public static int RunProgram<TProgram, TOptions>(string[] args)
            where TProgram : CommandLineProgram<TOptions>, new()
            where TOptions : CommandLineOptions, new()
        {
            return RunProgram<TProgram, TOptions>(args, false);
        }

        /// <summary>
        /// Runs a command line program and returns the exit code of the program.
        /// </summary>
        /// <typeparam name="TProgram">The type of the program to run.</typeparam>
        /// <typeparam name="TOptions">The type of the program's options.</typeparam>
        /// <param name="args">The raw command line arguments, which the program will parse.</param>
        /// <param name="suppressInternalLogging">If true, internal start-up logging will not be shown. Exceptions will always be shown though.</param>
        /// <returns></returns>
        public static int RunProgram<TProgram, TOptions>(string[] args, bool suppressInternalLogging)
            where TProgram : CommandLineProgram<TOptions>, new()
            where TOptions : CommandLineOptions, new()
        {
            TProgram program;

            try
            {
                if (!suppressInternalLogging)
                {
                    ConsoleLogger.LogLine("Running program '{0}'".FormatSafely(typeof(TProgram).Name), ConsoleLogger.LogType.Detail);

                    //print out the assembly version of the program
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    string version = fvi.FileVersion;
                    ConsoleLogger.LogLine("Program version: " + version, ConsoleLogger.LogType.Detail);
                }

                program = new TProgram();
            }
            catch (Exception ex)
            {
                //setup failed, error code 1

                Console.Error.WriteLine("Error initializing program.");
                ConsoleLogger.LogException(ex);

                return 1;
            }

            try
            {
                int programExitCode = program.RunProgram(args, suppressInternalLogging);
                return programExitCode;
            }
            catch (Exception ex)
            {
                //failed to run program, not handled by program

                Console.Error.WriteLine("Exception thrown in program that was not handled in the program.");
                ConsoleLogger.LogException(ex);

                return 2;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;
using TheCommonLibrary.Procedure.Logging;

namespace TheCommonLibrary.Procedure
{
    public class ProcedureManager<TInputData>
    {
        private LoggingManager lm;
        private List<Procedure<TInputData>> allProcedures = new List<Procedure<TInputData>>();

        public ProcedureManager(LoggingManager loggingManager)
        {
            lm = loggingManager;
        }

        private async Task<bool> CheckIfValidProcedureAsync(Procedure<TInputData> procedure, TInputData inputData)
        {
            try
            {
                var returnMessage = await procedure.IsCorrectForThisProcedureAsync(inputData, lm);

                if (returnMessage == null)
                    lm.AddUILogMessage("Foil is appropriate for procedure '{0}'".FormatInline(procedure.GetProcedureName()), UILoggingEntryType.Success);
                else
                    lm.AddUILogMessage("Foil is not appropriate for procedure '{0}' for reason: {1}".FormatInline(procedure.GetProcedureName(), returnMessage), UILoggingEntryType.Warning);

                return returnMessage == null;
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to determine if procedure is appropriate for this data because of an unhanded error. ", ex);
            }
        }

        public void AddProcedure<TProcedure>()
            where TProcedure : Procedure<TInputData>, new()
        {
            allProcedures.Add(new TProcedure());
        }

        private async Task<bool> SomeTest()
        {
            await Task.Delay(1);
            return false;
        }

        /// <summary>
        /// Takes input data, determines which procedure to run, and optionally runs it.
        /// </summary>
        /// <param name="inputData"></param>
        /// <param name="runProcedure">If true, the found procedure, if there is one, will be ran. 
        /// If false, this method will only check which procedure can be ran.</param>
        /// <returns></returns>
        public async Task ProcessAsync(TInputData inputData, bool runProcedure)
        {
            lm.ClearList();

            lm.AddUILogMessage("Starting Process", UILoggingEntryType.Header);

            //find the procedure to run
            lm.AddUILogMessage("Determining procedure to run", UILoggingEntryType.Header);

            List<Procedure<TInputData>> possibleProcedures;

            try
            {
                var tasks = allProcedures
                    .Select(async x => new
                    {
                        Procedure = x,
                        IsValid = await CheckIfValidProcedureAsync(x, inputData)
                    })
                    .ToList();

                var results = await Task.WhenAll(tasks);

                possibleProcedures = results
                    .Where(x => x.IsValid)
                    .Select(x => x.Procedure)
                    .ToList();
            }
            catch (Exception ex) //should only hit here if there was a manually thrown exception in CheckIfValidProcedure
            {
                //just print out the error message and all inner exception messages
                lm.AddUILogMessage(ex.FullErrorMessage(" | "), UILoggingEntryType.Error);
                lm.AddFileLogMessage(ex.FullErrorMessage(Environment.NewLine));
                return;
            }

            if (possibleProcedures.Count == 0)
            {
                lm.AddUILogMessage("There are no procedures that accept the given data", UILoggingEntryType.Error, true);
                return;
            }

            if (possibleProcedures.Count > 1)
            {
                string message = "There is more than one procedure that accepts the given data. Procedures needs to be tuned to have more strict requirements.";
                lm.AddUILogMessage(message, UILoggingEntryType.Error, true);
                return;
            }

            //else - count is 1
            var procedureToRun = possibleProcedures.Single();
            lm.AddUILogMessage("Process to run is: " + procedureToRun.GetProcedureName(), UILoggingEntryType.Information, true);

            if (runProcedure)
            {
                //now, run that procedure
                try
                {
                    lm.AddUILogMessage("Beginning procedure", UILoggingEntryType.Header, true);
                    await procedureToRun.RunProcedureAsync(inputData, lm);
                    lm.AddUILogMessage("Procedure completed", UILoggingEntryType.Success, true);
                }
                catch (Exception ex)
                {
                    lm.AddUILogMessage("Unable to complete the procedure. " + ex.FullErrorMessage(" | "), UILoggingEntryType.Error);
                    lm.AddFileLogMessage("Unable to complete the procedure. " + Environment.NewLine + ex.FullErrorMessage(Environment.NewLine));
                    return;
                }
            }
            else
            {
                lm.AddUILogMessage("Not running procedure", UILoggingEntryType.Information, true);
            }
        }
    }
}

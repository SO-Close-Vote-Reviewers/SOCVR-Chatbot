using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Procedure.Logging;

namespace TheCommonLibrary.Procedure
{
    public abstract class Procedure<TInputData>
    {
        /// <summary>
        /// Determines if this procedure is appropriate for the given foil data. Does not do any work.
        /// Returns text indicating why the procedure is incorrect, or null if the procedure is correct.
        /// </summary>
        /// <param name="topBarcodeData"></param>
        /// <param name="foilIdOnFoil"></param>
        /// <param name="newFoilId"></param>
        /// <param name="wafermapFileContents"></param>
        /// <returns></returns>
        public abstract Task<string> IsCorrectForThisProcedureAsync(TInputData inputData, LoggingManager lm);

        public abstract Task RunProcedureAsync(TInputData inputData, LoggingManager lm);

        public abstract string GetProcedureName();
    }
}

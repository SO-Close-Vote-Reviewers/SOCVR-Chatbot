using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Procedure.FormControls
{
    public abstract class UserInputProvider<TInputData>
    {
        /// <summary>
        /// Returns the names of the fields that the user must input.
        /// </summary>
        /// <returns></returns>
        public abstract List<string> GetInputFields();

        /// <summary>
        /// Given the raw user input, parse it into the output data object
        /// </summary>
        /// <param name="rawInputs"></param>
        /// <returns></returns>
        public abstract TInputData ParseUserInput(Dictionary<string, string> rawInputs);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheCommonLibrary.Procedure.FormControls;

namespace TheCommonLibrary.Procedure
{
    public class ProcedureProgramRunner<TUserInputProvider, TInputData>
        where TUserInputProvider : UserInputProvider<TInputData>, new()
    {
        private frmProcedureInterface<TUserInputProvider, TInputData> form;

        /// <summary>
        /// Creates a new ProcedureProgramRunner with a given name and file path to a help file.
        /// </summary>
        /// <param name="procedureProgramName">The name of the program. Will be displayed as the form's title.</param>
        /// <param name="htmlHelpFilePath">A path to the html file to display when a user clicks the help button.</param>
        public ProcedureProgramRunner(string procedureProgramName, string htmlHelpFilePath)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new frmProcedureInterface<TUserInputProvider,TInputData>(procedureProgramName, htmlHelpFilePath);
        }

        public void AddProcedure<TProcedure>()
            where TProcedure : Procedure<TInputData>, new()
        {
            form.AddProcedure<TProcedure>();
        }

        /// <summary>
        /// Launches the Procedure Interface form which will control all future actions. 
        /// Make sure you call this AFTER adding the procedures you want to use.
        /// </summary>
        /// <typeparam name="TUserInputControl"></typeparam>
        /// <typeparam name="TInputData"></typeparam>
        public void RunProcedureProgram()
        {
            form.ShowDialog();
        }
    }
}

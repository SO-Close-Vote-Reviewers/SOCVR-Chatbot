using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheCommonLibrary.Procedure.Logging;
using TheCommonLibrary.Extensions;
using System.Diagnostics;

namespace TheCommonLibrary.Procedure.FormControls
{
    public partial class frmProcedureInterface<TUserInputProvider, TInputData> : Form
        where TUserInputProvider : UserInputProvider<TInputData>, new()
    {
        LoggingManager loggingManager;
        ProcedureManager<TInputData> procedureManager;
        TUserInputProvider inputProvider;
        List<InputField> inputFields;
        string htmlHelpFilePath;

        /// <summary>
        /// Creates a new ProcedureInterface form.
        /// </summary>
        /// <param name="formTitle">The name for the title of the window.</param>
        /// <param name="helpFilePath">The path to the html help file.</param>
        public frmProcedureInterface(string formTitle, string helpFilePath)
        {
            InitializeComponent();
            InitializeForm();

            this.Text = formTitle;
            htmlHelpFilePath = helpFilePath;
        }

        public void AddProcedure<TProcedure>()
            where TProcedure : Procedure<TInputData>, new()
        {
            procedureManager.AddProcedure<TProcedure>();
        }

        private void InitializeForm()
        {
            loggingManager = new LoggingManager(olvStatus);
            loggingManager.AddFileLogMessage("UI started");

            procedureManager = new ProcedureManager<TInputData>(loggingManager);
            inputProvider = new TUserInputProvider();

            //get the fields to create
            var fieldNames = inputProvider.GetInputFields();

            //make and place all the input fields
            inputFields = new List<InputField>();

            var fieldNamesWithIndexes = fieldNames
                .Select((x, i) => new { Field = x, Index = i });

            foreach (var field in fieldNamesWithIndexes)
            {
                var newField = new InputField();
                newField.FieldName = field.Field;
                newField.Location = new Point(0, (newField.Height * field.Index) + (6 * field.Index));
                newField.Width = pnlUserInput.Width;
                newField.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                inputFields.Add(newField);
                pnlUserInput.Controls.Add(newField);
            }

            //added all input controls, resize the input panel
            var lowestControl = inputFields
                .OrderByDescending(x=>x.Location.Y) //order by the Y pos
                .First(); //get the control that is at the bottom

            var totalInputControlHeight = lowestControl.Location.Y + lowestControl.Height;
            grpParameters.Height = totalInputControlHeight + 22;

            //move the bottom panel to match with the input panel's position and size
            pnlActionContainer.Location = new Point(pnlActionContainer.Location.X, grpParameters.Location.Y + grpParameters.Height + 6);
            pnlActionContainer.Height = this.Height - pnlActionContainer.Location.Y - 50;

            //give focus to first input
            inputFields.First().Focus();
        }

        private async void btnProcess_Click(object sender, EventArgs e)
        {
            loggingManager.AddFileLogMessage("-------------------");
            loggingManager.AddFileLogMessage("Process button click");
            EnableInputs(false);

            TInputData inputData;

            try
            {
                loggingManager.AddFileLogMessage("Gathering inputs");

                var rawInputs = inputFields
                    .ToDictionary(x => x.FieldName, x => x.FieldValue);

                inputData = inputProvider.ParseUserInput(rawInputs);

                foreach (var input in rawInputs)
                {
                    loggingManager.AddFileLogMessage("{0}: '{1}'".FormatInline(input.Key, input.Value));
                }
            }
            catch (Exception ex)
            {
                loggingManager.AddUILogMessage("Unable to gather input. Reason: " + ex.FullErrorMessage(" | "), UILoggingEntryType.Error);
                loggingManager.AddFileLogMessage("Unable to gather input. Reason:");
                loggingManager.AddFileLogMessage(ex.FullErrorMessage());
                btnClear.Enabled = true;
                return;
            }

            await procedureManager.ProcessAsync(inputData, chkDoProcedure.Checked);
            loggingManager.AddFileLogMessage("Procedure manager done, process complete");

            btnClear.Enabled = true;
        }

        private void EnableInputs(bool enable)
        {
            btnProcess.Enabled = enable;
            btnClear.Enabled = enable;
            chkDoProcedure.Enabled = enable;

            foreach (var inputField in inputFields)
            {
                inputField.Enabled = enable;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            EnableInputs(true);
            loggingManager.ClearList();

            foreach (var inputField in inputFields)
            {
                inputField.FieldValue = "";
            }

            inputFields.First().Focus();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            Process.Start(htmlHelpFilePath);
        }

        private void btnOpenLogFolder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "Logs");
        }
    }
}

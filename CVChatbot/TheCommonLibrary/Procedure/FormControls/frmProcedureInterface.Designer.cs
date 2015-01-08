namespace TheCommonLibrary.Procedure.FormControls
{
    partial class frmProcedureInterface<TUserInputProvider, TInputData>
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlUserInput = new System.Windows.Forms.Panel();
            this.grpParameters = new System.Windows.Forms.GroupBox();
            this.pnlActionContainer = new System.Windows.Forms.Panel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.olvStatus = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.chkDoProcedure = new System.Windows.Forms.CheckBox();
            this.btnProcess = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnOpenLogFolder = new System.Windows.Forms.Button();
            this.grpParameters.SuspendLayout();
            this.pnlActionContainer.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlUserInput
            // 
            this.pnlUserInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlUserInput.Location = new System.Drawing.Point(3, 16);
            this.pnlUserInput.Name = "pnlUserInput";
            this.pnlUserInput.Size = new System.Drawing.Size(524, 130);
            this.pnlUserInput.TabIndex = 10;
            // 
            // grpParameters
            // 
            this.grpParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpParameters.Controls.Add(this.pnlUserInput);
            this.grpParameters.Location = new System.Drawing.Point(12, 41);
            this.grpParameters.Name = "grpParameters";
            this.grpParameters.Size = new System.Drawing.Size(530, 149);
            this.grpParameters.TabIndex = 11;
            this.grpParameters.TabStop = false;
            this.grpParameters.Text = "Parameters";
            // 
            // pnlActionContainer
            // 
            this.pnlActionContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlActionContainer.Controls.Add(this.groupBox4);
            this.pnlActionContainer.Controls.Add(this.chkDoProcedure);
            this.pnlActionContainer.Controls.Add(this.btnProcess);
            this.pnlActionContainer.Controls.Add(this.btnClear);
            this.pnlActionContainer.Location = new System.Drawing.Point(12, 196);
            this.pnlActionContainer.Name = "pnlActionContainer";
            this.pnlActionContainer.Size = new System.Drawing.Size(530, 286);
            this.pnlActionContainer.TabIndex = 12;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.olvStatus);
            this.groupBox4.Location = new System.Drawing.Point(3, 55);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(524, 228);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Process Status";
            // 
            // olvStatus
            // 
            this.olvStatus.AllColumns.Add(this.olvColumn2);
            this.olvStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn2});
            this.olvStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvStatus.EmptyListMsg = "Current Status Messages Will Appear Here";
            this.olvStatus.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.olvStatus.Location = new System.Drawing.Point(3, 16);
            this.olvStatus.Name = "olvStatus";
            this.olvStatus.ShowGroups = false;
            this.olvStatus.Size = new System.Drawing.Size(518, 209);
            this.olvStatus.TabIndex = 0;
            this.olvStatus.UseCompatibleStateImageBehavior = false;
            this.olvStatus.View = System.Windows.Forms.View.Details;
            // 
            // olvColumn2
            // 
            this.olvColumn2.AspectName = "Message";
            this.olvColumn2.FillsFreeSpace = true;
            this.olvColumn2.Text = "Message";
            // 
            // chkDoProcedure
            // 
            this.chkDoProcedure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDoProcedure.AutoSize = true;
            this.chkDoProcedure.Checked = true;
            this.chkDoProcedure.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDoProcedure.Location = new System.Drawing.Point(435, 32);
            this.chkDoProcedure.Name = "chkDoProcedure";
            this.chkDoProcedure.Size = new System.Drawing.Size(92, 17);
            this.chkDoProcedure.TabIndex = 9;
            this.chkDoProcedure.Text = "Do Procedure";
            this.chkDoProcedure.UseVisualStyleBackColor = true;
            // 
            // btnProcess
            // 
            this.btnProcess.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnProcess.Location = new System.Drawing.Point(438, 3);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(89, 23);
            this.btnProcess.TabIndex = 8;
            this.btnProcess.Text = "Process";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(3, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(89, 23);
            this.btnClear.TabIndex = 10;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHelp.Location = new System.Drawing.Point(485, 12);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(57, 23);
            this.btnHelp.TabIndex = 13;
            this.btnHelp.Text = "Help";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnOpenLogFolder
            // 
            this.btnOpenLogFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenLogFolder.Location = new System.Drawing.Point(372, 12);
            this.btnOpenLogFolder.Name = "btnOpenLogFolder";
            this.btnOpenLogFolder.Size = new System.Drawing.Size(107, 23);
            this.btnOpenLogFolder.TabIndex = 14;
            this.btnOpenLogFolder.Text = "Open Log Folder";
            this.btnOpenLogFolder.UseVisualStyleBackColor = true;
            this.btnOpenLogFolder.Click += new System.EventHandler(this.btnOpenLogFolder_Click);
            // 
            // frmProcedureInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 494);
            this.Controls.Add(this.btnOpenLogFolder);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.pnlActionContainer);
            this.Controls.Add(this.grpParameters);
            this.HelpButton = true;
            this.Name = "frmProcedureInterface";
            this.Text = "Form1";
            this.grpParameters.ResumeLayout(false);
            this.pnlActionContainer.ResumeLayout(false);
            this.pnlActionContainer.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvStatus)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlUserInput;
        private System.Windows.Forms.GroupBox grpParameters;
        private System.Windows.Forms.Panel pnlActionContainer;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.GroupBox groupBox4;
        private BrightIdeasSoftware.ObjectListView olvStatus;
        private BrightIdeasSoftware.OLVColumn olvColumn2;
        private System.Windows.Forms.CheckBox chkDoProcedure;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnOpenLogFolder;
    }
}
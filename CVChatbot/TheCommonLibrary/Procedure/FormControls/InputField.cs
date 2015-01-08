using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TheCommonLibrary.Procedure.FormControls
{
    public partial class InputField : UserControl
    {
        public InputField()
        {
            InitializeComponent();
        }

        public string FieldName
        {
            get { return lblFieldName.Text; }
            set
            {
                lblFieldName.Text = value;
                RepositionTextBox();
            }
        }

        public string FieldValue
        {
            get { return txtFieldValue.Text; }
            set
            {
                txtFieldValue.Text = value;
                RepositionTextBox();
            }
        }

        private void RepositionTextBox()
        {
            txtFieldValue.Location = new Point(lblFieldName.Width + 6, 0);
            txtFieldValue.Width = this.Width - txtFieldValue.Location.X;
        }
    }
}

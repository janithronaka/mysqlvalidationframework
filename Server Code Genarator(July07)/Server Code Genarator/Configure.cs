using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server_Code_Genarator
{
    public partial class Configure : Form
    {
        public Configure()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnSetup_Click(object sender, EventArgs e)
        {
            Form1 frm =  new Form1();
            frm.server =new DBConnect(txtUser.Text,txtPass.Text);
            frm.Visible = true;
            this.Visible = false;
        }

        private void txtUser_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 13)
            {
                btnSetup.PerformClick();
            }
        }
    }
}

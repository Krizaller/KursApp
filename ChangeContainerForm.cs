using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KursApp
{
    public partial class ChangeContainerForm : Form {

        public string IDContainer;

        public ChangeContainerForm()
        {
            InitializeComponent();
        }
        public ChangeContainerForm(string IDContainer){
            InitializeComponent();
            this.IDContainer = IDContainer;
            textBox1.Text = IDContainer;
        }

        private void button1_Click(object sender, EventArgs e){
            IDContainer = textBox1.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace KursApp
{
    public partial class LoadPresetSettings : Form{
        List<string> SettingsName;
        public string SettingsNameChoised;
        public LoadPresetSettings(List<string> SettingsName)
        {
            InitializeComponent();
            this.SettingsName = SettingsName;
            Init();


        }
        private void Init() {
            for (int i = 0; i < SettingsName.Count; i++) {
                dataGridView1.Rows.Add(SettingsName[i]);
            }  
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SettingsNameChoised = label2.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            label2.Text = SettingsName[dataGridView1.CurrentRow.Index];
        }
    }
}

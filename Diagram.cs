using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace KursApp
{
    public partial class Diagram : Form
    {
        public Diagram(String[] VisiterName,int[] MaxPulse, int[] TimeTraning)
        {
            InitializeComponent();
            //S0 - MaxPulse
            //S1 - Timr Traning
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.ChartAreas[0].Axes[0].CustomLabels.Clear();
            for (int i = 0; i < VisiterName.Length; i++){
                this.chart1.ChartAreas[0].Axes[0].CustomLabels.Add(i,i+2,VisiterName[i]);
                this.chart1.Series[0].Points.Add(MaxPulse[i]);
                this.chart1.Series[1].Points.Add(TimeTraning[i]);
            }
        }


        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}


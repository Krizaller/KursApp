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
    public partial class ChangeSensorForm : Form {

        public string sensor_id,sensor_name, sensor_type_value;
        public double sensor_value;
        public double[] sensor_minmax;

        public ChangeSensorForm() {
            InitializeComponent();
        }
        public ChangeSensorForm(string sensor_id,
            string sensor_name,
            string sensor_type_value,
            double sensor_value,
            double[] sensor_minmax)
        {
            this.sensor_id = sensor_id;
            this.sensor_name = sensor_name;
            this.sensor_type_value = sensor_type_value;
            this.sensor_value = sensor_value;
            this.sensor_minmax = sensor_minmax;

            InitializeComponent();
            Init();
        }

        private void Init(){
            textBox1.Text = sensor_id.ToString();
            textBox2.Text = sensor_name;
            textBox4.Text = sensor_type_value;  
            textBox5.Text = sensor_value.ToString();
            textBox6.Text = sensor_minmax[0].ToString();
            textBox7.Text = sensor_minmax[1].ToString();
        }


        private void button1_Click(object sender, EventArgs e){
            sensor_id = textBox1.Text;
            sensor_name = textBox2.Text;
            sensor_type_value = textBox4.Text;
            sensor_value = Convert.ToDouble(textBox5.Text);
            sensor_minmax[0] = Convert.ToDouble(textBox6.Text);
            sensor_minmax[1] = Convert.ToDouble(textBox7.Text);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

    }
}

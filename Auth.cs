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
    public partial class Auth : Form
    {
        DatabaseHelper db = new DatabaseHelper("localhost", "dbcontainermonitoring", "root", "");
        public Auth()
        {
            InitializeComponent();
        }

        private void button14_Click(object sender, EventArgs e){

            if (ValidNameOperator())
            {
                int operatorid = CheckOperator();
                if (operatorid != -1){
                    MessageBox.Show("Доступ разрешен", "Доступ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Hide();
                    using (var form = new Form1(operatorid)){
                        form.ShowDialog(); 
                    }
                    //DialogResult a = new Form1(operatorid).ShowDialog();
                    this.Close();
                }
                else{
                    MessageBox.Show("Нет такого человека", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }
        private int CheckOperator()
        {
            string name = $"{textBox1.Text}-{textBox2.Text}-{textBox3.Text}";
            int OperatorId = db.GetOperator(name.ToLower());
            return OperatorId;
        }

        private bool ValidNameOperator()
        {
            TextBox[] textboxs = { textBox1, textBox2, textBox3 };
            foreach (var textbox in textboxs)
            {
                if (textbox.Text.Length <= 1)
                {
                    MessageBox.Show("Не может быть меньше 1 буквы", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }
    }
}

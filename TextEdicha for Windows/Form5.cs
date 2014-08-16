using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEdicha_Windows
{
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.url = textBox1.Text;
            this.Close();
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.url == "")
            {
                textBox1.Text = "http://";
            }
            else
            {
                textBox1.Text = Properties.Settings.Default.url;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                Properties.Settings.Default.url = textBox1.Text;
                this.Close();
            }
        }
    }
}

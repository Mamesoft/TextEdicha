using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEdicha_Windows
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Regex imgurl = new Regex(@"(http://gyazo\.com/|http://myazo\.net/|http://g\.81\.la/)([0-9a-f]{32})(?:\\.png)?");
            MatchCollection url_match = imgurl.Matches(Form1.logtext);
            foreach (System.Text.RegularExpressions.Match m in url_match)
            {
                toolStripComboBox1.Items.Add(m.Groups[1].Value + m.Groups[2].Value);
            }
            if (toolStripComboBox1.Items.Count > 0)
            {
                toolStripComboBox1.SelectedIndex = 0;
            }
            else
            {
                if (Form1.japanese)
                {
                    MessageBox.Show("画像が含まれた発言が見つかりません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("I can't find comments with pictures.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                this.Close();
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.ImageLocation = toolStripComboBox1.SelectedItem.ToString() + ".png";
            string imageurl = Regex.Replace(toolStripComboBox1.SelectedItem.ToString(), "[.]", "\\.");

            Regex comment_r = new Regex("\n(.*?)" + imageurl + "(?:\\.png)(.*?)\r\n");
            Match comment_match = comment_r.Match(Form1.logtext);
            this.Invoke((MethodInvoker)delegate()
            {
                if (Form1.japanese)
                {
                    toolStripStatusLabel1.Text = comment_match.Groups[1].Value + "[画像]" + comment_match.Groups[2].Value;
                }
                else
                {
                    toolStripStatusLabel1.Text = comment_match.Groups[1].Value + "[Picture]" + comment_match.Groups[2].Value;
                }
            });
        }
    }
}
//[0-9a-f]{32})(?:\\.png)?
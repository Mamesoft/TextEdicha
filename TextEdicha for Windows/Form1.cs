using SocketIOClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;


namespace TextEdicha_Windows
{
    public partial class Form1 : Form
    {


        static Client socket;
        public static Hashtable user = new Hashtable();
        public static bool japanese;
        int activenum;
        int romnum;


        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.Name.StartsWith("ja")){
                japanese = true;
            }else{
                japanese = false;
            }
            if(Properties.Settings.Default.font != null){
                textBox1.Font = Properties.Settings.Default.font;
                textBox1.ForeColor = Properties.Settings.Default.fontcolor;
                textBox2.Font = Properties.Settings.Default.font;
                textBox2.ForeColor = Properties.Settings.Default.fontcolor;
            }

            if (Properties.Settings.Default.wordwrap)
            {
                textBox1.WordWrap = true;
                右端で折り返すToolStripMenuItem.Checked = true;
            }
            else
            {
                textBox1.WordWrap = false;
                右端で折り返すToolStripMenuItem.Checked = false;
            }

            if (Properties.Settings.Default.url == "")
            {
                Form5 form5 = new Form5();
                form5.ShowDialog();
                Socket_Connect(Properties.Settings.Default.url);
            }
            else
            {
                Socket_Connect(Properties.Settings.Default.url);
            }
            if(Properties.Settings.Default.sound)
            {
                サウンドToolStripMenuItem.Checked = true;
            }
        }

        
        private void Socket_Connect(string url)
        {
            if (Regex.IsMatch(url,@"^s?https?://[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+$"))
            {
                
                var headers = new NameValueCollection();
                Version OSver = Environment.OSVersion.Version;
                string os = "Windows NT " + OSver.Major + "." + OSver.Minor;
                if (Environment.Is64BitOperatingSystem){
                    os = os + "; WOW64";
                }
                string framework = System.Environment.Version.ToString();
                headers.Add("user-agent", "TextEdicha/" + ProductVersion + " (" + os + ") .NET Framework/" + framework + " (Mamesoft Web)" );
                
                socket = new Client(url, headers);


                socket.On("connect", (fn) =>
                {
                    Part reg = new Part() { mode = "client", lastid = 1 };
                    socket.Emit("register", reg);
                });

                socket.On("disconnect", (fn) =>
                {
                    DialogResult result = MessageBox.Show("サーバーから切断されました。", "エラー", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
                    socket.Close();
                    if (result == DialogResult.Abort)
                    {
                        Form5 form5 = new Form5();
                        form5.ShowDialog();
                        Socket_Connect(Properties.Settings.Default.url);
                    }
                    if (result == DialogResult.Retry)
                    {
                        Socket_Connect(Properties.Settings.Default.url);
                    }
                });

                socket.On("userinfo", (data) =>
                {
                    Part userinfo = data.Json.GetFirstArgAs<Part>();
                    if (userinfo.rom == true)
                    {
                        this.Invoke((MethodInvoker)delegate()
                        {
                            入室ToolStripMenuItem.Visible = true;
                            退室ToolStripMenuItem.Visible = false;
                        });
                    }
                    else
                    {
                        this.Invoke((MethodInvoker)delegate()
                        {
                            入室ToolStripMenuItem.Visible = false;
                            退室ToolStripMenuItem.Visible = true;
                        });
                    }



                });

                socket.On("log", (data) =>
                {
                    string jsonstring = data.Json.ToJsonString();

                    Regex r_log = new Regex(@"\x22args\x22:\x5b(.*?),\x22_id\x22:\x22.*?\x22\x7d\x5d");
                    Match log_match = r_log.Match(jsonstring);
                    string log = log_match.Groups[1].Value + ",";


                    Regex r_name = new Regex("\"name\":\"(.*?)\",");
                    Match name_match = r_name.Match(log);
                    string name = name_match.Groups[1].Value;

                    Regex r_comment = new Regex("\"comment\":\"(.*?)\",");
                    Match comment_match = r_comment.Match(log);
                    string comment = comment_match.Groups[1].Value;

                    Regex r_time = new Regex("\"time\":\"(.*?)\",");
                    Match time_match = r_time.Match(log);
                    DateTime t = DateTime.Parse(time_match.Groups[1].Value);
                    string time = t.ToString();

                    Regex r_ip = new Regex("\"ip\":\"(.*?)\",");
                    Match ip_match = r_ip.Match(log);
                    string ip = ip_match.Groups[1].Value;

                    Regex r_channels = new Regex(@"\x22channel\x22:\x5b(.*?)\x5d");
                    Match channels_match = r_channels.Match(log);
                    string channels = channels_match.Groups[1].Value + ",";

                    Regex r_channel = new Regex(@"\x22(.*?)\x22,");
                    MatchCollection channel_match = r_channel.Matches(channels);

                    string channel;
                    string channelstring;
                    channelstring = "";
                    foreach (System.Text.RegularExpressions.Match n in channel_match)
                    {
                        channel = n.Groups[1].Value;
                        channelstring = " #" + channel + channelstring;

                    }

                    string speech = name + "> " + comment + channelstring + " (" + time + ", " + ip + ")";
                    if (Properties.Settings.Default.sound)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                    }

                    this.Invoke((MethodInvoker)delegate()
                    {
                        textBox1.Text = System.Environment.NewLine + speech + textBox1.Text;

                    });
                });

                socket.On("init", (data) =>
                {
                    string jsonstring = data.Json.ToJsonString();


                    Regex r_logs = new Regex(@"\x5b\x7b\x22logs\x22:\x5b(.*)\x5d\x7d\x5d");
                    Match logs_match = r_logs.Match(jsonstring);
                    string logs = logs_match.Groups[1].Value + ",";
                    Regex r_log = new Regex(@"\x7b(.*?),\x22_id\x22:\x22.*?\x22\x7d,");
                    MatchCollection log_match = r_log.Matches(logs);
                    int log_num = log_match.Count;
                    string log;
                    string[] name = new string[log_num];
                    string[] comment = new string[log_num];
                    string[] time = new string[log_num];
                    string[] ip = new string[log_num];
                    string[] channelstring = new string[log_num];
                    int i = 0;
                    foreach (System.Text.RegularExpressions.Match m in log_match)
                    {
                        log = m.Groups[1].Value;

                        Regex r_name = new Regex("\"name\":\"(.*?)\"");
                        Match name_match = r_name.Match(log);
                        name[i] = name_match.Groups[1].Value;

                        Regex r_comment = new Regex("\"comment\":\"(.*?)\"");
                        Match comment_match = r_comment.Match(log);
                        comment[i] = comment_match.Groups[1].Value;

                        Regex r_time = new Regex("\"time\":\"(.*?)\"");
                        Match time_match = r_time.Match(log);
                        DateTime t = DateTime.Parse(time_match.Groups[1].Value);
                        time[i] = t.ToString();



                        Regex r_ip = new Regex("\"ip\":\"(.*?)\"");
                        Match ip_match = r_ip.Match(log);
                        ip[i] = ip_match.Groups[1].Value;

                        Regex r_channels = new Regex(@"\x22channel\x22:\x5b(.*?)\x5d");
                        Match channels_match = r_channels.Match(log);
                        string channels = channels_match.Groups[1].Value + ",";



                        Regex r_channel = new Regex(@"\x22(.*?)\x22,");
                        MatchCollection channel_match = r_channel.Matches(channels);

                        string channel;
                        channelstring[i] = "";
                        foreach (System.Text.RegularExpressions.Match n in channel_match)
                        {
                            channel = n.Groups[1].Value;
                            channelstring[i] = " #" + channel + channelstring[i];

                        }


                        i++;
                    }


                    for (int j = log_num - 1; j >= 0; j--)
                    {
                        string speech = name[j] + "> " + comment[j] + channelstring[j] + " (" + time[j] + ", " + ip[j] + ")";
                        this.Invoke((MethodInvoker)delegate()
                        {
                            textBox1.Text = System.Environment.NewLine + speech + textBox1.Text;
                        });
                    }

                });


                socket.On("users", (data) =>
                {
                    string jsonstring = data.Json.ToJsonString();

                    Regex r_users = new Regex(@"\x5b\x7b\x22users\x22:\x5b(.*)\x5d,\x22roms\x22:([0-9]*),\x22actives\x22:([0-9]*)\x7d\x5d");
                    Match users_match = r_users.Match(jsonstring);
                    string users = users_match.Groups[1].Value + ",";
                    romnum = int.Parse(users_match.Groups[2].Value);
                    activenum = int.Parse(users_match.Groups[3].Value);

                    numreload();
                    Regex r_user = new Regex(@"\x7b(.*?),\x22ua\x22:\x22.*?\x22\x7d,");
                    MatchCollection user_match = r_user.Matches(users);
                    string userjson;
                    foreach (System.Text.RegularExpressions.Match m in user_match)
                    {
                        userjson = m.Groups[1].Value;

                        Regex r_id = new Regex("\"id\":([0-9]*?),");
                        Match id_match = r_id.Match(userjson);

                        Regex r_rom = new Regex("\"rom\":(.*?),");
                        Match rom_match = r_rom.Match(userjson);

                        Regex r_ip = new Regex("\"ip\":\"(.*?)\"");
                        Match ip_match = r_ip.Match(userjson);

                        Regex r_name = new Regex("\"name\":\"(.*?)\"");
                        Match name_match = r_name.Match(userjson);

                        if (name_match.Groups[1].Value != "")
                        {
                            user[id_match.Groups[1].Value] = name_match.Groups[1].Value + " (" + ip_match.Groups[1].Value + ")";
                        }
                        else
                        {
                            user[id_match.Groups[1].Value] = "ROM (" + ip_match.Groups[1].Value + ")";
                        }


                    }
                });


                socket.On("newuser", (data) =>
                {
                    Part newuser = data.Json.GetFirstArgAs<Part>();
                    if (newuser.rom)
                    {
                        user[newuser.id] = "ROM (" + newuser.ip + ")";
                        romnum++;
                    }
                    else
                    {
                        user[newuser.id] = newuser.name + " (" + newuser.ip + ")";
                        activenum++;
                    }
                    numreload();
                });

                socket.On("inout", (data) =>
                {
                    Part inout = data.Json.GetFirstArgAs<Part>();
                    if (inout.rom)
                    {
                        user[inout.id] = "ROM (" + inout.ip + ")";
                        activenum--;
                        romnum++;
                    }
                    else
                    {
                        if (user[inout.id] == null)
                        {
                            activenum++;
                        }
                        else
                        {
                            activenum++;
                            romnum--;
                        }
                        user[inout.id] = inout.name + " (" + inout.ip + ")";
                    }
                    numreload();
                });
                socket.On("deluser", (data) =>
                {
                    string jsonstring = data.Json.ToJsonString();

                    Regex r_del = new Regex(@"\x22args\x22:\x5b([0-9]*)\x5d");
                    Match del_match = r_del.Match(jsonstring);
                    if (Regex.IsMatch(del_match.Groups[1].Value, "^ROM"))
                    {
                        romnum--;
                    }
                    else
                    {
                        activenum--;
                    }

                    user.Remove(del_match.Groups[1].Value);
                    numreload();
                });

                socket.Connect();
            }
            else
            {
                MessageBox.Show("正しいSocket接続先URLを入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Form5 form5 = new Form5();
                form5.ShowDialog();
                Socket_Connect(Properties.Settings.Default.url);
            }
        }

        private void 入室ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }


        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox3.Text == "")
                {
                    Part say = new Part() { comment = textBox2.Text };
                    socket.Emit("say", say);
                }
                else
                {
                    Part say = new Part() { comment = textBox2.Text, channel = textBox3.Text};
                    socket.Emit("say", say);
                }
                textBox2.Text = "";
            }
            else if (e.KeyCode == Keys.S && e.Control)
            {
                textBox2.Text = "[s]" + textBox2.Text;
            }
            else if (e.KeyCode == Keys.D && e.Control)
            {
                textBox2.Text = "[small]" + textBox2.Text;
            }
            else if (e.KeyCode == Keys.I && e.Control)
            {
                if (入室ToolStripMenuItem.Visible)
                {
                    Form2 form2 = new Form2();
                    form2.Show();
                }
                else
                {
                    inout(Properties.Settings.Default.name);
                }
            }
            else if (e.KeyCode == Keys.L && e.Control)
            {
                Form4 form4 = new Form4();
                form4.Show();
            }
            else if (e.KeyCode == Keys.M && e.Control)
            {
                Form3 form3 = new Form3();
                form3.Show();
            }
        }

        private void 退室ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            inout(Properties.Settings.Default.name);
        }

        private void toolStripTextBox1_Leave(object sender, EventArgs e)
        {
            toolStripTextBox1.BackColor = SystemColors.MenuBar;
            toolStripTextBox1.BorderStyle = BorderStyle.None;
            toolStripTextBox1.Text = "";
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                inout(toolStripTextBox1.Text);
                toolStripTextBox1.Text = "";
                textBox2.Focus();
            }
        }

        private void toolStripTextBox1_Enter(object sender, EventArgs e)
        {
            toolStripTextBox1.BackColor = SystemColors.Window;
            toolStripTextBox1.BorderStyle = BorderStyle.Fixed3D;
            toolStripTextBox1.Text = Properties.Settings.Default.name;
        }

        public static void inout(string data)
        {
            Part inout = new Part() { name = data };
            socket.Emit("inout", inout);
            Properties.Settings.Default.name = data;
        }

        private void Form1_ClientSizeChanged(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                textBox2.Width = ClientSize.Width - 20;
                toolStripTextBox1.Width = ClientSize.Width - 355;
                textBox3.Width = toolStripStatusLabel2.Width;
                textBox3.Top = ClientSize.Height - 42;
                //562
            });
        }

        private void フォントToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FontDialog fd = new FontDialog();

            //初期のフォントを設定
            fd.Font = textBox1.Font;
            //初期の色を設定
            fd.Color = textBox1.ForeColor;
            //ユーザーが選択できるポイントサイズの最大値を設定する
            fd.MaxSize = 30;
            fd.MinSize = 6;
            //存在しないフォントやスタイルをユーザーが選択すると
            //エラーメッセージを表示する
            fd.FontMustExist = true;
            //横書きフォントだけを表示する
            fd.AllowVerticalFonts = false;
            //色を選択できるようにする
            fd.ShowColor = true;
            //取り消し線、下線、テキストの色などのオプションを指定可能にする
            //デフォルトがTrueのため必要はない
            fd.ShowEffects = true;
            //固定ピッチフォント以外も表示する
            //デフォルトがFalseのため必要はない
            fd.FixedPitchOnly = false;
            //ベクタ フォントを選択できるようにする
            //デフォルトがTrueのため必要はない
            fd.AllowVectorFonts = true;

            //ダイアログを表示する
            if (fd.ShowDialog() != DialogResult.Cancel)
            {
                //TextBox1のフォントと色を変える
                textBox1.Font = fd.Font;
                textBox1.ForeColor = fd.Color;
                textBox2.Font = fd.Font;
                textBox2.ForeColor = fd.Color;
                Properties.Settings.Default.font = fd.Font;
                Properties.Settings.Default.fontcolor = fd.Color;

            }

        }
        public static string logtext;
        private void 画像ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logtext = textBox1.Text;
            Form3 form3 = new Form3();
            form3.Show();
        }

        private void 右端で折り返すToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                if (textBox1.WordWrap)
                {
                    textBox1.WordWrap = false;
                    右端で折り返すToolStripMenuItem.Checked = false;
                    Properties.Settings.Default.wordwrap = false;
                }
                else
                {
                    textBox1.WordWrap = true;
                    右端で折り返すToolStripMenuItem.Checked = true;
                    Properties.Settings.Default.wordwrap = false;
                }
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            textBox3.BackColor = SystemColors.Window;
            textBox3.BorderStyle = BorderStyle.Fixed3D;
            textBox3.Top = ClientSize.Height - 47;
            toolStripStatusLabel1.Text = "#";
            textBox3.Left = 18;
            textBox3.Width = toolStripStatusLabel2.Width;

        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            textBox3.BackColor = SystemColors.Control;
            textBox3.BorderStyle = BorderStyle.None;
            textBox3.Top = ClientSize.Height - 42;
            if (textBox3.Text != "")
            {
                toolStripStatusLabel1.Text = "#";
                textBox3.Left = 18;
                textBox3.Width = toolStripStatusLabel2.Width;
                //18, 520
            }
            else
            {
                toolStripStatusLabel1.Text = "";
                textBox3.Left = 0;
                textBox3.Width = toolStripStatusLabel2.Width;
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                textBox2.Focus();
            }
        }

        private void numreload()
        {
            this.Invoke((MethodInvoker)delegate()
            {
                toolStripStatusLabel3.Text = activenum + " R" + romnum;
                

            });
        }

        private void ユーザーリストToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 form4 = new Form4();
            form4.Show();
        }

        private void オプションToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string oldurl = Properties.Settings.Default.url;
            Form5 form5 = new Form5();
            form5.ShowDialog();
            if (oldurl != Properties.Settings.Default.url)
            {
                socket.Close();
                textBox1.Text = System.Environment.NewLine;
                Socket_Connect(Properties.Settings.Default.url);
            }
        }

        private void バージョン情報ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TextEdicha for Windows Ver." + ProductVersion, "バージョン情報", MessageBoxButtons.OK);
        }

        private void サウンドToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.sound)
            {
                Properties.Settings.Default.sound = false;
                サウンドToolStripMenuItem.Checked = false;

            }
            else
            {
                Properties.Settings.Default.sound = true;
                サウンドToolStripMenuItem.Checked = true;
            }

        }

    }
}

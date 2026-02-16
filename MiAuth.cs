using AntdUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WackeClient.date;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WackeClient
{
    public partial class MiAuth : AntdUI.Window
    {
        public MiAuth()
        {
            InitializeComponent();
        }
        private void MiAuth_Load(object sender, EventArgs e)
        {
            inputblob.Text = MyData.Blob;
        }
        private void buttoncopy_Click(object sender, EventArgs e)
        {
            try
            {
                string text = inputblob.Text;
                Clipboard.SetText(text);
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"失败", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string text = Clipboard.GetText();
                inputsign.Text = text;
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"失败", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            

        }

        private void buttonauth_Click(object sender, EventArgs e)
        {
            try
            {
                string text = inputsign.Text;
                int bytecount = Encoding.UTF8.GetByteCount(text);
                if (!Regex.IsMatch(text, @"\s") && bytecount == 256)
                {
                    MyData.Sign = text;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    DialogResult ressult = AntdUI.Modal.open(new AntdUI.Modal.Config(this, "严重警告", "当前Sign格式异常！！！\n是否继续操作")
                    {
                        Icon = TType.Warn,
                        //内边距
                        Padding = new Size(24, 20),
                    });
                    if (ressult == DialogResult.OK)
                    {
                        MyData.Sign = text;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"失败", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }
    }
}

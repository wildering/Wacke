namespace WackeClient
{
    partial class Login
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            AntdUI.Tabs.StyleLine styleLine1 = new AntdUI.Tabs.StyleLine();
            this.pageHeader = new AntdUI.PageHeader();
            this.progress = new AntdUI.Progress();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabs1 = new AntdUI.Tabs();
            this.tabPage3 = new AntdUI.TabPage();
            this.image3D1 = new AntdUI.Image3D();
            this.buttonlogin = new AntdUI.Button();
            this.checkbox = new AntdUI.Checkbox();
            this.inputuserna = new AntdUI.Input();
            this.inputpassword = new AntdUI.Input();
            this.tabPage4 = new AntdUI.TabPage();
            this.inputnotice = new AntdUI.Input();
            this.divider1 = new AntdUI.Divider();
            this.groupBox1.SuspendLayout();
            this.tabs1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // pageHeader
            // 
            this.pageHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pageHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pageHeader.Icon = ((System.Drawing.Image)(resources.GetObject("pageHeader.Icon")));
            this.pageHeader.IconRatio = 1.1F;
            this.pageHeader.Loading = true;
            this.pageHeader.LocalizationSubText = "";
            this.pageHeader.Location = new System.Drawing.Point(0, 0);
            this.pageHeader.Margin = new System.Windows.Forms.Padding(4);
            this.pageHeader.MaximizeBox = false;
            this.pageHeader.MDI = true;
            this.pageHeader.Name = "pageHeader";
            this.pageHeader.ShowButton = true;
            this.pageHeader.ShowIcon = true;
            this.pageHeader.Size = new System.Drawing.Size(800, 25);
            this.pageHeader.SubText = "Login";
            this.pageHeader.TabIndex = 0;
            this.pageHeader.Text = "Wacke";
            // 
            // progress
            // 
            this.progress.Animation = 10;
            this.progress.Fill = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(64)))), ((int)(((byte)(92)))));
            this.progress.Location = new System.Drawing.Point(4, 572);
            this.progress.Name = "progress";
            this.progress.Radius = 5;
            this.progress.Shape = AntdUI.TShapeProgress.Default;
            this.progress.ShowInTaskbar = true;
            this.progress.ShowTextDot = 1;
            this.progress.Size = new System.Drawing.Size(792, 25);
            this.progress.TabIndex = 15;
            this.progress.Text = " ";
            this.progress.UseTextCenter = true;
            this.progress.ValueRatio = 1.2F;
            this.progress.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tabs1);
            this.groupBox1.Location = new System.Drawing.Point(4, 32);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(792, 536);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            // 
            // tabs1
            // 
            this.tabs1.Centered = true;
            this.tabs1.Controls.Add(this.tabPage3);
            this.tabs1.Controls.Add(this.tabPage4);
            this.tabs1.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabs1.Fill = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.tabs1.FillActive = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.tabs1.FillHover = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.tabs1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.tabs1.Location = new System.Drawing.Point(8, 12);
            this.tabs1.Name = "tabs1";
            this.tabs1.Pages.Add(this.tabPage3);
            this.tabs1.Pages.Add(this.tabPage4);
            this.tabs1.Size = new System.Drawing.Size(776, 518);
            this.tabs1.Style = styleLine1;
            this.tabs1.TabIndex = 2;
            this.tabs1.Text = "tabs";
            this.tabs1.TypExceed = AntdUI.TabTypExceed.None;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.image3D1);
            this.tabPage3.Controls.Add(this.buttonlogin);
            this.tabPage3.Controls.Add(this.checkbox);
            this.tabPage3.Controls.Add(this.inputuserna);
            this.tabPage3.Controls.Add(this.inputpassword);
            this.tabPage3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPage3.Location = new System.Drawing.Point(3, 39);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(770, 476);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "登录";
            // 
            // image3D1
            // 
            this.image3D1.Image = ((System.Drawing.Image)(resources.GetObject("image3D1.Image")));
            this.image3D1.Location = new System.Drawing.Point(62, 74);
            this.image3D1.Name = "image3D1";
            this.image3D1.Size = new System.Drawing.Size(356, 332);
            this.image3D1.TabIndex = 0;
            this.image3D1.Text = "image3D";
            // 
            // buttonlogin
            // 
            this.buttonlogin.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.buttonlogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.buttonlogin.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.buttonlogin.BorderWidth = 1F;
            this.buttonlogin.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.buttonlogin.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.buttonlogin.Enabled = false;
            this.buttonlogin.IconSvg = "LoginOutlined";
            this.buttonlogin.Location = new System.Drawing.Point(622, 284);
            this.buttonlogin.Name = "buttonlogin";
            this.buttonlogin.Size = new System.Drawing.Size(100, 32);
            this.buttonlogin.TabIndex = 4;
            this.buttonlogin.Text = "Login";
            this.buttonlogin.Click += new System.EventHandler(this.buttonlogin_Click);
            // 
            // checkbox
            // 
            this.checkbox.Fill = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.checkbox.Location = new System.Drawing.Point(472, 236);
            this.checkbox.Name = "checkbox";
            this.checkbox.Size = new System.Drawing.Size(200, 25);
            this.checkbox.TabIndex = 3;
            this.checkbox.Text = "记住我的嘴脸";
            // 
            // inputuserna
            // 
            this.inputuserna.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputuserna.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputuserna.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputuserna.Location = new System.Drawing.Point(472, 135);
            this.inputuserna.Name = "inputuserna";
            this.inputuserna.PlaceholderText = "用户";
            this.inputuserna.PrefixSvg = "UserOutlined";
            this.inputuserna.SelectionColor = System.Drawing.Color.White;
            this.inputuserna.Size = new System.Drawing.Size(250, 32);
            this.inputuserna.TabIndex = 2;
            // 
            // inputpassword
            // 
            this.inputpassword.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputpassword.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputpassword.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputpassword.Location = new System.Drawing.Point(472, 187);
            this.inputpassword.Name = "inputpassword";
            this.inputpassword.PasswordChar = '*';
            this.inputpassword.PlaceholderText = "密码";
            this.inputpassword.PrefixSvg = "KeyOutlined";
            this.inputpassword.SelectionColor = System.Drawing.Color.White;
            this.inputpassword.Size = new System.Drawing.Size(250, 32);
            this.inputpassword.TabIndex = 1;
            this.inputpassword.UseSystemPasswordChar = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.inputnotice);
            this.tabPage4.Controls.Add(this.divider1);
            this.tabPage4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPage4.Location = new System.Drawing.Point(3, 39);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(770, 476);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "关于";
            // 
            // inputnotice
            // 
            this.inputnotice.AutoScroll = true;
            this.inputnotice.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputnotice.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputnotice.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputnotice.Location = new System.Drawing.Point(16, 32);
            this.inputnotice.Multiline = true;
            this.inputnotice.Name = "inputnotice";
            this.inputnotice.PlaceholderText = "等待获取公告";
            this.inputnotice.ReadOnly = true;
            this.inputnotice.Size = new System.Drawing.Size(751, 453);
            this.inputnotice.TabIndex = 5;
            // 
            // divider1
            // 
            this.divider1.ColorSplit = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.divider1.Location = new System.Drawing.Point(16, 3);
            this.divider1.Name = "divider1";
            this.divider1.Size = new System.Drawing.Size(751, 23);
            this.divider1.TabIndex = 4;
            this.divider1.Text = "版本更新公告";
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.pageHeader);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Login";
            this.Resizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "LoginForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Login_FormClosed);
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.tabs1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private AntdUI.PageHeader pageHeader;
        private AntdUI.Progress progress;
        private System.Windows.Forms.GroupBox groupBox1;
        private AntdUI.Tabs tabs1;
        private AntdUI.TabPage tabPage3;
        private AntdUI.Image3D image3D1;
        private AntdUI.Button buttonlogin;
        private AntdUI.Checkbox checkbox;
        private AntdUI.Input inputuserna;
        private AntdUI.Input inputpassword;
        private AntdUI.TabPage tabPage4;
        private AntdUI.Input inputnotice;
        private AntdUI.Divider divider1;
    }
}
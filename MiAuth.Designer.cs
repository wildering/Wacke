namespace WackeClient
{
    partial class MiAuth
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MiAuth));
            this.pageHeader = new AntdUI.PageHeader();
            this.label1 = new AntdUI.Label();
            this.inputblob = new AntdUI.Input();
            this.label2 = new AntdUI.Label();
            this.inputsign = new AntdUI.Input();
            this.buttonauth = new AntdUI.Button();
            this.buttoncopy = new AntdUI.Button();
            this.button1 = new AntdUI.Button();
            this.SuspendLayout();
            // 
            // pageHeader
            // 
            this.pageHeader.DividerColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.pageHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pageHeader.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pageHeader.Icon = ((System.Drawing.Image)(resources.GetObject("pageHeader.Icon")));
            this.pageHeader.IconRatio = 1F;
            this.pageHeader.Location = new System.Drawing.Point(0, 0);
            this.pageHeader.Margin = new System.Windows.Forms.Padding(4);
            this.pageHeader.MaximizeBox = false;
            this.pageHeader.MDI = true;
            this.pageHeader.Name = "pageHeader";
            this.pageHeader.ShowButton = true;
            this.pageHeader.ShowIcon = true;
            this.pageHeader.Size = new System.Drawing.Size(450, 29);
            this.pageHeader.SubFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pageHeader.SubGap = 2;
            this.pageHeader.SubText = "";
            this.pageHeader.TabIndex = 1;
            this.pageHeader.Text = "Wacke Auth";
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.BadgeSvg = "";
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.IconRatio = 0F;
            this.label1.Location = new System.Drawing.Point(8, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(4);
            this.label1.Name = "label1";
            this.label1.PrefixSvg = "";
            this.label1.Size = new System.Drawing.Size(41, 33);
            this.label1.SuffixSvg = "";
            this.label1.TabIndex = 12;
            this.label1.Text = "Blob：";
            // 
            // inputblob
            // 
            this.inputblob.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputblob.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputblob.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputblob.JoinMode = AntdUI.TJoinMode.Left;
            this.inputblob.Location = new System.Drawing.Point(44, 38);
            this.inputblob.Name = "inputblob";
            this.inputblob.PlaceholderText = "";
            this.inputblob.ReadOnly = true;
            this.inputblob.Size = new System.Drawing.Size(358, 32);
            this.inputblob.TabIndex = 32;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.SystemColors.Control;
            this.label2.BadgeSvg = "";
            this.label2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.IconRatio = 0F;
            this.label2.Location = new System.Drawing.Point(8, 78);
            this.label2.Margin = new System.Windows.Forms.Padding(4);
            this.label2.Name = "label2";
            this.label2.PrefixSvg = "";
            this.label2.Size = new System.Drawing.Size(41, 33);
            this.label2.SuffixSvg = "";
            this.label2.TabIndex = 33;
            this.label2.Text = "Sign：";
            // 
            // inputsign
            // 
            this.inputsign.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputsign.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputsign.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.inputsign.Location = new System.Drawing.Point(44, 79);
            this.inputsign.Multiline = true;
            this.inputsign.Name = "inputsign";
            this.inputsign.PlaceholderText = "";
            this.inputsign.Size = new System.Drawing.Size(388, 221);
            this.inputsign.TabIndex = 34;
            // 
            // buttonauth
            // 
            this.buttonauth.BorderWidth = 1F;
            this.buttonauth.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.buttonauth.Location = new System.Drawing.Point(332, 306);
            this.buttonauth.Name = "buttonauth";
            this.buttonauth.Size = new System.Drawing.Size(100, 32);
            this.buttonauth.TabIndex = 45;
            this.buttonauth.Text = "授权";
            this.buttonauth.Click += new System.EventHandler(this.buttonauth_Click);
            // 
            // buttoncopy
            // 
            this.buttoncopy.BorderWidth = 1F;
            this.buttoncopy.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.buttoncopy.IconSvg = "CopyOutlined";
            this.buttoncopy.JoinMode = AntdUI.TJoinMode.Right;
            this.buttoncopy.Location = new System.Drawing.Point(400, 38);
            this.buttoncopy.Name = "buttoncopy";
            this.buttoncopy.Size = new System.Drawing.Size(32, 32);
            this.buttoncopy.TabIndex = 46;
            this.buttoncopy.Click += new System.EventHandler(this.buttoncopy_Click);
            // 
            // button1
            // 
            this.button1.BorderWidth = 1F;
            this.button1.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.button1.Location = new System.Drawing.Point(44, 306);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 32);
            this.button1.TabIndex = 47;
            this.button1.Text = "粘贴";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MiAuth
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 350);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttoncopy);
            this.Controls.Add(this.buttonauth);
            this.Controls.Add(this.inputsign);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.inputblob);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pageHeader);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MiAuth";
            this.Resizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MiAuth";
            this.Load += new System.EventHandler(this.MiAuth_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private AntdUI.PageHeader pageHeader;
        private AntdUI.Label label1;
        private AntdUI.Input inputblob;
        private AntdUI.Label label2;
        private AntdUI.Input inputsign;
        private AntdUI.Button buttonauth;
        private AntdUI.Button buttoncopy;
        private AntdUI.Button button1;
    }
}
namespace Control
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.luffer = new System.Windows.Forms.Label();
            this.heightlabel = new System.Windows.Forms.Label();
            this.rotationlabel = new System.Windows.Forms.Label();
            this.luffertextBox = new System.Windows.Forms.TextBox();
            this.heighttextBox = new System.Windows.Forms.TextBox();
            this.rotationtextBox = new System.Windows.Forms.TextBox();
            this.consolelist = new System.Windows.Forms.ListBox();
            this.startbutton = new System.Windows.Forms.Button();
            this.stopbutton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // luffer
            // 
            this.luffer.AutoSize = true;
            this.luffer.Location = new System.Drawing.Point(1, 15);
            this.luffer.Name = "luffer";
            this.luffer.Size = new System.Drawing.Size(53, 12);
            this.luffer.TabIndex = 0;
            this.luffer.Text = "变幅机构";
            // 
            // heightlabel
            // 
            this.heightlabel.AutoSize = true;
            this.heightlabel.Location = new System.Drawing.Point(0, 95);
            this.heightlabel.Name = "heightlabel";
            this.heightlabel.Size = new System.Drawing.Size(53, 12);
            this.heightlabel.TabIndex = 1;
            this.heightlabel.Text = "卷扬机构";
            // 
            // rotationlabel
            // 
            this.rotationlabel.AutoSize = true;
            this.rotationlabel.Location = new System.Drawing.Point(0, 55);
            this.rotationlabel.Name = "rotationlabel";
            this.rotationlabel.Size = new System.Drawing.Size(53, 12);
            this.rotationlabel.TabIndex = 2;
            this.rotationlabel.Text = "回转机构";
            // 
            // luffertextBox
            // 
            this.luffertextBox.Location = new System.Drawing.Point(60, 12);
            this.luffertextBox.Name = "luffertextBox";
            this.luffertextBox.Size = new System.Drawing.Size(100, 21);
            this.luffertextBox.TabIndex = 3;
            // 
            // heighttextBox
            // 
            this.heighttextBox.Location = new System.Drawing.Point(60, 92);
            this.heighttextBox.Name = "heighttextBox";
            this.heighttextBox.Size = new System.Drawing.Size(100, 21);
            this.heighttextBox.TabIndex = 4;
            // 
            // rotationtextBox
            // 
            this.rotationtextBox.Location = new System.Drawing.Point(60, 52);
            this.rotationtextBox.Name = "rotationtextBox";
            this.rotationtextBox.Size = new System.Drawing.Size(100, 21);
            this.rotationtextBox.TabIndex = 5;
            // 
            // consolelist
            // 
            this.consolelist.FormattingEnabled = true;
            this.consolelist.ItemHeight = 12;
            this.consolelist.Location = new System.Drawing.Point(186, 12);
            this.consolelist.Name = "consolelist";
            this.consolelist.Size = new System.Drawing.Size(389, 340);
            this.consolelist.TabIndex = 6;
            // 
            // startbutton
            // 
            this.startbutton.Location = new System.Drawing.Point(46, 184);
            this.startbutton.Name = "startbutton";
            this.startbutton.Size = new System.Drawing.Size(75, 23);
            this.startbutton.TabIndex = 7;
            this.startbutton.Text = "start";
            this.startbutton.UseVisualStyleBackColor = true;
            this.startbutton.Click += new System.EventHandler(this.startbutton_Click);
            // 
            // stopbutton
            // 
            this.stopbutton.Location = new System.Drawing.Point(46, 232);
            this.stopbutton.Name = "stopbutton";
            this.stopbutton.Size = new System.Drawing.Size(75, 23);
            this.stopbutton.TabIndex = 8;
            this.stopbutton.Text = "stop";
            this.stopbutton.UseVisualStyleBackColor = true;
            this.stopbutton.Click += new System.EventHandler(this.stopbutton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 460);
            this.Controls.Add(this.stopbutton);
            this.Controls.Add(this.startbutton);
            this.Controls.Add(this.consolelist);
            this.Controls.Add(this.rotationtextBox);
            this.Controls.Add(this.heighttextBox);
            this.Controls.Add(this.luffertextBox);
            this.Controls.Add(this.rotationlabel);
            this.Controls.Add(this.heightlabel);
            this.Controls.Add(this.luffer);
            this.Name = "Form1";
            this.Text = "Control";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label luffer;
        private System.Windows.Forms.Label heightlabel;
        private System.Windows.Forms.Label rotationlabel;
        private System.Windows.Forms.TextBox luffertextBox;
        private System.Windows.Forms.TextBox heighttextBox;
        private System.Windows.Forms.TextBox rotationtextBox;
        private System.Windows.Forms.ListBox consolelist;
        private System.Windows.Forms.Button startbutton;
        private System.Windows.Forms.Button stopbutton;
    }
}


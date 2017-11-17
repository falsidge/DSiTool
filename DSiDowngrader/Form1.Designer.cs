namespace DSiDowngrader
{
    partial class Form1
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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.ConsoleID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.button6 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.completed = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(35, 210);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(264, 23);
            this.progressBar1.TabIndex = 2;
            // 
            // ConsoleID
            // 
            this.ConsoleID.Location = new System.Drawing.Point(77, 23);
            this.ConsoleID.Name = "ConsoleID";
            this.ConsoleID.Size = new System.Drawing.Size(172, 20);
            this.ConsoleID.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Console ID";
            // 
            // CID
            // 
            this.CID.Location = new System.Drawing.Point(44, 98);
            this.CID.Name = "CID";
            this.CID.Size = new System.Drawing.Size(255, 20);
            this.CID.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "CID";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(88, 124);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(150, 25);
            this.button4.TabIndex = 12;
            this.button4.Text = "Extract from CID.bin";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.CID_Extract);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 187);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "NAND";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(88, 179);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(150, 25);
            this.button6.TabIndex = 16;
            this.button6.Text = "Decrypt/Encrypt";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.crypt_NAND);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(88, 49);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 25);
            this.button1.TabIndex = 0;
            this.button1.Text = "Extract from DSiware export (.bin)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ConsoleID_Extract);
            // 
            // completed
            // 
            this.completed.AutoSize = true;
            this.completed.ForeColor = System.Drawing.Color.Lime;
            this.completed.Location = new System.Drawing.Point(128, 236);
            this.completed.Name = "completed";
            this.completed.Size = new System.Drawing.Size(54, 13);
            this.completed.TabIndex = 17;
            this.completed.Text = "Complete!";
            this.completed.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 262);
            this.Controls.Add(this.completed);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.CID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ConsoleID);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "DSiTool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox ConsoleID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox CID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label completed;
    }
}


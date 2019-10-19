namespace SiMay.RemoteMonitor.Controls
{
    partial class ShellManager
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
            this.txtCommandLine = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtCommandLine
            // 
            this.txtCommandLine.BackColor = System.Drawing.Color.Black;
            this.txtCommandLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCommandLine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCommandLine.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCommandLine.ForeColor = System.Drawing.Color.White;
            this.txtCommandLine.Location = new System.Drawing.Point(0, 0);
            this.txtCommandLine.MaxLength = 2147483647;
            this.txtCommandLine.Multiline = true;
            this.txtCommandLine.Name = "txtCommandLine";
            this.txtCommandLine.Size = new System.Drawing.Size(780, 449);
            this.txtCommandLine.TabIndex = 1;
            this.txtCommandLine.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // ShellManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(780, 449);
            this.Controls.Add(this.txtCommandLine);
            this.Name = "ShellManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "cmd";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShellForm_FormClosing);
            this.Load += new System.EventHandler(this.ShellForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCommandLine;
    }
}
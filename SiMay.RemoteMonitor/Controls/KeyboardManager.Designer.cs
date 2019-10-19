namespace SiMay.RemoteMonitor.Controls
{
    partial class KeyboardManager
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
            this.txtKey = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtKey
            // 
            this.txtKey.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtKey.Location = new System.Drawing.Point(0, 0);
            this.txtKey.Multiline = true;
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(486, 327);
            this.txtKey.TabIndex = 0;
            // 
            // KeyboardManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 327);
            this.Controls.Add(this.txtKey);
            this.Name = "KeyboardManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KeyboardForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.KeyboardForm_FormClosing);
            this.Load += new System.EventHandler(this.KeyboardForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtKey;
    }
}
using System;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Application
{
    partial class ScreenApplication
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
            this.imgDesktop = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.imgDesktop)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imgDesktop
            // 
            this.imgDesktop.BackColor = System.Drawing.Color.Black;
            this.imgDesktop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgDesktop.Location = new System.Drawing.Point(0, 0);
            this.imgDesktop.Name = "imgDesktop";
            this.imgDesktop.Size = new System.Drawing.Size(953, 651);
            this.imgDesktop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgDesktop.TabIndex = 7;
            this.imgDesktop.TabStop = false;
            this.imgDesktop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.desktopImg_MouseDown);
            this.imgDesktop.MouseMove += new System.Windows.Forms.MouseEventHandler(this.m_desktop_MouseMove);
            this.imgDesktop.MouseUp += new System.Windows.Forms.MouseEventHandler(this.desktopImg_MouseUp);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.imgDesktop);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(953, 651);
            this.panel1.TabIndex = 8;
            // 
            // ScreenManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(953, 651);
            this.Controls.Add(this.panel1);
            this.Name = "ScreenManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ScreenSpyForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScreenSpyForm_FormClosing);
            this.Load += new System.EventHandler(this.ScreenSpyForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScreenSpyForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ScreenSpyForm_KeyUp);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.ScreenManager_MouseWheel);
            ((System.ComponentModel.ISupportInitialize)(this.imgDesktop)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox imgDesktop;
        private System.Windows.Forms.Panel panel1;
    }
}
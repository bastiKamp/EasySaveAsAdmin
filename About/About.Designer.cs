using System.ComponentModel;

namespace EasySaveAsAdmin.About
{
    partial class About
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.aboutText = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // aboutText
            // 
            this.aboutText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.aboutText.BackColor = System.Drawing.SystemColors.Window;
            this.aboutText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.aboutText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.aboutText.Location = new System.Drawing.Point(12, 12);
            this.aboutText.Name = "aboutText";
            this.aboutText.ReadOnly = true;
            this.aboutText.Size = new System.Drawing.Size(360, 237);
            this.aboutText.TabIndex = 0;
            this.aboutText.Text = "Easy Save as Admin\n\n------------------------------------------\n\nSave files in pat" + "hs that require admin rights without restart notepad++\n\nVersion: 0.1.0\nAuthor: b" + "kamp";
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 261);
            this.Controls.Add(this.aboutText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "About";
            this.Text = "About";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.RichTextBox aboutText;

        #endregion
    }
}
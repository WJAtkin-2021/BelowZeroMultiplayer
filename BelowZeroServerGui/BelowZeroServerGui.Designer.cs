namespace BelowZeroServerGui
{
    partial class BelowZeroServerGui
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BelowZeroServerGui));
            this.ConsoleOutput = new System.Windows.Forms.ListBox();
            this.CommandTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SendCmdButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ConsoleOutput
            // 
            this.ConsoleOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConsoleOutput.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ConsoleOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConsoleOutput.ForeColor = System.Drawing.SystemColors.Info;
            this.ConsoleOutput.FormattingEnabled = true;
            this.ConsoleOutput.HorizontalExtent = 9999;
            this.ConsoleOutput.HorizontalScrollbar = true;
            this.ConsoleOutput.ItemHeight = 16;
            this.ConsoleOutput.Location = new System.Drawing.Point(12, 12);
            this.ConsoleOutput.Name = "ConsoleOutput";
            this.ConsoleOutput.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.ConsoleOutput.Size = new System.Drawing.Size(710, 500);
            this.ConsoleOutput.TabIndex = 0;
            // 
            // CommandTextBox
            // 
            this.CommandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CommandTextBox.Location = new System.Drawing.Point(40, 528);
            this.CommandTextBox.Name = "CommandTextBox";
            this.CommandTextBox.Size = new System.Drawing.Size(601, 22);
            this.CommandTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 527);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(22, 22);
            this.label1.TabIndex = 2;
            this.label1.Text = ">";
            // 
            // SendCmdButton
            // 
            this.SendCmdButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SendCmdButton.Location = new System.Drawing.Point(647, 527);
            this.SendCmdButton.Name = "SendCmdButton";
            this.SendCmdButton.Size = new System.Drawing.Size(75, 23);
            this.SendCmdButton.TabIndex = 3;
            this.SendCmdButton.Text = "Send CMD";
            this.SendCmdButton.UseVisualStyleBackColor = true;
            this.SendCmdButton.Click += new System.EventHandler(this.SendCmdButton_Click);
            // 
            // BelowZeroServerGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 561);
            this.Controls.Add(this.SendCmdButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CommandTextBox);
            this.Controls.Add(this.ConsoleOutput);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(750, 600);
            this.Name = "BelowZeroServerGui";
            this.Text = "Subnautica Below Zero Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox ConsoleOutput;
        private System.Windows.Forms.TextBox CommandTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button SendCmdButton;
    }
}


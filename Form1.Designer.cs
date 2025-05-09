namespace RefreshRateSwitch
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            HzLabel = new Label();
            RefreshRateLabel = new Label();
            startupToggle = new Panel();
            startupLabel = new Label();
            SuspendLayout();
            // 
            // HzLabel
            // 
            HzLabel.AutoSize = true;
            HzLabel.BackColor = Color.FromArgb(128, 255, 128);
            HzLabel.BorderStyle = BorderStyle.FixedSingle;
            HzLabel.Cursor = Cursors.Hand;
            HzLabel.Location = new Point(232, 9);
            HzLabel.Name = "HzLabel";
            HzLabel.Size = new Size(31, 22);
            HzLabel.TabIndex = 0;
            HzLabel.Text = "HZ";
            HzLabel.Click += HzLabel_Click;
            // 
            // RefreshRateLabel
            // 
            RefreshRateLabel.AutoSize = true;
            RefreshRateLabel.Location = new Point(12, 9);
            RefreshRateLabel.Name = "RefreshRateLabel";
            RefreshRateLabel.Size = new Size(214, 20);
            RefreshRateLabel.TabIndex = 1;
            RefreshRateLabel.Text = "Refresh Rate (Press to Change):";
            // 
            // startupToggle
            // 
            startupToggle.BackColor = Color.LightGray;
            startupToggle.BorderStyle = BorderStyle.FixedSingle;
            startupToggle.Location = new Point(232, 43);
            startupToggle.Name = "startupToggle";
            startupToggle.Size = new Size(50, 25);
            startupToggle.TabIndex = 0;
            startupToggle.Click += ToggleStartupSetting;
            // 
            // startupLabel
            // 
            startupLabel.AutoSize = true;
            startupLabel.Location = new Point(12, 43);
            startupLabel.Name = "startupLabel";
            startupLabel.Size = new Size(113, 20);
            startupLabel.TabIndex = 1;
            startupLabel.Text = "Start on Startup";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(311, 81);
            Controls.Add(startupToggle);
            Controls.Add(startupLabel);
            Controls.Add(RefreshRateLabel);
            Controls.Add(HzLabel);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Form1";
            ShowInTaskbar = false;
            Text = "RefreshRateSwitch";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion


        private Label HzLabel;
        private Label RefreshRateLabel;
        private System.Windows.Forms.Panel startupToggle;
        private System.Windows.Forms.Label startupLabel;
        private List<int> availableRefreshRates;
        private int currentRateIndex;
    }
}

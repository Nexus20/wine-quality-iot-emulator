namespace IoTEmulator
{
    partial class EmulatorMainForm
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
            addEmulatorButton = new Button();
            deviceIdInputTextBox = new TextBox();
            deviceKeyInputTextBox = new TextBox();
            deviceIdInputLabel = new Label();
            deviceKeyInputLabel = new Label();
            emulatorsDataGridView = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)emulatorsDataGridView).BeginInit();
            SuspendLayout();
            // 
            // addEmulatorButton
            // 
            addEmulatorButton.Location = new Point(958, 25);
            addEmulatorButton.Name = "addEmulatorButton";
            addEmulatorButton.Size = new Size(136, 73);
            addEmulatorButton.TabIndex = 0;
            addEmulatorButton.Text = "Add emulator";
            addEmulatorButton.UseVisualStyleBackColor = true;
            addEmulatorButton.Click += addEmulatorButton_Click;
            // 
            // deviceIdInputTextBox
            // 
            deviceIdInputTextBox.Location = new Point(135, 25);
            deviceIdInputTextBox.Name = "deviceIdInputTextBox";
            deviceIdInputTextBox.PlaceholderText = "Paste device Id";
            deviceIdInputTextBox.Size = new Size(817, 27);
            deviceIdInputTextBox.TabIndex = 1;
            // 
            // deviceKeyInputTextBox
            // 
            deviceKeyInputTextBox.Location = new Point(135, 71);
            deviceKeyInputTextBox.Name = "deviceKeyInputTextBox";
            deviceKeyInputTextBox.PlaceholderText = "Paste device key";
            deviceKeyInputTextBox.Size = new Size(817, 27);
            deviceKeyInputTextBox.TabIndex = 2;
            // 
            // deviceIdInputLabel
            // 
            deviceIdInputLabel.AutoSize = true;
            deviceIdInputLabel.Location = new Point(16, 28);
            deviceIdInputLabel.Name = "deviceIdInputLabel";
            deviceIdInputLabel.Size = new Size(71, 20);
            deviceIdInputLabel.TabIndex = 3;
            deviceIdInputLabel.Text = "Device Id";
            // 
            // deviceKeyInputLabel
            // 
            deviceKeyInputLabel.AutoSize = true;
            deviceKeyInputLabel.Location = new Point(16, 74);
            deviceKeyInputLabel.Name = "deviceKeyInputLabel";
            deviceKeyInputLabel.Size = new Size(82, 20);
            deviceKeyInputLabel.TabIndex = 4;
            deviceKeyInputLabel.Text = "Device Key";
            // 
            // emulatorsDataGridView
            // 
            emulatorsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            emulatorsDataGridView.Location = new Point(16, 146);
            emulatorsDataGridView.Name = "emulatorsDataGridView";
            emulatorsDataGridView.RowHeadersWidth = 51;
            emulatorsDataGridView.RowTemplate.Height = 29;
            emulatorsDataGridView.Size = new Size(1274, 595);
            emulatorsDataGridView.TabIndex = 5;
            // 
            // EmulatorMainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1302, 753);
            Controls.Add(emulatorsDataGridView);
            Controls.Add(deviceKeyInputLabel);
            Controls.Add(deviceIdInputLabel);
            Controls.Add(deviceKeyInputTextBox);
            Controls.Add(deviceIdInputTextBox);
            Controls.Add(addEmulatorButton);
            Name = "EmulatorMainForm";
            Text = "IoT Emulator";
            ((System.ComponentModel.ISupportInitialize)emulatorsDataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button addEmulatorButton;
        private TextBox deviceIdInputTextBox;
        private TextBox deviceKeyInputTextBox;
        private Label deviceIdInputLabel;
        private Label deviceKeyInputLabel;
        private DataGridView emulatorsDataGridView;
    }
}
namespace GenerateEFModels
{
    partial class FormGenerateEfModels
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
            buttonGenerate = new Button();
            textBoxDbml = new TextBox();
            SuspendLayout();
            // 
            // buttonGenerate
            // 
            buttonGenerate.Dock = DockStyle.Bottom;
            buttonGenerate.Location = new Point(0, 416);
            buttonGenerate.Name = "buttonGenerate";
            buttonGenerate.Size = new Size(800, 34);
            buttonGenerate.TabIndex = 0;
            buttonGenerate.Text = "Generate";
            buttonGenerate.UseVisualStyleBackColor = true;
            buttonGenerate.Click += buttonGenerate_Click;
            // 
            // textBoxDbml
            // 
            textBoxDbml.Dock = DockStyle.Fill;
            textBoxDbml.Location = new Point(0, 0);
            textBoxDbml.Multiline = true;
            textBoxDbml.Name = "textBoxDbml";
            textBoxDbml.Size = new Size(800, 416);
            textBoxDbml.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textBoxDbml);
            Controls.Add(buttonGenerate);
            Name = "FormGenerateEFModels";
            Text = "Generate EF Models";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonGenerate;
        private TextBox textBoxDbml;
    }
}
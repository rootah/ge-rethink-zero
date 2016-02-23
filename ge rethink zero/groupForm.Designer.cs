namespace ge_rethink_zero
{
    partial class groupForm
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
            this.groupUC1 = new ge_rethink_zero.groupUC();
            this.SuspendLayout();
            // 
            // groupUC1
            // 
            this.groupUC1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupUC1.Location = new System.Drawing.Point(0, 0);
            this.groupUC1.MaximumSize = new System.Drawing.Size(264, 164);
            this.groupUC1.MinimumSize = new System.Drawing.Size(264, 164);
            this.groupUC1.Name = "groupUC1";
            this.groupUC1.Size = new System.Drawing.Size(264, 164);
            this.groupUC1.TabIndex = 0;
            // 
            // groupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 165);
            this.Controls.Add(this.groupUC1);
            this.Name = "groupForm";
            this.Text = "groupForm";
            this.ResumeLayout(false);

        }

        #endregion

        private groupUC groupUC1;
    }
}
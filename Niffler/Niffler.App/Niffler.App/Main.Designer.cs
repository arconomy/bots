namespace Niffler.App
{
    partial class Main
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
            this.ButtonBuy = new System.Windows.Forms.Button();
            this.ButtonSell = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ButtonBuy
            // 
            this.ButtonBuy.Location = new System.Drawing.Point(12, 12);
            this.ButtonBuy.Name = "ButtonBuy";
            this.ButtonBuy.Size = new System.Drawing.Size(116, 92);
            this.ButtonBuy.TabIndex = 0;
            this.ButtonBuy.Text = "BUY";
            this.ButtonBuy.UseVisualStyleBackColor = true;
            // 
            // ButtonSell
            // 
            this.ButtonSell.Location = new System.Drawing.Point(156, 12);
            this.ButtonSell.Name = "ButtonSell";
            this.ButtonSell.Size = new System.Drawing.Size(116, 92);
            this.ButtonSell.TabIndex = 1;
            this.ButtonSell.Text = "SELL";
            this.ButtonSell.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.ButtonSell);
            this.Controls.Add(this.ButtonBuy);
            this.Name = "Main";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ButtonBuy;
        private System.Windows.Forms.Button ButtonSell;
    }
}


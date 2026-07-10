namespace SelfishNetv3
{
    partial class AdapterSelectionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.labelTypeText = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelIPINFO = new System.Windows.Forms.Label();
            this.labelRedirectInfo = new System.Windows.Forms.Label();
            this.labelGWText = new System.Windows.Forms.Label();
            this.labelIpText = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();

            // labelTypeText
            this.labelTypeText.AutoSize = true;
            this.labelTypeText.Location = new System.Drawing.Point(117, 126);
            this.labelTypeText.Name = "labelTypeText";
            this.labelTypeText.Size = new System.Drawing.Size(71, 20);
            this.labelTypeText.Text = "Ethernet";

            // label4
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 126);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 20);
            this.label4.Text = "NIC Type :";

            // labelIPINFO
            this.labelIPINFO.AutoSize = true;
            this.labelIPINFO.Location = new System.Drawing.Point(208, 158);
            this.labelIPINFO.Name = "labelIPINFO";
            this.labelIPINFO.Size = new System.Drawing.Size(0, 20);

            // labelRedirectInfo
            this.labelRedirectInfo.AutoSize = true;
            this.labelRedirectInfo.Location = new System.Drawing.Point(14, 234);
            this.labelRedirectInfo.Name = "labelRedirectInfo";
            this.labelRedirectInfo.Size = new System.Drawing.Size(563, 20);
            this.labelRedirectInfo.Text = "Checking IP forwarding status...";

            // labelGWText
            this.labelGWText.AutoSize = true;
            this.labelGWText.Location = new System.Drawing.Point(117, 197);
            this.labelGWText.Name = "labelGWText";
            this.labelGWText.Size = new System.Drawing.Size(57, 20);
            this.labelGWText.Text = "0.0.0.0";

            // labelIpText
            this.labelIpText.AutoSize = true;
            this.labelIpText.Location = new System.Drawing.Point(117, 158);
            this.labelIpText.Name = "labelIpText";
            this.labelIpText.Size = new System.Drawing.Size(57, 20);
            this.labelIpText.Text = "0.0.0.0";

            // label3
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 197);
            this.label3.Name = "label3";
            this.label3.Text = "Gateway :";

            // label2
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 158);
            this.label2.Name = "label2";
            this.label2.Text = "IP Address :";

            // label1
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 65);
            this.label1.Name = "label1";
            this.label1.Text = "Select the Network Interface Card :";

            // buttonCancel
            this.buttonCancel.Location = new System.Drawing.Point(292, 308);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(76, 51);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);

            // buttonOK
            this.buttonOK.Location = new System.Drawing.Point(154, 308);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(76, 51);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);

            // comboBox1
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(18, 89);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(490, 28);
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.ComboBox1_SelectedIndexChanged);

            // AdapterSelectionForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(612, 409);
            this.ControlBox = false;
            this.Controls.Add(this.labelTypeText);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.labelIPINFO);
            this.Controls.Add(this.labelRedirectInfo);
            this.Controls.Add(this.labelGWText);
            this.Controls.Add(this.labelIpText);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.comboBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "AdapterSelectionForm";
            this.Opacity = 0D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NIC Selection";
            this.Shown += new System.EventHandler(this.CAdapter_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelTypeText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelIPINFO;
        private System.Windows.Forms.Label labelRedirectInfo;
        private System.Windows.Forms.Label labelGWText;
        private System.Windows.Forms.Label labelIpText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

using System;
using System.Windows.Forms;

namespace SelfishNetv3
{
    /// <summary>
    /// About dialog form.
    /// </summary>
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

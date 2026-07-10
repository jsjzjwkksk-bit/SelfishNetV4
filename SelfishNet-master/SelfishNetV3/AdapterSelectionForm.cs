using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SelfishNetv3
{
    /// <summary>
    /// Dialog for selecting a network interface card (NIC) for ARP operations.
    /// </summary>
    public partial class AdapterSelectionForm : Form
    {
        private NetworkInterface[] nics = Array.Empty<NetworkInterface>();
        private NetworkInterface? selectedNic;

        public AdapterSelectionForm()
        {
            InitializeComponent();
            buttonOK.Enabled = false;
            buttonCancel.Text = "Quit";
        }

        // ───── Events ─────

        private void CAdapter_Shown(object sender, EventArgs e)
        {
            Opacity = 1.0;
            ArpForm.Instance!.Enabled = false;

            nics = NetworkInterface.GetAllNetworkInterfaces();

            // Check IP forwarding
            var firstNic = nics.FirstOrDefault();
            if (firstNic is not null)
            {
                try
                {
                    bool forwarding = firstNic.GetIPProperties().GetIPv4Properties().IsForwardingEnabled;
                    labelRedirectInfo.Text = forwarding
                        ? "Windows redirects packets.\nInternal redirection will be turned off."
                        : "Windows does not redirect packets.\nInternal redirection will be turned on.";
                }
                catch
                {
                    labelRedirectInfo.Text = "Could not determine IP forwarding status.";
                }
            }

            // Populate combo box with NICs that have gateways and are up
            foreach (var nic in nics)
            {
                if (nic.OperationalStatus == OperationalStatus.Up
                    && nic.GetIPProperties().GatewayAddresses.Count > 0)
                {
                    comboBox1.Items.Add(nic.Description);
                }
            }

            if (comboBox1.Items.Count >= 1)
            {
                // Auto-select the first NIC with a non-zero gateway
                var preferred = nics.FirstOrDefault(n =>
                    n.GetIPProperties().GatewayAddresses.Count > 0
                    && n.GetIPProperties().GatewayAddresses[0].Address.ToString() != "0.0.0.0");

                comboBox1.SelectedIndex = preferred is not null
                    ? comboBox1.Items.IndexOf(preferred.Description)
                    : 0;
            }
            else
            {
                MessageBox.Show(
                    "No network card with a gateway has been found!",
                    "No Adapter",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                ArpForm.Instance?.Dispose();
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDesc = comboBox1.SelectedItem?.ToString() ?? string.Empty;
            var nic = nics.FirstOrDefault(n => n.Description == selectedDesc);
            if (nic is null) return;

            labelTypeText.Text = nic.NetworkInterfaceType.ToString();

            // Find first IPv4 address
            var ipv4 = nic.GetIPProperties().UnicastAddresses
                .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);

            labelIpText.Text = ipv4?.Address.ToString() ?? "N/A";

            // Check gateway
            var gw = nic.GetIPProperties().GatewayAddresses;
            if (gw.Count > 0 && gw[0].Address.ToString() != "0.0.0.0")
            {
                labelGWText.Text = gw[0].Address.ToString();
                buttonOK.Enabled = true;
                selectedNic = nic;
            }
            else
            {
                labelGWText.Text = "No Gateway!";
                buttonOK.Enabled = false;
                selectedNic = null;
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (selectedNic is null) return;

            buttonCancel.Text = "Cancel";
            ArpForm.Instance!.Enabled = true;
            ArpForm.Instance.NicIsSelected(selectedNic);
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to close SelfishNet?",
                "Confirm Exit",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.OK)
            {
                if (buttonCancel.Text == "Quit")
                {
                    ArpForm.Instance?.Dispose();
                    Environment.Exit(0);
                }
                else
                {
                    ArpForm.Instance!.Enabled = true;
                    Close();
                }
            }
        }
    }
}

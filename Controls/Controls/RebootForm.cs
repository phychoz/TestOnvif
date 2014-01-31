using System.Windows.Forms;

namespace Onvif.Controls
{
    public partial class RebootForm : Form
    {
        Timer timer;

        int interval = 1000;
        int max = 120000;
        int min = 0;
        int value = 0;

        public RebootForm(string message)
        {      
            InitializeComponent();

            this.Text = "Reboot in process...";
            RebootMessgeLabel.Text = message;

            timer = new Timer();
            timer.Interval = interval;
            value = min;

            RebootProgressBar.Maximum = max;
            RebootProgressBar.Minimum = min;
            RebootProgressBar.Value = value;

            timer.Tick += (obj,arg) => 
            {
                if (value == max) { this.Close(); return; }
                RebootProgressBar.Value = value;
                value += interval;
            };
            timer.Start();

        }
    }
}

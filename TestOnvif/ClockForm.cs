using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestOnvif
{
    public partial class ClockForm : Form
    {
        public ClockForm(String capture)
        {
            InitializeComponent();
            this.Text = capture;
        }

        public void UpdateLabel(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                {
                    ClockLabel.Text = text;
                }));
            }
            else
            {
                ClockLabel.Text = text;
            }
        }
    }
}

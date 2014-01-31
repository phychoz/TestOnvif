using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Onvif.Controls
{
    public partial class WebForm : Form
    {
        public WebForm(Uri uri)
        {
            InitializeComponent();
            this.webBrowser.Url = uri;
        }
    }
}

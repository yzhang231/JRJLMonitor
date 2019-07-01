using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JRJLMonitor
{
    public partial class Monitor : Form
    {
        private Boolean islogin=false;
        private Camera cl;
        public Monitor()
        {
            InitializeComponent();
            cl = new Camera("192.168.0.64", 8000, "admin", "zhang1983");
            islogin = cl.Login();
            cl.Preview(this.pictureBox1);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (islogin) { cl.StartRecord("1.mp4"); }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (islogin) { cl.StopRecord(); }
        }
    }
}

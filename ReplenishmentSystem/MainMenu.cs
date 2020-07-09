using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReplenishmentSystem.Process;

namespace ReplenishmentSystem
{
    public partial class MainMenu : Form
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            InitData();
        }

        private void InitData()
        {
            if (GlobalVariables.UserRights == 2)
            {
                this.TabControl.TabPages.Remove(tab2);
            }



        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            Form Users = new Users();
            Users.Show();
        }

        private void btnRequest_Click(object sender, EventArgs e)
        {
            Form Request = new Request();
            Request.ShowDialog();
        }

        private void btnMonitoring_Click(object sender, EventArgs e)
        {
            Form Monitoring = new Monitoring();
            Monitoring.ShowDialog();
        }

    }
}

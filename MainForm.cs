using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TimeStop.SystemChecker;
using TimeStop.Utils;

namespace TimeStop.DatabaseUpdater
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            DatabaseUpdater form = new DatabaseUpdater();
            form.ShowDialog();
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            EmployeeHistoryUpdater form = new EmployeeHistoryUpdater();
            form.ShowDialog();
        }

        private void btnLegacy_Click(object sender, EventArgs e)
        {
            LegacyToUniqueIDUpdater form = new LegacyToUniqueIDUpdater();
            form.ShowDialog();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SystemInitMin init = new SystemInitMin();
            init.ShowDialog();
            if (!init.IsSuccessful())
            {
                Util.ShowError("Can't establish connection to online database!");
                this.Close();
            }
        }
    }
}

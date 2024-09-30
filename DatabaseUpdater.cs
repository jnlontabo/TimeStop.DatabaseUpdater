using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TimeStop.Utils;
using System.IO;
using System.Text.RegularExpressions;
using TimeStop.SystemChecker;

namespace TimeStop.DatabaseUpdater
{
    public partial class DatabaseUpdater : Form
    {
        BackgroundWorker _worker;
        public DatabaseUpdater()
        {
            InitializeComponent();
            InitializeWorker();
        }

        private void InitializeWorker()
        {
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.ProgressChanged += new ProgressChangedEventHandler(_worker_ProgressChanged);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
        }

        void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            txtResult.AppendText(Util.ToString(e.UserState) + Environment.NewLine);
        }

        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Util.ShowInfo("Update completed.");
            EnableControls(true);
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            RunSqlScript();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateFields().IsEmptyValue()) return;
            if (!Directory.Exists(txtFolder.Text)) return;
            EnableControls(false);
            _worker.RunWorkerAsync();
        }

        private void RunSqlScript()
        {
            // get the list of files
            string[] fileArray = Directory.GetFiles(txtFolder.Text, "*.sql");
            for (int j = txtStart.Text.ConvertToInt(); j <= txtEnd.Text.ConvertToInt(); j++)
            {
                string head = j.ToString() + ")";
                var file = fileArray.Where(t => Path.GetFileName(t).StartsWith(head)).FirstOrDefault();
                if (File.Exists(file))
                {
                    if (Path.GetFileName(file).ToLower().Contains("run employeehistoryupdater"))
                    {
                        EmployeeHistoryUpdater historyUpdater = new EmployeeHistoryUpdater();
                        historyUpdater.ShowDialog();
                        continue;
                    }

                    string sql = Util.ReadFile(file);
                    sql = Regex.Replace(sql, @"\r\n?|\n", " ");

                    if (new SQLManager().ExecuteNonQuery(CommandOption.Custom, null, sql))
                        _worker.ReportProgress(1, head + " Success.");
                    else
                        _worker.ReportProgress(1, head + " Failed.");
                }
            }
        }

        private string ValidateFields()
        {
            string err = string.Empty;
            if (Util.IsEmpty(txtFolder)) err += " - Folder location";
            if (Util.IsEmpty(txtStart)) err += " - Folder location";
            if (Util.IsEmpty(txtEnd)) err += " - Folder location";
            return err;
        }

        private void EnableControls(bool b)
        {
            btnUpdate.Enabled = b;
            txtFolder.Enabled = b;
            txtStart.Enabled = b;
            txtEnd.Enabled = b;
            this.Cursor = b ? Cursors.Default : Cursors.WaitCursor;
        }

        private void txtResult_VisibleChanged(object sender, EventArgs e)
        {
            if (txtResult.Visible)
            {
                txtResult.SelectionStart = txtResult.TextLength;
                txtResult.ScrollToCaret();
            }
        }
    }
}

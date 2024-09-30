using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeStop.Utils;

namespace TimeStop.DatabaseUpdater
{
    public partial class EmployeeHistoryUpdater : Form
    {
        BackgroundWorker _worker;
        public EmployeeHistoryUpdater()
        {
            InitializeComponent();
            InitializeWorker();
        }

        private void InitializeWorker()
        {
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
            _worker.WorkerSupportsCancellation = true;
        }

        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Util.ShowInfo("Update completed.");
            btnUpdate.Enabled = true;
            this.Cursor = Cursors.Default;
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateTable("Active");
            UpdateTable("Inactive");
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            btnUpdate.Enabled = false;
            _worker.RunWorkerAsync();
        }

        private void UpdateTable(string serviceStatus)
        {
            // iterate first to all employees
            DataTable dt = new SQLManager().ExecuteQuery("select employee_id from employees", null);
            foreach(DataRow row in dt.Rows)
            {

                string employeeId = row["employee_id"].ToString();

                // get first distinct record_date
                string sql =
                    "select cast(record_date as date) record_date from employees_history " +
                    "where employee_id = '" + employeeId + "' and service_status = '" + serviceStatus + "' " +
                    "order by history_id desc";
                DataTable active = new SQLManager().ExecuteQuery(sql, null);
                foreach(DataRow rowActive in active.Rows)
                {
                    string recordDate = Util.ToDateDB(rowActive["record_date"]);
                    // get the highest history_id for each record
                    sql =
                        "select history_id from employees_history " +
                        "where employee_id = '" + employeeId + "' " +
                        "and service_status = '" + serviceStatus + "' " +
                        "and cast(record_date as date) = '" + recordDate + "' " +
                        "order by history_id desc " +
                        "limit 1;";
                    int highestHistoryId = new SQLManager().ExecuteQuery(sql, null).GetInt();

                    // delete other records
                    sql =
                        "delete from employees_history " +
                        "where employee_id = '" + employeeId + "' " +
                        "and service_status = '" + serviceStatus + "' " +
                        "and cast(record_date as date) = '" + recordDate + "' " +
                        "and history_id != " + highestHistoryId;
                    var result = new SQLManager().ExecuteQuery(sql, null);
                }
            }
        }
    }
}

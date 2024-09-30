using System;
using System.Data;
using System.Windows.Forms;
using TimeStop.Utils;

namespace TimeStop.DatabaseUpdater
{
    public partial class LegacyToUniqueIDUpdater : Form
    {
        public LegacyToUniqueIDUpdater()
        {
            InitializeComponent();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to update the column " + cboColumns.Text + " of table " + cboTables.Text + "?", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.Cancel) return;
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = new SQLManager().ExecuteQuery("select concat(" + cboColumns.Text + ",'') from " + cboTables.Text, null);
            foreach(DataRow row in dt.Rows)
            {
                string sql = "update " + cboTables.Text + " set " + cboColumns.Text + " = '" + Util.GetGuid() + "' where " + cboColumns.Text + " = '" + row[0].ToString() + "'";
                bool success = new SQLManager().ExecuteNonQuery(CommandOption.Update, cboTables.Text, null, sql);
                if (!success)
                {
                    Util.ShowError("Something is wrong while updating row " + row[0] + "!");
                    return;
                }
            }
            this.Cursor = Cursors.Default;
            Util.ShowInfo("Table update done!");
        }

        private void FillTables() { Util.FillComboBox(cboTables, "select table_name from information_schema.tables where table_schema = 'stbdv104'", "table_name", "table_name"); }
        private void FillColumns() { Util.FillComboBox(cboColumns, "select column_name from information_schema.columns where table_schema = 'stbdv104' and table_name = '" + cboTables.Text + "'", "column_name", "column_name"); }

        private void DatabaseUpdater_Load(object sender, EventArgs e)
        {
            FillTables();
            FillColumns();
        }

        private void cboTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillColumns();
        }
    }
}

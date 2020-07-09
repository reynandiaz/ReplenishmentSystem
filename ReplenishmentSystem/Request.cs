using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using ReplenishmentSystem.Process;

namespace ReplenishmentSystem
{
    public partial class Request : Form
    {
        public static DataTable dRequestTable { get; set; }

        public Request()
        {
            InitializeComponent();
        }
        
        private void Request_Load(object sender, EventArgs e)
        {
            setTable();
            initData();
        }

        private void setTable()
        {

            dRequestTable = new DataTable();

            dRequestTable.Columns.Add("TYPE");
            dRequestTable.Columns.Add("ITEM ID");
            dRequestTable.Columns.Add("ITEM DESCRIPTION");
            dRequestTable.Columns.Add("PRICE PER UNIT");
            dRequestTable.Columns.Add("QUANTITY");
            dRequestTable.Columns.Add("UNIT");
            dRequestTable.Columns.Add("TOTAL PRICE");

            RequestTable.DataSource = dRequestTable;
            RequestTable.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            RequestTable.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        }
        private void initData()
        {
            if (GlobalVariables.UserRights != 1)
            {
                txtDepartment.Text = GlobalVariables.DepartmentName;
                txtDepartment.ReadOnly = true;
                txtSection.Text = GlobalVariables.SectionName;
                txtSection.ReadOnly = true;
                txtStatus.Text = "NEW";
                txtStatus.ReadOnly = true;
                txtRequestor.Text = GlobalVariables.EmployeeName;
                txtRequestor.ReadOnly = true;
                txtDate.Text = DateTime.Now.ToString().Substring(0, 11);
                txtDate.ReadOnly = true;
            }
        
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Form RequestItems = new RequestItems();
            RequestItems.ShowDialog();
            RequestTable.DataSource = dRequestTable;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to save?",
                      "System Message", MessageBoxButtons.YesNo);
            switch (dr)
            {
                case DialogResult.Yes:

                    if (RequestTable.Rows.Count != 0)
                    {
                        string maxRequest = getMaxRequestNo();
                        InsertHeader(maxRequest);
                        InsertDetails(maxRequest);
                    }
                    else
                    { MessageBox.Show("Input Data!"); }
                    break;
                case DialogResult.No:
                    break;
            }

        }

        #region //INSERT HEADER AND DETAILS AND GENERATE REQUEST NO
        private static string getMaxRequestNo()
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);
            string strMaxReqNo;
           
            string query = "Select max(RequestCode) from RequestHeaders";

            var command = new OleDbCommand(query, connection);
            try
            {
                connection.Open();
                var MaxReq = command.ExecuteScalar();

                string curDate = DateTime.Now.ToString().Substring(0, 11).Replace("/", "").Trim();
                string maxDate = (MaxReq.ToString() == "" ? "R-" + curDate + "-0001": MaxReq.ToString().Split('-')[1].Trim());
                
                if (MaxReq.ToString() == "")
                {
                    strMaxReqNo = "R-" + curDate + "-0001";
                }
                else if (maxDate != curDate)
                {
                    strMaxReqNo = "R-" + curDate + "-0001";
                }
                else
                {
                    int intMaxSeq = Convert.ToInt32(MaxReq.ToString().Split('-')[2]);

                    if (intMaxSeq.ToString().Length == 1)
                    {
                        strMaxReqNo = "R-" + curDate + "-000" + (intMaxSeq + 1);
                    }
                    else if (intMaxSeq.ToString().Length == 2)
                    {
                        strMaxReqNo = "R-" + curDate + "-00" + (intMaxSeq + 1);
                    }
                    else if (intMaxSeq.ToString().Length == 3)
                    {
                        strMaxReqNo = "R-" + curDate + "-0" + (intMaxSeq + 1);
                    }
                    else
                    {
                        strMaxReqNo = "R-" + curDate + "-" + (intMaxSeq + 1);
                    }
                }
                return strMaxReqNo;
            }
            finally
            {
                connection.Close();
            }
        }

        private void InsertHeader(string ReqNo)
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

            string strInsert = "Insert into RequestHeaders values" +
                "(" +
                "@RequestCode ," +
                "@DepartmentID ," +
                "@SectionID ," +
                "@StatusID ," +
                "@ControlNo ," +
                "@RequestorID ," +
                "@Reason ," +
                "date() ," +
                "date() ," +
                "null ," +
                "@UpdatedBy " +
                ")";

            connection.Open();
            var trans = connection.BeginTransaction();
            var command = new OleDbCommand(strInsert, connection, trans);
            try
            {
                command.Parameters.Add("@RequestCode", OleDbType.VarChar).Value = ReqNo;
                command.Parameters.Add("@DepartmentID", OleDbType.Integer).Value = GlobalVariables.DepartmentID;
                command.Parameters.Add("@SectionID", OleDbType.Integer).Value = GlobalVariables.SectionID;
                command.Parameters.Add("@StatusID", OleDbType.Integer).Value = 1;
                command.Parameters.Add("@ControlNo", OleDbType.VarChar).Value = txtControlNo.Text;
                command.Parameters.Add("@RequestorID", OleDbType.VarChar).Value = GlobalVariables.UserID;
                command.Parameters.Add("@Reason", OleDbType.VarChar).Value = txtReason.Text;
                command.Parameters.Add("@UpdatedBy", OleDbType.VarChar).Value = GlobalVariables.UserID;
                command.ExecuteNonQuery();
                trans.Commit();
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.ToString());
                trans.Rollback();
            }
            finally
            {
                connection.Close();
            }
        }

        private void InsertDetails(string ReqNo)
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

            int SeqNo = 1;
            foreach (DataRow rows in dRequestTable.Rows)
            {
                string strInsert = "Insert into RequestDetails Values" +
                        "(" +
                        "@RequestNo ," +
                        "@SeqNo ," +
                        "@ItemID ," +
                        "@ItemPrice , " +
                        "@Qty , " +
                        "@TotalPrice ," +
                        "date() ," +
                        "date() ," +
                        "null ," +
                        "@UpdatedBy " +
                        ")";

                connection.Open();
                var trans = connection.BeginTransaction();
                var command = new OleDbCommand(strInsert, connection, trans);
                try
                {

                    command.Parameters.Add("@RequestNo", OleDbType.VarChar).Value = ReqNo;
                    command.Parameters.Add("@SeqNo", OleDbType.Integer).Value = SeqNo;
                    command.Parameters.Add("@ItemID", OleDbType.VarChar).Value = rows[1].ToString();
                    command.Parameters.Add("@ItemPrice", OleDbType.Integer).Value = Convert.ToInt32(rows[3]);
                    command.Parameters.Add("@Qty", OleDbType.Integer).Value = Convert.ToInt32(rows[4]);
                    command.Parameters.Add("@TotalPrice", OleDbType.Integer).Value = Convert.ToInt32(rows[6]);
                    command.Parameters.Add("@UpdatedBy", OleDbType.VarChar).Value = GlobalVariables.UserID;
                    command.ExecuteNonQuery();
                    trans.Commit();

                }
                catch(Exception exc)
                {
                    MessageBox.Show(exc.ToString());
                    trans.Rollback();
                }
                finally
                {
                    connection.Close();
                }
                SeqNo = SeqNo + 1;
            }
            MessageBox.Show("Record Saved! \n \nRequest No:"+ReqNo);

            this.Close();
        }
        #endregion  
    }
}

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
    public partial class Request_TEST : Form
    {
        public static DataTable dRequestTable { get; set; }

        public static DataGridViewComboBoxColumn cmbItems { get; set; }

        DataTable dataTable = dRequestTable;
        

        public Request_TEST()
        {
            InitializeComponent();

            RequestTable.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(RequestTable_EditingControlShowing);
        }


        private void RequestTable_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            ComboBox combo = e.Control as ComboBox;
            if((RequestTable.CurrentCell.ColumnIndex) ==0)
            { 
                if (combo != null)
                {
                    combo.SelectedIndexChanged -= new EventHandler(cmbKind_SelectedIndexChanged);
                    combo.SelectedIndexChanged += new EventHandler(cmbKind_SelectedIndexChanged);
                }
            }
        }

        private void cmbKind_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            string item = cb.Text;
            //if (item != null)
            //    MessageBox.Show(item);
            //MessageBox.Show(RequestTable.Rows[RequestTable.RowIndex].Cells[RequestTable.ColumnIndex].Value.ToString());
            if (item != null)
            {
                var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

                string strItems = "SELECT Items.* " +
               "FROM Items INNER JOIN Types ON Items.TypeID = Types.TypeID " +
               "WHERE(((Items.DeletedDate)Is Null) AND((Types.Type) = @Type)); ";

                var command = new OleDbCommand(strItems, connection);

                try
                {
                    connection.Open();
                    cmbItems = new DataGridViewComboBoxColumn();
                    command.Parameters.Add("@Type", OleDbType.VarChar).Value = item;
                    var reader = command.ExecuteReader();
                    cmbItems.Items.Clear();
                    while (reader.Read())
                    {
                        cmbItems.Items.Add(reader["ItemDescription"].ToString());
                    }
                    
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.ToString());
                }
                finally
                {
                    connection.Close();
                }

            }

        }








        private void Request_TEST_Load(object sender, EventArgs e)
        {
            initData();
            setTable();
            
        }
        private void setTable()
        {

            dRequestTable = new DataTable();
            dRequestTable.Columns.Add("PRICE PER UNIT");
            dRequestTable.Columns.Add("QUANTITY");
            dRequestTable.Columns.Add("UNIT");
            dRequestTable.Columns.Add("TOTAL PRICE");

            RequestTable.DataSource = dRequestTable;

        }

        private void initData() {

            generateKind();

            cmbItems = new DataGridViewComboBoxColumn();
            cmbItems.HeaderText = "ITEMS";
            cmbItems.DisplayIndex = 1;
            cmbItems.ToolTipText = "RENAN";

            RequestTable.Columns.Add(cmbItems);

        }

        private void generateKind()
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

            string strTypes = "SELECT Types.* " +
                  "FROM Types " +
                  "WHERE(((Types.DeletedDate)Is Null)); ";

            var command = new OleDbCommand(strTypes, connection);


            DataGridViewComboBoxColumn cmbKind = new DataGridViewComboBoxColumn();

            cmbKind.HeaderText = "TYPE";
            cmbKind.DisplayIndex = 0;

            try
            {
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    cmbKind.Items.Add(reader["Type"].ToString());
                }
                RequestTable.Columns.Add(cmbKind);

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
            finally
            {
                connection.Close();
            }
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
                string maxDate = (MaxReq.ToString() == "" ? "R-" + curDate + "-0001" : MaxReq.ToString().Split('-')[1].Trim());



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
            catch (Exception exc)
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
                catch (Exception exc)
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
            MessageBox.Show("Record Saved! \n \nRequest No:" + ReqNo);

            this.Close();
        }

        #endregion


    }
}

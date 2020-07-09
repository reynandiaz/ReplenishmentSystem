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
    public partial class Monitoring : Form
    {
        public Monitoring()
        {
            InitializeComponent();
        }

        private void Monitoring_Load(object sender, EventArgs e)
        {
            if(GlobalVariables.UserRights!=2)
            { 
                getRequestDetails("","");
            }
            else 
            {
                getRequestDetails("AND ((RequestHeaders.RequestorID)=':requestorid:')".Replace(":requestorid:",GlobalVariables.UserID),"");
            }
        }

        private void getRequestDetails(string queryFilter,string SearchParams)
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

            string query = "SELECT RequestHeaders.RequestCode , RequestHeaders.CreatedDate, " +
                           "Employees.EmployeeName as Requestor, Departments.DepartmentName , " +
                           "Sections.SectionName , RequestHeaders.Reason, RequestHeaders.ControlNo, " +
                           "Status.Status " +
                           "FROM(Sections INNER JOIN Departments ON Sections.DepartmentID = Departments.DepartmentID) INNER JOIN(Employees INNER JOIN (RequestHeaders INNER JOIN Status ON RequestHeaders.StatusID = Status.StatusID) " +
                           "ON Employees.EmployeeID = RequestHeaders.RequestorID) ON(Departments.DepartmentID = Employees.DepartmentID) AND(Sections.SectionID = Employees.SectionID) " +
                           "WHERE (((RequestHeaders.DeletedDate) Is Null) "+ queryFilter + " "+ SearchParams + "); ";

            var command = new OleDbCommand(query, connection);
            try
            {
                connection.Open();
                var reader = command.ExecuteReader();
                var dTable = new DataTable();
                dTable.Load(reader);

                RequestTable.Columns.Clear();
                RequestTable.DataSource = dTable;
                RequestTable.Columns[0].Width = 104;
                RequestTable.Columns[1].Width = 101;
                RequestTable.Columns[2].Width = 101;
                RequestTable.Columns[3].Width = 101;
                RequestTable.Columns[4].Width = 101;
                RequestTable.Columns[5].Width = 101;
                RequestTable.Columns[6].Width = 101;
                RequestTable.Columns[7].Width = 101;
                
                DataGridViewButtonColumn btnDetails = new DataGridViewButtonColumn();
                btnDetails.HeaderText = "";
                btnDetails.Text = ">>";
                btnDetails.Width = 30;
                btnDetails.DisplayIndex = 8;
                btnDetails.UseColumnTextForButtonValue = true;
                btnDetails.FlatStyle = FlatStyle.Standard;
                RequestTable.Columns.Add(btnDetails);
                
            }

            finally { connection.Close(); }
        }

        private void generateItemDetails(string ReqNo)
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

            string query = "Select [ItemDescription] & ' ' & [Quantity] & '(' & [Unit] &')  = ' & [TotalPrice] " +
                          "from " +
                          "( " +
                          "SELECT Items.ItemDescription, RequestDetails.Quantity, Units.Unit, RequestDetails.TotalPrice " +
                          "FROM(RequestDetails INNER JOIN Items ON RequestDetails.ItemID = Items.ItemID) INNER JOIN Units ON Items.UnitID = Units.UnitID " +
                          "WHERE(((RequestDetails.RequestCode) = @RequestNo))); ";

            var command = new OleDbCommand(query, connection);
            try
            {
                connection.Open();
                command.Parameters.Add("@RequestNo", OleDbType.VarChar).Value = ReqNo;
                var reader = command.ExecuteReader();
                var dTable = new DataTable();
                dTable.Load(reader);
                RequestDetails.DataSource = dTable;
                RequestDetails.Columns[0].Width = 230;
            }
            finally { connection.Close(); }
        }

        private void RequestTable_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {

            try
            {
                if (RequestTable.Rows[e.RowIndex].Index >= 0)
                {
                    generateItemDetails(RequestTable.Rows[e.RowIndex].Cells[0].Value.ToString());
                }
            }
            catch { }
        }

        private void RequestTable_MouseClick(object sender, MouseEventArgs e)
        {
            //RIGHT CLICK IS FOR APPROVER AND ADMIN 
            //if (GlobalVariables.UserRights == 4 || GlobalVariables.UserRights==1)
            //{ 
                if (e.Button == MouseButtons.Right)
                {
                    ContextMenu m = new ContextMenu();

                    int currentMouseOverRow = RequestTable.HitTest(e.X, e.Y).RowIndex;
                    string CellReqNo = RequestTable.Rows[currentMouseOverRow].Cells[0].Value.ToString();
                    if (currentMouseOverRow >= 0)
                    {
                        m.MenuItems.Add(new MenuItem(string.Format("Do something to row {0}", CellReqNo)));
                    }
                    m.Show(RequestTable, new Point(e.X, e.Y));
                }
            //}
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (GlobalVariables.UserRights != 2)
            {
                getRequestDetails("", "AND (RequestHeaders.RequestCode LIKE '%:SEARCH:%')".Replace(":SEARCH:",txtRequestNo.Text));
            }
            else
            {
                getRequestDetails("AND ((RequestHeaders.RequestorID)=':requestorid:')".Replace(":requestorid:", GlobalVariables.UserID), "AND (RequestHeaders.RequestCode LIKE '% :SEARCH: %')".Replace(" :SEARCH: ", txtRequestNo.Text));
            }
            txtRequestNo.Text = "";
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (GlobalVariables.UserRights != 2)
            {
                getRequestDetails("", "");
            }
            else
            {
                getRequestDetails("AND ((RequestHeaders.RequestorID)=':requestorid:')".Replace(":requestorid:", GlobalVariables.UserID), "");
            }
        }

    }
}

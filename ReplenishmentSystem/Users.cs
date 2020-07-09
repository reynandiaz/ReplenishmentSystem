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
using System.Data.OleDb;

namespace ReplenishmentSystem
{
    public partial class Users : Form
    {
        public Users()
        {
            InitializeComponent();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {

        }

        private void Users_Load(object sender, EventArgs e)
        {
            initData();
            SetTable("");
            
        }

        private void initData()
        {

            cmbFilter.Items.Add("EmployeeID");
            cmbFilter.Items.Add("EmployeeName");
            cmbFilter.Items.Add("Department");
            cmbFilter.Items.Add("Section");
            cmbFilter.Items.Add("Rights");
            
        }

        private void SetTable(string strFilter)
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);
            
            string query = "SELECT Employees.EmployeeID, Employees.EmployeeName, Departments.DepartmentName, Sections.SectionName, Users.Username, Users.Password, UserRights.Rights,Employees.DeletedDate as DelDT " +
                           "FROM Sections INNER JOIN((UserRights INNER JOIN Users ON UserRights.UserRightsID = Users.UserRightsID) INNER JOIN(Employees INNER JOIN Departments ON Employees.DepartmentID = Departments.DepartmentID) ON Users.EmployeeID = Employees.EmployeeID) ON(Sections.SectionID = Employees.SectionID) AND(Sections.DepartmentID = Departments.DepartmentID) " +
                           "WHERE Employees.EmployeeID <> 'ADMIN' " + strFilter + "  ORDER BY Employees.EmployeeID; ";
            var command = new OleDbCommand(query, connection);
            try
            {
                connection.Open();
                UsersTable.Columns.Clear();
                var reader = command.ExecuteReader();
                var dTable = new DataTable();
                dTable.Load(reader);
                UsersTable.DataSource = dTable;

                DataGridViewButtonColumn btnDetails = new DataGridViewButtonColumn();
                btnDetails.HeaderText = "";
                btnDetails.Text = ">>";
                btnDetails.Width = 30;
                btnDetails.DisplayIndex = 9;
                btnDetails.UseColumnTextForButtonValue = true;
                btnDetails.FlatStyle = FlatStyle.Standard;
                UsersTable.Columns.Add(btnDetails);

                UsersTable.Columns[7].Width = 75;

                foreach (DataGridViewRow row in UsersTable.Rows)
                {
                    if(UsersTable[7,row.Index].Value.ToString()!="")
                    { 
                        foreach (DataGridViewColumn col in UsersTable.Columns)
                        {
                            UsersTable[col.Index, row.Index].Style.BackColor = Color.Gray; //doesn't work
                        }
                    }
                }

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
            finally
            { connection.Close(); }

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (cmbFilter.Text != "" && txtFilter.Text != "")
            {
                switch (cmbFilter.SelectedIndex)
                {
                    case 0:
                        SetTable("AND ((Employees.EmployeeID) LIKE '%:filter:%')".Replace(":filter:", txtFilter.Text));
                        break;
                    case 1:
                        SetTable("AND ((Employees.EmployeeName) LIKE '%:filter:%')".Replace(":filter:", txtFilter.Text));
                        break;
                    case 2:
                        SetTable("AND ((Departments.DepartmentName) LIKE '%:filter:%')".Replace(":filter:", txtFilter.Text));
                        break;
                    case 3:
                        SetTable("AND ((Sections.SectionName) LIKE '%:filter:%')".Replace(":filter:", txtFilter.Text));
                        break;
                    default:
                        SetTable("AND ((UserRights.Rights) LIKE '%:filter:%')".Replace(":filter:", txtFilter.Text));
                        break;
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            cmbFilter.Text = "";
            txtFilter.Text = "";
            SetTable("");
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Form AddUser = new AddUser();
            GlobalVariables.UpdateUserID = null;
            AddUser.ShowDialog();
            SetTable("");
        }

        private void UsersTable_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 8)
            {

                GlobalVariables.UpdateUserID = UsersTable.Rows[e.RowIndex].Cells[0].Value.ToString();
                Form AddUser = new AddUser();
                AddUser.ShowDialog();
                SetTable("");
                GlobalVariables.UpdateUserID = null;
            }
        }

    }
}

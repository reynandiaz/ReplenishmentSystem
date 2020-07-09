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
    public partial class AddUser : Form
    {

        public AddUser()
        {
            InitializeComponent();
        }

        private void AddUser_Load(object sender, EventArgs e)
        {
            if (GlobalVariables.UpdateUserID == null)
            {
                GenerateUserRights();
                GenerateDepartments();
                GenerateEmployeeID();
                btnDelete.Visible = false;
                btnAdd.Text = "Add";
            }
            else
            {
                txtMaxID.Text = GlobalVariables.UpdateUserID;
                GenerateUserRights();
                GenerateDepartments();
                GenerateEmployeeInformation();
                btnDelete.Visible = true;
                btnAdd.Text = "Update";
            }

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (txtUser.Text != "" && txtPassword.Text != "" && cmbUserRights.Text != "" && txtFullName.Text != "" && cmbDept.Text!="" && cmbSect.Text!= "")
            {
                var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    if (GlobalVariables.UpdateUserID == null)
                    {
                        InsertUsers(connection, trans);
                        InsertEmployees(connection, trans);
                        trans.Commit();
                        MessageBox.Show("Record Saved!");
                        this.Close();
                    }
                    else
                    {
                        UpdateUsers(connection, trans);
                        UpdateEmployees(connection, trans);
                        trans.Commit();
                        MessageBox.Show("Records Updated!");
                        this.Close();
                    }
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
            else
            {
                MessageBox.Show("Input Fields");
            }
        }

        private void GenerateEmployeeInformation()
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

            string query = " SELECT Employees.*, Users.*, UserRights.*, Departments.*, Sections.* "+
                           " FROM Sections INNER JOIN(((Users INNER JOIN Employees ON Users.EmployeeID = Employees.EmployeeID) "+
                           " INNER JOIN UserRights ON Users.UserRightsID = UserRights.UserRightsID) INNER JOIN Departments ON Employees.DepartmentID = Departments.DepartmentID) "+
                           " ON(Sections.DepartmentID = Departments.DepartmentID) AND(Sections.SectionID = Employees.SectionID)"+
                           " WHERE(((Employees.EmployeeID) = @EmployeeID)); ";

            try{
                connection.Open();
                var command = new OleDbCommand(query, connection);
                command.Parameters.Add("@EmployeeID", OleDbType.VarChar).Value = GlobalVariables.UpdateUserID;
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    txtFullName.Text = reader["EmployeeName"].ToString();
                    cmbDept.Text = reader["DepartmentName"].ToString();
                    cmbSect.Text = reader["SectionName"].ToString();
                    txtUser.Text = reader["Username"].ToString();
                    txtPassword.Text = reader["Password"].ToString();
                    cmbUserRights.Text = reader["Rights"].ToString();
                    //if deleted =1 else 2
                    if (reader[6].ToString() != "")
                    {
                        GlobalVariables.AccountStatus = 1;
                        btnDelete.Text = "Activate";
                    }
                    else
                    {
                        GlobalVariables.AccountStatus = 2;
                        btnDelete.Text = "Deactivate";
                    }
                }
            }
            catch (Exception exc)
            { MessageBox.Show(exc.ToString()); }
            finally
            {
                connection.Close();
            }


        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            UpdateDeleted();
        }

        private void cmbDept_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerateSections();
        }

        private void GenerateUserRights()
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

            string query = "SELECT UserRights.* "+
                           "FROM UserRights "+
                           "WHERE(((UserRights.DeletedDate)Is Null)); ";

            var command = new OleDbCommand(query, connection);
            try 
            { 
                connection.Open();
                var reader = command.ExecuteReader();
                var dTable = new DataTable();
                dTable.Load(reader);
                foreach (DataRow row in dTable.Rows)
                {
                    cmbUserRights.Items.Add(row["Rights"]);
                }
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
            finally { connection.Close(); }
        }

        private void GenerateSections()
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

            string query = "SELECT Sections.* "+
                           "FROM Sections INNER JOIN Departments ON Sections.DepartmentID = Departments.DepartmentID "+
                           "WHERE(((Departments.DepartmentName) = @Department)); ";

            var command = new OleDbCommand(query, connection);
            try
            {
                connection.Open();
                command.Parameters.Add("@Department", OleDbType.VarChar).Value = cmbDept.Text;
                var reader = command.ExecuteReader();
                var dTable = new DataTable();
                dTable.Load(reader);
                cmbSect.Items.Clear();
                foreach (DataRow row in dTable.Rows)
                {
                    cmbSect.Items.Add(row["SectionName"]);
                }
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
            finally { connection.Close(); }

        }

        private void GenerateDepartments()
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

            string query = "SELECT * " +
                           "FROM Departments " +
                           "WHERE(((Departments.DeletedDate)Is Null) AND (Not (Departments.DepartmentID)=(99))); ";
           
            var command = new OleDbCommand(query, connection);
            try
            {
                connection.Open();
                var reader = command.ExecuteReader();
                var dTable = new DataTable();
                dTable.Load(reader);
                foreach (DataRow row in dTable.Rows)
                {
                    cmbDept.Items.Add(row["DepartmentName"]);
                }
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
            finally { connection.Close(); }
        }

        private void GenerateEmployeeID()
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

            string query = "SELECT Max(Users.EmployeeID) AS EmployeeIDOfMax " +
                            "FROM Users " +
                            "WHERE(((Users.UserRightsID) = 2 Or(Users.UserRightsID) = 3)); ";

            var command = new OleDbCommand(query, connection);
            try
            {
                connection.Open();
                var maxID = command.ExecuteScalar();
                int intMaxID = Convert.ToInt32(maxID);
                txtMaxID.Text = (intMaxID + 1).ToString();
                
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
            finally { connection.Close(); }
        }

        private void InsertUsers(OleDbConnection connection,OleDbTransaction trans) 
        {
            string strRights = "SELECT UserRights.* "+
                               "FROM UserRights "+
                               "WHERE(((UserRights.Rights) =@Rights));";


            string strInsertUsers = "Insert into Users values " +
                                "(" +
                                "@EmployeeID," +
                                "@Username," +
                                "@Password," +
                                "@UserRightsID," +
                                "date()," +
                                "date()," +
                                "null," +
                                "@Updatedby)";

            var rightsCommand = new OleDbCommand(strRights, connection,trans);
            rightsCommand.Parameters.Add("@Rights", OleDbType.VarChar).Value = cmbUserRights.Text;
            OleDbDataReader rightsReader = rightsCommand.ExecuteReader();

            var command = new OleDbCommand(strInsertUsers, connection, trans);
            while (rightsReader.Read())
            {
                command.Parameters.Add("@EmployeeID", OleDbType.VarChar).Value = txtMaxID.Text;
                command.Parameters.Add("@Username", OleDbType.VarChar).Value = txtUser.Text;
                command.Parameters.Add("@Password", OleDbType.VarChar).Value = txtPassword.Text;
                command.Parameters.Add("@UserRightsID", OleDbType.Integer).Value = Convert.ToInt32(rightsReader["UserRightsID"]);
                command.Parameters.Add("@Updatedby", OleDbType.VarChar).Value = GlobalVariables.UserID;
                command.ExecuteNonQuery();
            }
        }

        private void InsertEmployees(OleDbConnection connection, OleDbTransaction trans)
        {
            int intDeptID = 0;
            int intSectionID = 0;

            string strDept = "SELECT Departments.DepartmentID, Sections.SectionID "+
                             "FROM Sections INNER JOIN Departments ON Sections.DepartmentID = Departments.DepartmentID "+
                             "WHERE(((Departments.DepartmentName) = @Department) AND((Sections.SectionName) = @Section)); ";
            var deptcommand = new OleDbCommand(strDept, connection, trans);
            deptcommand.Parameters.Add("@Department", OleDbType.VarChar).Value = cmbDept.Text;
            deptcommand.Parameters.Add("@Section", OleDbType.VarChar).Value = cmbSect.Text;
            var deptReader = deptcommand.ExecuteReader();

            while (deptReader.Read())
            {
                intDeptID = Convert.ToInt32(deptReader["DepartmentID"]);
                intSectionID =Convert.ToInt32(deptReader["SectionID"]);
            }

           string strInsert = "Insert into Employees values ( " +
                "@EmployeeID, " +
                "@EmployeeName, " +
                "@Department," +
                "@Section," +
                "date(), " +
                "date(), " +
                "null, " +
                "@Updatedby)";

            if (intDeptID != 0 && intSectionID != 0)
            {
                var command = new OleDbCommand(strInsert, connection, trans);
                command.Parameters.Add("@EmployeeID", OleDbType.VarChar).Value = txtMaxID.Text;
                command.Parameters.Add("@EmployeeName", OleDbType.VarChar).Value = txtFullName.Text;
                command.Parameters.Add("@Department", OleDbType.Integer).Value = intDeptID;
                command.Parameters.Add("@Section", OleDbType.Integer).Value = intSectionID;
                command.Parameters.Add("@Updatedby", OleDbType.VarChar).Value = GlobalVariables.UserID;
                command.ExecuteNonQuery();
            }
        }

        private void UpdateUsers(OleDbConnection connection, OleDbTransaction trans)
        {
            string strRights = "SELECT UserRights.* " +
                                "FROM UserRights " +
                                "WHERE(((UserRights.Rights) =@Rights));";


            string strUpdate = "UPDATE Users SET Users.Username = @Username, " +
                                "Users.Password = @Password, " +
                                "Users.UserRightsID = @UserRights, " +
                                "Users.UpdatedDate = date(), " +
                                "Users.UpdatedBy = @Updatedby " +
                                "WHERE Users.EmployeeID  = @EmployeeID ; ";



            var rightsCommand = new OleDbCommand(strRights, connection, trans);
            rightsCommand.Parameters.Add("@Rights", OleDbType.VarChar).Value = cmbUserRights.Text;
            OleDbDataReader rightsReader = rightsCommand.ExecuteReader();

            var command = new OleDbCommand(strUpdate, connection, trans);
            while (rightsReader.Read())
            {
                command.Parameters.Add("@Username", OleDbType.VarChar).Value = txtUser.Text;
                command.Parameters.Add("@Password", OleDbType.VarChar).Value = txtPassword.Text;
                command.Parameters.Add("@UserRights", OleDbType.Integer).Value = Convert.ToInt32(rightsReader["UserRightsID"]);
                command.Parameters.Add("@Updatedby", OleDbType.VarChar).Value = GlobalVariables.UserID;
                command.Parameters.Add("@EmployeeID", OleDbType.VarChar).Value = txtMaxID.Text;
                command.ExecuteNonQuery();
            }

        }

        private void UpdateEmployees(OleDbConnection connection, OleDbTransaction trans)
        {
            int intDeptID = 0;
            int intSectionID = 0;

            string strDept = "SELECT Departments.DepartmentID, Sections.SectionID " +
                             "FROM Sections INNER JOIN Departments ON Sections.DepartmentID = Departments.DepartmentID " +
                             "WHERE(((Departments.DepartmentName) = @Department) AND((Sections.SectionName) = @Section)); ";
            var deptcommand = new OleDbCommand(strDept, connection, trans);
            deptcommand.Parameters.Add("@Department", OleDbType.VarChar).Value = cmbDept.Text;
            deptcommand.Parameters.Add("@Section", OleDbType.VarChar).Value = cmbSect.Text;
            var deptReader = deptcommand.ExecuteReader();

            while (deptReader.Read())
            {
                intDeptID = Convert.ToInt32(deptReader["DepartmentID"]);
                intSectionID = Convert.ToInt32(deptReader["SectionID"]);
            }

            string strInsert = "UPDATE Employees SET " +
                                "Employees.EmployeeName = @EmployeeName, " +
                                "Employees.DepartmentID = @Department, " +
                                "Employees.SectionID = @Section, " +
                                "Employees.UpdatedDate =date(), " +
                                "Employees.UpdatedBy = @Updatedby " +
                                "WHERE(((Employees.EmployeeID) = @EmployeeID)); ";


            if (intDeptID != 0 && intSectionID != 0)
            {
                var command = new OleDbCommand(strInsert, connection, trans);
                command.Parameters.Add("@EmployeeName", OleDbType.VarChar).Value = txtFullName.Text;
                command.Parameters.Add("@Department", OleDbType.Integer).Value = intDeptID;
                command.Parameters.Add("@Section", OleDbType.Integer).Value = intSectionID;
                command.Parameters.Add("@Updatedby", OleDbType.VarChar).Value = GlobalVariables.UserID;
                command.Parameters.Add("@EmployeeID", OleDbType.VarChar).Value = txtMaxID.Text;
                command.ExecuteNonQuery();
            }
        }

        private void UpdateDeleted() 
        {
            OleDbCommand command;

            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

            string strActivate = "UPDATE Employees SET Employees.DeletedDate = Null "+
                                 "WHERE(((Employees.EmployeeID) = @EmployeeID ));";
            string strDeactivate = "UPDATE Employees SET Employees.DeletedDate = date() " +
                     "WHERE(((Employees.EmployeeID) = @EmployeeID ));";

            if (GlobalVariables.AccountStatus==1)
            {
                command = new OleDbCommand(strActivate, connection);
            }
            else
            {
                command = new OleDbCommand(strDeactivate, connection);
            }

            try
            {
                connection.Open();
                command.Parameters.Add("@EmployeeID", OleDbType.VarChar).Value = txtMaxID.Text;
                command.ExecuteNonQuery();
                if (GlobalVariables.AccountStatus == 1)
                {
                    MessageBox.Show("Account Activated!");
                    this.Close();
                }
                else
                { 
                    MessageBox.Show("Account Deactivated!");
                    this.Close();
                }
            }
            catch (Exception exc)
            { MessageBox.Show(exc.ToString()); }
            finally
            { connection.Close(); }
        }

    }
}

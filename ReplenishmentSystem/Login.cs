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
using System.IO;
using System.Reflection;

namespace ReplenishmentSystem
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtUser.Text != "" || txtPassword.Text != "")
            {
                CheckEmployee();
            }
            else
            { MessageBox.Show("Input Data!"); }
        }
        private void CheckEmployee()
        {
            var connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);

            string query = "SELECT Departments.DepartmentID, Departments.DepartmentName, Sections.SectionID, Sections.SectionName, Employees.EmployeeID, Employees.EmployeeName,Users.UserRightsID " +
                           "FROM Sections INNER JOIN((Employees INNER JOIN Departments ON Employees.DepartmentID = Departments.DepartmentID) INNER JOIN Users ON Employees.EmployeeID = Users.EmployeeID) ON(Sections.SectionID = Employees.SectionID) AND(Sections.DepartmentID = Departments.DepartmentID) "+
                           "WHERE(((Users.Username) =@UserName) AND((Users.Password) = @Password) AND((Employees.DeletedDate)Is Null)); ";

            var command = new OleDbCommand(query, connection);
            try
            {

                connection.Open();

                command.Parameters.Add("@UserName", OleDbType.VarChar).Value = txtUser.Text;
                command.Parameters.Add("@Password", OleDbType.VarChar).Value = txtPassword.Text;
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    GlobalVariables.UserID= reader["EmployeeID"].ToString();
                    GlobalVariables.UserRights = Convert.ToInt32(reader["UserRightsID"].ToString());
                    GlobalVariables.EmployeeName = reader["EmployeeName"].ToString();
                    GlobalVariables.DepartmentID = Convert.ToInt32(reader["DepartmentID"]);
                    GlobalVariables.DepartmentName = reader["DepartmentName"].ToString();
                    GlobalVariables.SectionID = Convert.ToInt32(reader["SectionID"]);
                    GlobalVariables.SectionName = reader["SectionName"].ToString();

                    this.Hide();

                    Form MainMenu = new MainMenu();
                    MainMenu.ShowDialog();

                    txtUser.Text = "";
                    txtPassword.Text = "";
                    this.Show();
                    txtUser.Focus();

                }
                else
                {
                    MessageBox.Show("Invalid User!");
                    txtUser.Text = "";
                    txtPassword.Text = "";
                    txtUser.Focus();
                }

            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
            finally { connection.Close(); }
        }

        private void Login_Load(object sender, EventArgs e)
        {

            //string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            //GlobalVariables.DBFile=
            //MessageBox.Show(appPath);
        }
    }
}

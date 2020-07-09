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
    public partial class RequestItems : Form
    {
        OleDbConnection connection = new OleDbConnection(ReplenishmentSystem.Properties.Settings.Default.ConnectionString);
        public RequestItems()
        {
            InitializeComponent();
        }

        private void RequestItems_Load(object sender, EventArgs e)
        {
            generateKind();
        }

        private void generateKind()
        {
            string strTypes = "SELECT Types.* " +
                  "FROM Types " +
                  "WHERE(((Types.DeletedDate)Is Null)); ";

            var command = new OleDbCommand(strTypes, connection);
            try
            {
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    cmbKind.Items.Add(reader["Type"].ToString());
                }

            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
            finally
            {
                connection.Close();
            }
        }

        private void cmbKind_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strItems = "SELECT Items.* " +
              "FROM Items INNER JOIN Types ON Items.TypeID = Types.TypeID " +
              "WHERE(((Items.DeletedDate)Is Null) AND((Types.Type) = @Type)); ";
            var command = new OleDbCommand(strItems, connection);
            try
            {
                connection.Open();
                command.Parameters.Add("@Type", OleDbType.VarChar).Value = cmbKind.Text;
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

        private void cmbItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strItems = "SELECT Items.*, ItemStocks.MinStocks, ItemStocks.CurrentStocks, ItemStocks.MaxStocks, Units.Unit " +
                              "FROM ItemStocks INNER JOIN(Items INNER JOIN Units ON Items.UnitID = Units.UnitID) ON ItemStocks.ItemID = Items.ItemID " +
                              "WHERE(((Items.ItemDescription) = @Item)); ";

            var command = new OleDbCommand(strItems, connection);
            try
            {
                connection.Open();
                command.Parameters.Add("@Item", OleDbType.VarChar).Value = cmbItems.Text;
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    txtItemID.Text = reader["ItemID"].ToString();
                    txtPrice.Text = reader["Price"].ToString();
                    txtUnit.Text = reader["Unit"].ToString();
                    txtMax.Text = reader["MaxStocks"].ToString();
                    txtCur.Text = reader["CurrentStocks"].ToString();
                    txtMin.Text = reader["MinStocks"].ToString();
                }

            }
            catch(Exception exc)
            { MessageBox.Show(exc.ToString()); }
            finally { connection.Close(); }
        }

        private void txtQty_TextChanged(object sender, EventArgs e)
        {
            if(txtQty.Text!="")
            {
                try
                {
                    txtTotal.Text = (Convert.ToInt32(txtPrice.Text) * Convert.ToInt32(txtQty.Text)).ToString();
                }
                catch
                { MessageBox.Show("Invalid Input!"); }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cmbKind.Text == "" || cmbItems.Text == "" || txtQty.Text == "")
            { MessageBox.Show("Input Fields!"); }
            else
            { 
                Request.dRequestTable.Rows.Add(cmbKind.Text,txtItemID.Text,cmbItems.Text,txtPrice.Text,txtQty.Text,txtUnit.Text,txtTotal.Text);
                this.Close();
            }
        }

    }
}

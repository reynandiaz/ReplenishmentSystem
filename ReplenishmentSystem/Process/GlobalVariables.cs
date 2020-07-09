using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;

namespace ReplenishmentSystem.Process
{
    public class GlobalVariables
    {
        public static string DBFile { get; set; }

        public static string UserID { get; set; }

        public static int UserRights { get; set; }

        public static string EmployeeName { get; set; }

        public static int DepartmentID { get; set; }

        public static string DepartmentName { get; set; }

        public static int SectionID { get; set; }

        public static string SectionName { get; set; }

        public static string UpdateUserID { get; set; }

        public static int AccountStatus { get; set; }
    }
}

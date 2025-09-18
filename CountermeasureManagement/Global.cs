using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountermeasureManagement
{
    public static class Global
    {
        public static string Name { get; set; }
        public static string User { get; set; }
        public static string Level { get; set; }
        public static string InsertOrDelete; 
        public static List<DataRecord> dataRecords = new List<DataRecord>();
        public static bool CheckExecuteQueryMySql = false;
        public static string MessageErrorExecuteQueryMySql;
        public static string ImageUrl;
    }
}

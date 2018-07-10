using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.CommandStringProviders
{
    public class UpdateDateCommandStringProvider : ICommandStringProvider
    {
        public string GetCommandString(string connectionString, string tableName)
        {
            String cmdStr = null;

            String getDateColsCmdStr = String.Format("SELECT Column_Name FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{0}' AND data_type IN ('{1}','{2}')", tableName, Constants.SqlTypeNames.Date, Constants.SqlTypeNames.DateTime);
            DataTable dt = Helper.GetSqlDataTable(connectionString, getDateColsCmdStr);
            if (dt.Rows.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("update " + tableName + " SET ");
                String comma = String.Empty;
                foreach (DataRow row in dt.Rows)
                {
                    sb.Append(comma);
                    comma = ", ";
                    String clause = String.Format(" {0} = (CASE WHEN {0} <= '{1}' THEN NULL ELSE {0} END)", Helper.GetDestinationColumnName(row[0].ToString()),Constants.SqlDateMinValue);
                    sb.Append(clause);
                }
                cmdStr = sb.ToString();

            }
            return cmdStr;
        }
    }
}

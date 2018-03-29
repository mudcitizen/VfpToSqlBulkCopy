using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public class DefaultCommandStringProvider : ICommandStringProvider
    {
        public string GetCommandString(string connectionName, string tableName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select ");
            Dictionary<String, OleDbColumnDefinition> schema = new OleDbSchemaProvider().GetSchema(connectionName, tableName);
            String comma = String.Empty;
            foreach (KeyValuePair<String, OleDbColumnDefinition> kvp in schema)
            {
                sb.Append(comma + kvp.Value.Name);
                comma = ", ";

                // When I had the IIF(EMPTY(msClDate),null,msClDate) as msClDate 
                // VFP blew chow with Error # 1890
                // And you get the same thing in the VFP Dev Edition if you issue
                // SELECT IIF(EMPTY(mscldate),null,mscldate) FROM in_msg using 
                // the Laptop
                //if (kvp.Value.IsDate)
                //{
                //    String s = String.Format("IIF(EMPTY({0}),null,{0}) as {0}", kvp.Value.Name);
                //    sb.Append(s);
                //}
                //else
                //{
                //    sb.Append(kvp.Value.Name);
                //}
            }
            sb.Append(comma + "IIF(DELETED(),1,0) AS " + Constants.DILayer.DeletedColumnName);
            sb.Append(" from " + tableName);
            return sb.ToString();
        }
    }
}

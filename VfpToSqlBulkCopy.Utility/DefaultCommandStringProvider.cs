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
            /*
             * The saga of deleted()....  1st I tried the following
              
            sb.Append(comma + "IIF(DELETED(),1,0) AS " + Constants.DILayer.DeletedColumnName);

            A problem with the above is the the column was preceived as having a Decimal type - which 
            makes sense.  While the values where either 0 or 1 nothing coerced the type to boolean
            
            So I tried the following .... 

            sb.Append(comma + " DELETED() AS " + Constants.DILayer.DeletedColumnName);

            This comand resulted in a "correct" DataTable in that it had an SqlDeleted column
            of type boolean

            However, it still didn't work.  The following is the bulk insert command and it knows nothing about SqlDeleted c

            insert bulk IN_MSG ([MSNUMB] Char(6) COLLATE Latin1_General_BIN, [MSTYPE] Char(1) COLLATE Latin1_General_BIN, [MSGNUM] Char(6) COLLATE Latin1_General_BIN, 
            [MSRNUM] Char(6) COLLATE Latin1_General_BIN, [MSDATE] Date, [MSTIME] Char(5) COLLATE Latin1_General_BIN, [MSUSER] Char(2) COLLATE Latin1_General_BIN, [MSCLDATE] Date, 
            [MSCLTIME] Char(5) COLLATE Latin1_General_BIN, [MSCLUSER] Char(2) COLLATE Latin1_General_BIN, [MSPRINT] Char(1) COLLATE Latin1_General_BIN, 
            [MSTO] Char(3) COLLATE Latin1_General_BIN, [MSFLAG] Char(1) COLLATE Latin1_General_BIN, [MSNOTTYP] Char(1) COLLATE Latin1_General_BIN, [MSBEGIN] Date,
            [MSDAY] Decimal(2,0), [MSBEGTIME] Char(5) COLLATE Latin1_General_BIN, [MSENDTIME] Char(5) COLLATE Latin1_General_BIN, 
            [MSTXT] Text COLLATE SQL_Latin1_General_CP1_CI_AS, [MSPRI] Decimal(2,0), [MSSTAT] Char(1) COLLATE Latin1_General_BIN, [MSIPADDR] Char(15) COLLATE Latin1_General_BIN, [MSAUTOCLS] Bit)
              
             */
            sb.Append(" from " + tableName);
            return sb.ToString();
        }
    }
}

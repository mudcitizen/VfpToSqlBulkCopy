using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.TableProcessors
{
    public class NullCharacterScrubber : ITableProcessor
    {
        public void Process(string sourceConnectionString, string sourceTableName, string destinationConnectionString, string destinationTableName)
        {
            destinationTableName = Helper.GetDestinationTableName(destinationTableName);

            StringBuilder updateClause = new StringBuilder();
            StringBuilder whereClause = new StringBuilder();
            String comma = String.Empty;
            String or = String.Empty;

            /*
             * Originally I used the VFP schema because I didn't want to deal with computed columns.  But when it came 
             * time to test I switched to the Sql Schema.  The code that tests is all based on a table that is created by
             * the unit test.   The Unit Test already had code to create / populate the table in SQL and I didn't want to
             * add stuff to create a table in VFP etc... 
            */
            String cmdStr = "SELECT column_name,DATA_TYPE FROM INFORMATION_SCHEMA.Columns where Table_Name = '" + destinationTableName + "'";
            DataTable dt = Helper.GetSqlDataTable(destinationConnectionString, cmdStr);

            int charTypeRows = 0;
            foreach (DataRow row in dt.Rows)
            {
                String colName = row[0].ToString();
                if (colName.Equals(Constants.DILayer.RecnoColumnName, StringComparison.InvariantCultureIgnoreCase))
                    break;

                if (row[1].ToString().Equals(Constants.SqlTypeNames.Char, StringComparison.InvariantCultureIgnoreCase))
                {

                    charTypeRows++;
                    colName = Helper.GetDestinationColumnName(colName);
                    updateClause.Append(comma + String.Format("{0} = REPLACE({0},CHAR(0),' ')", colName));
                    comma = ",";
                    whereClause.Append(or + String.Format("CHARINDEX(CHAR(0),{0}) > 0", colName));
                    or = " OR ";
                }
            }

            if (charTypeRows > 0)
            {
                cmdStr = String.Format("UPDATE {0} SET {1} WHERE SqlRecNo IN (SELECT SqlRecNo FROM {0} WHERE {2})", destinationTableName, updateClause.ToString(), whereClause.ToString());
                Helper.ExecuteSqlNonQuery(destinationConnectionString, cmdStr);
            }

        }
    }
}

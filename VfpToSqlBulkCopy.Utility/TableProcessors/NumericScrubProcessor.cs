using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.TableProcessors
{
    /*
    
        VFP allows you to store values in numeric columns that don't match 
        the column definition.  You can have the wrong number of decimal
        places and you can "overflow" - storing a value too large for the
        the column

        This class corrects the above problems... 
    */
    
    public class NumericScrubProcessor : ITableProcessor
    {
        // This is just a debugging thing....
        public IList<String> CommandStrings { get; }

        public NumericScrubProcessor()
        {
            CommandStrings = new List<String>();
        }
        public void Process(string sourceConnectionString, string sourceTableName, string destinationConnectionString, string destinationTableName)
        {
            OleDbSchemaProvider schemaProvider = new OleDbSchemaProvider();
            Dictionary<String, OleDbColumnDefinition> schema = schemaProvider.GetSchema(sourceConnectionString, sourceTableName);


            IEnumerable<OleDbColumnDefinition> numericColDefs = schema.Values.Where(colDef => colDef.Type == System.Data.OleDb.OleDbType.Numeric).ToList();

            String comma = String.Empty;
            StringBuilder sb = new StringBuilder().Append("SELECT ");
            // if we can read all the numerics we're good-to-go 
            foreach (OleDbColumnDefinition colDef in numericColDefs)
            {
                sb.Append(comma + colDef.Name);
                comma = ",";
            }

            sb.Append(" FROM " + sourceTableName);

            if (!TryRead(sourceConnectionString, sb.ToString()))
            {
                // go at it column by column. If we can't read it then udpate it...
                foreach (OleDbColumnDefinition colDef in numericColDefs)
                {
                    String cmdStr = String.Format("SELECT {0} FROM {1}", colDef.Name, sourceTableName);
                    if (!TryRead(sourceConnectionString, cmdStr))
                    {
                        String maxVal;
                        if (colDef.NumericScale > 0)
                        {
                            // 4,2 - -9.99 - 99.99
                            maxVal = new String('9', (int)(colDef.NumericPrecision - colDef.NumericScale)) + "." + new String('9', (int)colDef.NumericScale);
                        }
                        else
                        {
                            maxVal = new String('9', (int)(colDef.NumericPrecision));
                        }
                        cmdStr = String.Format("UPDATE {0} SET {1} = 0 WHERE NOT BETWEEN({1},-{2},{2})", sourceTableName, colDef.Name, maxVal);
                        CommandStrings.Add(cmdStr);
                        Helper.ExecuteOleDbNonQuery(sourceConnectionString, cmdStr);
                    }                    
                }
            }

            return;
        }

        private Boolean TryRead(String connectionString, String commandString)
        {
            Boolean success = false;
            CommandStrings.Add(commandString);
            try
            {
                DataTable dt = Helper.GetOleDbDataTable(connectionString, commandString);
                success = true;
            }
            catch (InvalidOperationException)
            {
            }
            catch (System.Data.OleDb.OleDbException)
            {
            }
            return success;
            }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
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

        private IBatchSizeProvider BatchSizeProvider;

        public NumericScrubProcessor() : this(null) { }

        public NumericScrubProcessor(IBatchSizeProvider batchSizeProvider)
        {
            CommandStrings = new List<String>();
            BatchSizeProvider = batchSizeProvider ?? new DefaultBatchSizeProvider();
        }

        public void Process(string sourceConnectionString, string sourceTableName, string destinationConnectionString, string destinationTableName)
        {
            OleDbSchemaProvider schemaProvider = new OleDbSchemaProvider();
            Dictionary<String, OleDbColumnDefinition> schema = schemaProvider.GetSchema(sourceConnectionString, sourceTableName);

            IEnumerable<OleDbColumnDefinition> numericColDefs = schema.Values.Where(colDef => colDef.Type == System.Data.OleDb.OleDbType.Numeric).ToList();
            if (numericColDefs.Count() == 0)
                return;

            VfpConnectionStringBuilder vfpConnStrBldr = new VfpConnectionStringBuilder(sourceConnectionString);
            sourceConnectionString = vfpConnStrBldr.ConnectionString;

            int batchSize = BatchSizeProvider.GetBatchSize(sourceTableName);
            int recordCount = Convert.ToInt32(Helper.GetOleDbScaler(sourceConnectionString, "SELECT COUNT(*) FROM " + sourceTableName));

            using (OleDbConnection sourceConnection = new OleDbConnection(sourceConnectionString))
            {
                sourceConnection.Open();

                int minRecno, maxRecno, recsProcessed;
                recsProcessed = 0;

                String comma = String.Empty;
                String columnList = String.Empty;

                StringBuilder sb = new StringBuilder().Append("SELECT ");
                foreach (OleDbColumnDefinition colDef in numericColDefs)
                {
                    sb.Append(comma + colDef.Name);
                    comma = ",";
                }

                sb.Append(" FROM " + sourceTableName + " WHERE ");

                String selectAllColsCmdStr = sb.ToString();

                while (true)
                {
                    minRecno = recsProcessed;
                    maxRecno = minRecno + batchSize;

                    // SELECT <all numeric cols> FROM <> RECNO() > 0 and RECNO() <= 25000

                    if (!TryRead(sourceConnectionString, String.Format(selectAllColsCmdStr + GetRecNoWhereClause(minRecno, maxRecno))))
                    {
                        // if we cant read all the numerics we're good-to-go.  Otherwise we'll go column-by-column
                        foreach (OleDbColumnDefinition colDef in numericColDefs)
                        {
                            String recnoWhereClause = GetRecNoWhereClause(minRecno, maxRecno);
                            String cmdStr = String.Format("SELECT {0} FROM {1} WHERE {2}", colDef.Name, sourceTableName, recnoWhereClause);
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
                                cmdStr = String.Format("UPDATE {0} SET {1} = 0 WHERE NOT BETWEEN({1},-{2},{2}) AND {3}", sourceTableName, colDef.Name, maxVal, recnoWhereClause);
                                CommandStrings.Add(cmdStr);
                                Helper.ExecuteOleDbNonQuery(sourceConnectionString, cmdStr);
                            }
                        }
                    }

                    recsProcessed = recsProcessed + batchSize;
                    if (recsProcessed >= recordCount)
                        break;
                }

                sourceConnection.Close();
            }

            return;

        }

        String GetRecNoWhereClause(int minRecNo, int maxRecNo)
        {
            return String.Format(" RECNO() > {0} AND RECNO() <= {1} ", Convert.ToString(minRecNo), Convert.ToString(maxRecNo));
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

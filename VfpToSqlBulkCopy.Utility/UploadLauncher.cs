using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public class UploadLauncher
    {

        public void Launch()
        {
            IList<String> requiredConnectionNames = new List<String>() { Constants.ConnectionNames.Host, Constants.ConnectionNames.Sql };
            foreach (String connName in requiredConnectionNames)
            {
                if (ConfigurationManager.ConnectionStrings[connName] == null)
                {
                    throw new ApplicationException("No connection string found for connection name " + connName);
                }
            }

            IList<String> vfpConnectionNames = new List<String> { Constants.ConnectionNames.Host };
            if (Helper.GetConnectionString(Constants.ConnectionNames.POS) == null)
                vfpConnectionNames.Add(Constants.ConnectionNames.POS);

            TableProcessor tableProcessor = new TableProcessor();

            // TODO - Find a better way of logging progres..... 
            foreach (String vfpConnectionName in vfpConnectionNames)
            {
                IEnumerable<String> tableNames = GetTables(vfpConnectionName);
                foreach (String tableName in tableNames)
                {
                    String msg = String.Format("{0} - {1}",DateTime.Now.ToShortTimeString(), tableName);
                    Debug.WriteLine(msg);
                    tableProcessor.Upload(vfpConnectionName, tableName, Constants.ConnectionNames.Sql, tableName.Replace('-', '_'));
                }

            }

        }

        IEnumerable<String> GetTables(String connectionName)
        {
            IList<String> tables = new List<String>();
            String connStr = Helper.GetConnectionString(connectionName);

            if (connStr != null)
            {

                String cmdStr = String.Format("SELECT TABLE FROM DITABLE WHERE UPPER(ClassName) NOT LIKE 'V%' AND IndxDbf LIKE '{0}%' ORDER BY TABLE", connectionName == Constants.ConnectionNames.Host ? 'I' : 'P');
                DataTable dt = Helper.GetOleDbDataTable(Constants.ConnectionNames.Host, cmdStr);

                OleDbConnectionStringBuilder bldr = new OleDbConnectionStringBuilder(Helper.GetConnectionString(connectionName));
                Object obj = null;
                bldr.TryGetValue("Data Source", out obj);

                if (obj == null)
                    throw new ApplicationException("No Data Source for connection name " + connectionName);

                String directoryName = obj.ToString();

                if (!Directory.Exists(directoryName))
                    throw new ApplicationException("Data Source does not exist for connection name " + connectionName);

                // TODO Ensure connectionString has DELETED=False
                foreach (DataRow row in dt.Rows)
                {
                    String tableName = row[0].ToString().Trim();
                    String fileName = Path.Combine(directoryName, tableName + ".DBF");
                    if (File.Exists(fileName))
                        tables.Add(tableName);
                }
            }

            return tables;
        }
    }



}

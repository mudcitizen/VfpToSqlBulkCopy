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

        IDictionary<String, String> ConnectionStrings;
        String SqlConnectionString;
        String HostConnectionString;
        RestartParameter RestartParm;
        public TableProcessor TableProcessor { get; set; }


        public UploadLauncher(IDictionary<String, String> connStrs) : this(connStrs, null) { }

        public UploadLauncher(IDictionary<String, String> connStrs, RestartParameter restartDetails)
        {
            ConnectionStrings = new Dictionary<String, String>();
            IList<String> connectionNames = new List<String>() { Constants.ConnectionNames.Host.ToUpper(), Constants.ConnectionNames.POS.ToUpper(), Constants.ConnectionNames.Sql.ToUpper() };
            foreach (KeyValuePair<String, String> kvp in connStrs)
            {
                String key = kvp.Key.ToUpper();
                if (connectionNames.Contains(key))
                    ConnectionStrings.Add(key, kvp.Value);
            }

            IList<String> requiredConnectionNames = new List<String>() { Constants.ConnectionNames.Host.ToUpper(), Constants.ConnectionNames.Sql.ToUpper() };
            foreach (String connName in requiredConnectionNames)
            {
                if (!ConnectionStrings.ContainsKey(connName))
                {
                    throw new ApplicationException("No connection string found for connection name " + connName);
                }
            }

            foreach (KeyValuePair<String, String> kvp in ConnectionStrings)
            {
                if (kvp.Key.Equals(Constants.ConnectionNames.Sql, StringComparison.InvariantCultureIgnoreCase))
                {
                    SqlConnectionString = kvp.Value;
                }
                if (kvp.Key.Equals(Constants.ConnectionNames.Host, StringComparison.InvariantCultureIgnoreCase))
                {
                    HostConnectionString = kvp.Value;
                }
            }

            RestartParm = restartDetails;
            TableProcessor = new TableProcessor();

        }

        public void Launch()
        {

            ITableNameProvider tableNameProvider;
            if (RestartParm == null)
                tableNameProvider = new TableNameProvider(HostConnectionString);
            else
                tableNameProvider = new TableNameProvider(HostConnectionString,RestartParm.SatisfiesFilter);

            foreach (KeyValuePair<String, String> kvp in ConnectionStrings)
            {
                if (!kvp.Key.Equals(Constants.ConnectionNames.Sql, StringComparison.InvariantCultureIgnoreCase))
                {
                    // TODO - Find a better way of logging progres..... 
                    IEnumerable<String> tableNames = tableNameProvider.GetTables(kvp.Key, kvp.Value);
                    foreach (String tableName in tableNames)
                    {
                        String msg = String.Format("{0} - {1} - Begin", DateTime.Now.ToShortTimeString(), tableName);
                        //Debug.WriteLine(msg);
                        TableProcessor.Process(kvp.Value, tableName, SqlConnectionString, tableName.Replace('-', '_'));
                        msg = String.Format("{0} - {1} - End", DateTime.Now.ToShortTimeString(), tableName);
                        //Debug.WriteLine(msg);
                    }
                }
            }
        }
    }



}

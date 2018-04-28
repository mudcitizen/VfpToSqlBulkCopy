using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using VfpToSqlBulkCopy.Utility;
using VfpToSqlBulkCopy.Utility.VfpToSqlBulkCopy.Utility.EventHandlers;

namespace VfpToSqlBulkCopy.Console
{
    public class Uploader
    {
        TableProcessorBeginEventArgs UploadBeginEventArgs;
        IDictionary<String, Exception> UploadExceptions;

        public void Upload()
        {
            IDictionary<String, String> connStrs = new Dictionary<String, String>();
            String connName;
            connName = VfpToSqlBulkCopy.Utility.Constants.ConnectionNames.Host;
            connStrs.Add(connName, GetConnectionString(connName,true));
            connName = VfpToSqlBulkCopy.Utility.Constants.ConnectionNames.Sql;
            connStrs.Add(connName, GetConnectionString(connName, true));

            connName = VfpToSqlBulkCopy.Utility.Constants.ConnectionNames.POS;
            String connStr = GetConnectionString(connName, false);
            if (!String.IsNullOrEmpty(connStr))
                connStrs.Add(connName, connStr);


            String logFileName = null;
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                logFileName = appSettings[Constants.TableProcessorEventsFileNameAppSettingsKey];
            }
            catch (ConfigurationErrorsException)
            {
                logFileName = "TableProcessorEvents.Log";
            }

            IList<ITableProcessorEventHandler> eventHandlers = new List<ITableProcessorEventHandler>()
            {
                new ConsoleTableProcessorEventHandler(),
                new TextFileITableProcessorEventHandler(logFileName)
            };
            ITableProcessorEventHandler eventHandler = new CompositeTableProcessorEventHandler(eventHandlers);


            UploadLauncher uploadLauncher = new UploadLauncher(connStrs);
            uploadLauncher.TableProcessor.TableProcessorBegin += eventHandler.HandleTableProcessorBegin;
            uploadLauncher.TableProcessor.TableProcessorEnd += eventHandler.HandleTableProcessorEnd;
            uploadLauncher.TableProcessor.TableProcessorException += eventHandler.HandleTableProcessorException;

            uploadLauncher.Launch();

            if (UploadExceptions != null)
            {

                String fileName = "Exceptions.Log" + "_" + DateTime.Now.ToLongTimeString();

                if (File.Exists(fileName))
                    File.Delete(fileName);

                StringBuilder consoleSb = new StringBuilder();
                StringBuilder fileSb = new StringBuilder();
                String header = "Problems were encountered with the following tables" + Environment.NewLine;
                consoleSb.Append(header);
                foreach (KeyValuePair<String, Exception> kvp in UploadExceptions)
                {
                    consoleSb.AppendLine(kvp.Key);
                    fileSb.AppendLine(kvp.Key);
                    fileSb.AppendLine(kvp.Value.ToString());
                    fileSb.AppendLine(String.Empty);
                }
                consoleSb.AppendLine(String.Format("See {0} for details",fileName));

                File.WriteAllText(fileName, fileSb.ToString());
                System.Console.Write(consoleSb.ToString());
            }
            
        }


        private String PadTableName(String tableName)
        {
            return tableName.PadRight(12);
        }

        private String GetConnectionString(String connectionName, Boolean required)
        {
            ConnectionStringSettings css = ConfigurationManager.ConnectionStrings[connectionName];
            if (css != null)
                return css.ConnectionString;

            if (!required)
                return null;
            else
                throw new ApplicationException("No connection string found for " + connectionName);
            
        }
    }
}

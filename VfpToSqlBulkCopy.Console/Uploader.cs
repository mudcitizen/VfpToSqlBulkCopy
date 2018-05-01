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

            IList<IUploadEventHandler> eventHandlers = new List<IUploadEventHandler>()
            {
                new ConsoleEventHandler(),
                new TextFileEventHandler(logFileName)
            };

            IUploadEventHandler eventHandler = new CompositeEventHandler(eventHandlers);

            UploadLauncher uploadLauncher = new UploadLauncher(connStrs);
            uploadLauncher.BeginUpload += eventHandler.HandleUploadBegin;
            uploadLauncher.TableProcessor.TableProcessorBegin += eventHandler.HandleTableProcessorBegin;
            uploadLauncher.TableProcessor.TableProcessorEnd += eventHandler.HandleTableProcessorEnd;
            uploadLauncher.TableProcessor.TableProcessorException += eventHandler.HandleTableProcessorException;
            uploadLauncher.EndUpload += eventHandler.HandleUploadEnd;
            uploadLauncher.Launch();
        
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

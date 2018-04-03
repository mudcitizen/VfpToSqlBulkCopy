using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using VfpToSqlBulkCopy.Utility;


namespace VfpToSqlBulkCopy.Console
{
    public class Uploader
    {
        TableUploadBeginEventArgs UploadBeginEventArgs;
        IDictionary<String, Exception> UploadExceptions;

        public void Upload()
        {
            IDictionary<String, String> connStrs = new Dictionary<String, String>();
            String connName;
            connName = Constants.ConnectionNames.Host;
            connStrs.Add(connName, GetConnectionString(connName,true));
            connName = Constants.ConnectionNames.Sql;
            connStrs.Add(connName, GetConnectionString(connName, true));

            connName = Constants.ConnectionNames.POS;
            String connStr = GetConnectionString(connName, false);
            if (!String.IsNullOrEmpty(connStr))
                connStrs.Add(connName, connStr);

            UploadLauncher uploadLauncher = new UploadLauncher(connStrs);
            uploadLauncher.TableUploader.TableUploadBegin += HandleTableUploadBegin;
            uploadLauncher.TableUploader.TableUploadEnd += HandleTableUploadEnd;

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

        private void HandleTableUploadBegin(Object sender, TableUploadBeginEventArgs args)
        {
            UploadBeginEventArgs = args;
            System.Console.Write(String.Format("{0} - {1}", PadTableName(args.TableName), args.When.ToLongTimeString()));
        }
        private void HandleTableUploadEnd(Object sender, TableUploadEndEventArgs args)
        {
            TimeSpan ts = args.When - UploadBeginEventArgs.When;
            String duration = String.Format("{0:D2}:{1:D2}:{2:D2}:{3:D3}", ts.Hours, ts.Minutes, ts.Seconds,ts.Milliseconds);

            System.Console.WriteLine(" " + args.When.ToLongTimeString() + " " + duration);
        }
        private void HandleTableUploadException(Object sender, TableUploadErrorEventArgs args)
        {
            if (UploadExceptions == null)
                UploadExceptions = new Dictionary<String, Exception>();
            UploadExceptions.Add(args.TableName, args.Exception);
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

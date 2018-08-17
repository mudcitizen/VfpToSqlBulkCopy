using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.Events
{
    public class TextFileEventHandler : IUploadEventHandler
    {
        String FileName;
        TableProcessorBeginEventArgs TableBeginEventArgs;
        TableProcessorBeginEventArgs BeginEventArgs;

        public TextFileEventHandler(String filename)
        {
            FileName = filename;
        }
        public void HandleTableProcessorBegin(object sender, TableProcessorBeginEventArgs args)
        {
            BeginEventArgs = args;
            if ((TableBeginEventArgs == null) || (args.TableName != TableBeginEventArgs.TableName))
                TableBeginEventArgs = args;
        }

        public void HandleTableProcessorEnd(object sender, TableProcessorEndEventArgs args)
        {
            TableProcessorBeginEventArgs beginArgs = (args.ClassName == TableBeginEventArgs.ClassName) ? TableBeginEventArgs : BeginEventArgs;
            TimeSpan ts = args.When - beginArgs.When;
            String duration = String.Format("{0:D2}:{1:D2}:{2:D2}:{3:D3}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            String txt = String.Format("Table - {0} ; Begin - {1} ; End - {2} ; Duration - {3} ; Class - {4}",
                beginArgs.TableName,
                beginArgs.When.ToLongTimeString(),
                args.When.ToLongTimeString(),
                duration,
                beginArgs.ClassName);
            Write(txt);

        }

        public void HandleTableProcessorException(object sender, TableProcessorExceptionEventArgs args)
        {
            Write(args.Exception.ToString());

        }

        public void HandleUploadBegin(object sender, BeginUploadEventArgs args)
        {
            Write(String.Format("Upload Begin - " + DateTime.Now.ToLongTimeString()));
            foreach (String connStr in args.ConnectionStrings)
                Write(connStr);
            Write(String.Format("Restart - {0}", args.RestartParameter == null ? "None" : args.RestartParameter.ToString()));
        }

        public void HandleUploadEnd(object sender, EndUploadEventArgs args)
        {
            Write(String.Format("Upload Complete - " + DateTime.Now.ToLongTimeString()));
        }

        void Write(String txt)
        {
            File.AppendAllText(FileName, txt + Environment.NewLine);
        }
    }
}

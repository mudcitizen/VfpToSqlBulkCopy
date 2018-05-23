using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.Events
{
    public class ConsoleEventHandler : IUploadEventHandler
    {
        TableProcessorBeginEventArgs BeginEventArgs;
        readonly int TableNamePadFactor = 12;
        IList<TableProcessorExceptionEventArgs> ExceptionsArgs;


        public ConsoleEventHandler() { }
        public ConsoleEventHandler(int tableNamePadFactor)
        {
            TableNamePadFactor = tableNamePadFactor;
        }

        public void HandleTableProcessorBegin(object sender, TableProcessorBeginEventArgs args)
        {
            BeginEventArgs = args;
            System.Console.Write(String.Format("{0} - {1}", PadTableName(args.TableName), args.When.ToLongTimeString()));
        }

        public void HandleTableProcessorEnd(object sender, TableProcessorEndEventArgs args)
        {
            TimeSpan ts = args.When - BeginEventArgs.When;
            String duration = String.Format("{0:D2}:{1:D2}:{2:D2}:{3:D3}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            System.Console.WriteLine(" " + args.When.ToLongTimeString() + " " + duration);
        }

        public void HandleTableProcessorException(object sender, TableProcessorExceptionEventArgs args)
        {

            if (ExceptionsArgs == null)
                ExceptionsArgs = new List<TableProcessorExceptionEventArgs>();
            ExceptionsArgs.Add(args);
        }

        private String PadTableName(String tableName)
        {
            return tableName.PadRight(TableNamePadFactor);
        }

        public void HandleUploadBegin(object sender, BeginUploadEventArgs args)
        {
  
        }

        public void HandleUploadEnd(object sender, EndUploadEventArgs args)
        {
            if (ExceptionsArgs != null)
            {
                Console.WriteLine("Exceptions occurred on the following tables");
                Console.ReadKey();
                foreach (TableProcessorExceptionEventArgs arg in ExceptionsArgs)
                {
                    Console.WriteLine("Table - {0} ; Exception - {1}", arg.TableName, arg.Exception);
                    Console.ReadKey();
                }

            }
        }
    }
    
}

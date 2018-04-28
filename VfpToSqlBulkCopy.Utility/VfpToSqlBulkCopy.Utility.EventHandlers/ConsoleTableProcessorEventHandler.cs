using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.VfpToSqlBulkCopy.Utility.EventHandlers
{
    public class ConsoleTableProcessorEventHandler : ITableProcessorEventHandler
    {
        TableProcessorBeginEventArgs BeginEventArgs;
        readonly int TableNamePadFactor = 12;

        public ConsoleTableProcessorEventHandler() { }
        public ConsoleTableProcessorEventHandler(int tableNamePadFactor)
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
            Console.WriteLine(args.TableName + " " + args.Exception.ToString());
        }

        private String PadTableName(String tableName)
        {
            return tableName.PadRight(TableNamePadFactor);
        }

    }
    
}

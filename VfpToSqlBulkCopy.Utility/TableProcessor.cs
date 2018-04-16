using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public class TableProcessor : ITableProcessor
    {
        IEnumerable<ITableProcessor> _TableProcessors;


        public event EventHandler<TableProcessorBeginEventArgs> TableProcessorBegin;
        public event EventHandler<TableProcessorEndEventArgs> TableProcessorEnd;
        public event EventHandler<TableProcessorErrorEventArgs> TableProcessorError;

        public TableProcessor()
        {
            Init(new List<ITableProcessor>() { new ZapProcessor(), new TableUploader(), new AsciiZeroMemoProcessor(), new NullDateProcessor(), new SetDeletedProcessor() });
        }


        public TableProcessor(IEnumerable<ITableProcessor> tableProcessors)
        {
            Init(tableProcessors);

        }
        public void Process(string sourceConnectionString, string sourceTableName, string destinationConnectionString, string destinationTableName)
        {
            OnTableProcessorBegin(sourceTableName);
            try
            {
                foreach (ITableProcessor tp in _TableProcessors)
                {
                    tp.Process(sourceConnectionString, sourceTableName, destinationConnectionString, destinationTableName);
                }
            }
            catch (Exception ex)
            {
                OnTableProcessorError(sourceTableName, ex);
            }
            OnTableProcessorEnd(sourceTableName);
        }

        private void Init(IEnumerable<ITableProcessor> tableProcessors)
        {
            _TableProcessors = tableProcessors;
        }

        #region EventPublishers
        protected virtual void OnTableProcessorBegin(String tableName)
        {
            EventHandler<TableProcessorBeginEventArgs> handler = TableProcessorBegin;
            if (handler != null)
            {
                TableProcessorBeginEventArgs args = new TableProcessorBeginEventArgs(tableName);
                handler(this, args);
            }
        }
        protected virtual void OnTableProcessorEnd(String tableName)
        {
            EventHandler<TableProcessorEndEventArgs> handler = TableProcessorEnd;
            if (handler != null)
            {
                TableProcessorEndEventArgs args = new TableProcessorEndEventArgs(tableName);
                handler(this, args);
            }
        }

        protected virtual void OnTableProcessorError(String tableName, Exception exception)
        {
            EventHandler<TableProcessorErrorEventArgs> handler = TableProcessorError;
            if (handler != null)
            {
                TableProcessorErrorEventArgs args = new TableProcessorErrorEventArgs(tableName, exception);
                handler(this, args);
            }
        }
        #endregion

    }
}

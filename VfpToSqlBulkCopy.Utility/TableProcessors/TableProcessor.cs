using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VfpToSqlBulkCopy.Utility.Events;

namespace VfpToSqlBulkCopy.Utility
{
    public class TableProcessor : ITableProcessor
    {
        IEnumerable<ITableProcessor> _TableProcessors;


        public event EventHandler<TableProcessorBeginEventArgs> TableProcessorBegin;
        public event EventHandler<TableProcessorEndEventArgs> TableProcessorEnd;
        public event EventHandler<TableProcessorExceptionEventArgs> TableProcessorException;

        #region ctor
        public TableProcessor()
        {
            BuildAndInit(new DefaultBatchSizeProvider());
        }

        public TableProcessor(IBatchSizeProvider batchSizeProvider)
        {
            BuildAndInit(batchSizeProvider);
        }


        public TableProcessor(IEnumerable<ITableProcessor> tableProcessors)
        {
            Init(tableProcessors);

        }
        #endregion

        #region Initialize
        private void BuildAndInit(IBatchSizeProvider batchSizeProvider)
        {
            Init(new List<ITableProcessor>() { new ZapProcessor(), new TableUploader(batchSizeProvider), new AsciiZeroMemoProcessor(), new NullDateProcessor(), new SetDeletedProcessor() });
        }
        private void Init(IEnumerable<ITableProcessor> tableProcessors)
        {
            _TableProcessors = tableProcessors;
        }
        #endregion

        public void Process(string sourceConnectionString, string sourceTableName, string destinationConnectionString, string destinationTableName)
        {
            destinationTableName = Helper.GetDestinationTableName(destinationTableName);
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
            EventHandler<TableProcessorExceptionEventArgs> handler = TableProcessorException;
            if (handler != null)
            {
                TableProcessorExceptionEventArgs args = new TableProcessorExceptionEventArgs(tableName, exception);
                handler(this, args);
            }
        }
        #endregion

    }
}

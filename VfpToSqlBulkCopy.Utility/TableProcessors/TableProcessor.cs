using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VfpToSqlBulkCopy.Utility.Events;
using VfpToSqlBulkCopy.Utility;

namespace VfpToSqlBulkCopy.Utility.TableProcessors
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
            Init(new List<ITableProcessor>() { new ZapProcessor(), new NumericScrubProcessor(), new TableUploader(batchSizeProvider), new AsciiZeroMemoProcessor(), new NullCharacterScrubber(), new NullDateProcessor(), new SetDeletedProcessor() });
        }
        private void Init(IEnumerable<ITableProcessor> tableProcessors)
        {
            _TableProcessors = tableProcessors;
        }
        #endregion

        public void Process(string sourceConnectionString, string sourceTableName, string destinationConnectionString, string destinationTableName)
        {
            destinationTableName = Helper.GetDestinationTableName(destinationTableName);
            String thisClassName = GetType().Name;
            OnTableProcessorBegin(new TableProcessorBeginEventArgs(sourceTableName, thisClassName));
            try
            {
                foreach (ITableProcessor tp in _TableProcessors)
                {
                    try
                    {
                        OnTableProcessorBegin(new TableProcessorBeginEventArgs(sourceTableName, tp.GetType().Name));
                        tp.Process(sourceConnectionString, sourceTableName, destinationConnectionString, destinationTableName);
                        OnTableProcessorEnd(new TableProcessorEndEventArgs(sourceTableName, tp.GetType().Name));
                    }
                    catch (Exception ex)
                    {
                        OnTableProcessorError(new TableProcessorExceptionEventArgs(sourceTableName, tp.GetType().Name, ex));
                    }
                }
            }
            catch (Exception ex)
            {
                OnTableProcessorError(new TableProcessorExceptionEventArgs(sourceTableName, thisClassName, ex));
            }
            OnTableProcessorEnd(new TableProcessorEndEventArgs(sourceTableName, thisClassName));
        }




        #region EventPublishers
        protected virtual void OnTableProcessorBegin(TableProcessorBeginEventArgs args)
        {
            EventHandler<TableProcessorBeginEventArgs> handler = TableProcessorBegin;
            if (handler != null)
            {
                handler(this, args);
            }
        }
        protected virtual void OnTableProcessorEnd(TableProcessorEndEventArgs args)
        {
            EventHandler<TableProcessorEndEventArgs> handler = TableProcessorEnd;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        protected virtual void OnTableProcessorError(TableProcessorExceptionEventArgs args)
        {
            EventHandler<TableProcessorExceptionEventArgs> handler = TableProcessorException;
            if (handler != null)
            {
                handler(this, args);
            }
        }
        #endregion

    }
}

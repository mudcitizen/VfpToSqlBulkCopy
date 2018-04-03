using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public class VfpConnectionStringBuilder
    {
        public String DataSource { get; set; }
        public String ConnectionString
        {
            get
            {
                if (String.IsNullOrEmpty(DataSource))
                    throw new ApplicationException("DataSouce null or empty");
                return String.Format(@"Provider=VFPOLEDB.1;Data Source={0};Collating Sequence=general;DELETED=False;", DataSource);
            }
        }

        public VfpConnectionStringBuilder() { }
        public VfpConnectionStringBuilder(String connectionString)
        {
            SetDataSourceFromConnectionString(connectionString);
        }

        public void SetDataSourceFromConnectionString(String connectionString)
        {
            OleDbConnectionStringBuilder bldr = new OleDbConnectionStringBuilder(connectionString);
            DataSource = bldr.DataSource;

        }
    }
}

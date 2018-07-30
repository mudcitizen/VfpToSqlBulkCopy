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
                /*

                The code in RestartParameter does character-by-character comparison - which
                will give the expected result only if we use machine collation

                 In VFP 
                 - SELECT table from ditable WHERE table like 'SY%' 
                 - INDEX ON table TAG generalCol COLLATE 'general'
                 - LIST the SY_ rows are at the top of the list 
                  Record#  TABLE     
                      10  SY_COMM   
                      11  SY_LOG    
                       2  SYCCTYP   
                      12  SYCFGCHD  
                      13  SYCFGCHH  


                 - INDEX ON table TAG MachineCol COLLATE 'machine'
                 - LIST the SY_ rows are at the bottom of the list 
                   Record#  TABLE     
                    21  SYSHRESC  
                    22  SYSHRHDR  
                    9   SYSTEM    
                    10  SY_COMM   
                    11  SY_LOG    

                 */

                return String.Format(@"Provider={0};Data Source={1};Collating Sequence=machine;DELETED=False;",Constants.ConnectionStringTerms.VfpOleDbProvider, DataSource);
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

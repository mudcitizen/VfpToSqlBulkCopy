using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public class RestartParameter
    {
        private String _ConnectionName;
        private String _TableName;
        public String ConnectionName
        {
            get { return _ConnectionName; }
            set { _ConnectionName = value.Trim().ToUpper(); }
        }
        public String TableName
        {
            get { return _TableName; }
            set {
                _TableName = value.Trim().ToUpper();
            }
        }

        public Boolean SatisfiesFilter(String connectionNameBeingProcessed, String inTableName)
        {
            /*
             * An assumption is that we always process Host before POS - but this 
             * will be called for both Host and POS connection names
             */

            String tblName = inTableName.Trim().ToUpper();

            // processing Host and restarting on POS then we skip all Host files
            if ((connectionNameBeingProcessed.Equals(Constants.ConnectionNames.Host, StringComparison.InvariantCultureIgnoreCase)) && (ConnectionName.Equals(Constants.ConnectionNames.POS, StringComparison.InvariantCultureIgnoreCase)))
                // called about Host ; Restarting on POS
                return false;

            // If processing POS and restarting on Host return true
            if ((connectionNameBeingProcessed.Equals(Constants.ConnectionNames.POS, StringComparison.InvariantCultureIgnoreCase)) && (ConnectionName.Equals(Constants.ConnectionNames.Host, StringComparison.InvariantCultureIgnoreCase)))
                return true;

            if (connectionNameBeingProcessed.Equals(ConnectionName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (tblName.StartsWith(TableName))
                    return true;

                /*
                 * When I 1st wrote this I used String.Compare() and got hozed because I wasn't thinking about
                 * "What collation is being used here?"  And I got results different than what I expected - which
                 * was "How would VFP sort?"  
                 * 
                 * Everything we do in VFP uses MACHINE collation. 
                 *  
                 * Windows doesn't use machine collation - at least by default
                 * when I do dir/b sy*.dbf /on > a.txt I get
                 * 
                 * SY_COMM.DBF
                 * SY_LOG.DBF
                 * SYBIGCMD.DBF
                 * SYCCTYP.DBF
                 * SYCFGCHD.DBF
                 * SYCFGCHH.DBF
                 * SYEUMLOG.DBF
                 * SYEUMREQ.DBF
                 * SYGRDCOL.DBF
                 * SYIMPRTD.DBF

                 * And if I built an index on DITABLE specifying general collation I'd get the same thing
                 * 
                 * Our config.fpw has collate=MACHINE - and with that collation the SY_*.DBF are at the bottom
                 * of the list
                 * 
                 * * Now I know about String.CompareOrdinal() .... 
                 * 
                 */

                return (String.CompareOrdinal(inTableName, TableName) >= 0);

            }

            return false;
        }

        public override string ToString()
        {
            return String.Format("Connection - {0} ; Table - {1}", ConnectionName, TableName);
        }

    }
}

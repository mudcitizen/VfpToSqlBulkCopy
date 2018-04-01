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
        private char[] _RestartTableChar;
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
                _RestartTableChar = _TableName.ToCharArray();
            }
        }

        public Boolean SatisfiesFilter(String connectionNameBeingProcessed, String tableName)
        {
            /*
             * An assumption is that we always process Host before POS - but this 
             * will be called for both Host and POS connection names
             */

            String tblName = tableName.ToUpper();

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
                 *  Totally hozed by String.Compare.  It's now "down" with ASCII stuff
                 *  saying PS_WTF is before / less than PSCHK whereas VFP says 
                 *  PS_WTF > PSCHK.  
                 * 
                 *  StringCompare says '_' comes before 'C' but it you just do a character
                 *  character-by-character compare we get the result we want..
                if (String.Compare(tableName, TableName) >= 0)
                    return true;
                    */

                char[] tblNameChar = tblName.ToCharArray();
                Boolean greaterOrEqual = true;
                int iterCount = tblNameChar.Length > _RestartTableChar.Length ? _RestartTableChar.Length : tblNameChar.Length;
                for(int i = 0; i < iterCount; i++)
                {
                    if (tblNameChar[i] < _RestartTableChar[i])
                    {
                        greaterOrEqual = false;
                        break;
                    }
                }
                return greaterOrEqual;


            }
            return false;
        }

    }
}

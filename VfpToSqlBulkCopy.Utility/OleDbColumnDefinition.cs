using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public class OleDbColumnDefinition
    {
        String _Name;
        public String Name { get { return _Name; } set { _Name = value.ToUpper(); } }
        public OleDbType Type { get; set; }

        public Boolean IsDate { get { return Type.ToString() == Constants.OleDbTypeNames.Date; } }

        public override string ToString()
        {
            return String.Format("Name - {0} ; Type - {1}", Name, Type.ToString());
        }
    }

}

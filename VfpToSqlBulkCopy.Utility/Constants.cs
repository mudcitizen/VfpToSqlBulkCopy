using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public static class Constants
    {
        public static class OleDbTypeNames
        {
            public static String Date => "DBDate";
        }
        public static class OleDbSchemaColumnNames
        {
            public static String Column => "COLUMN_NAME";
            public static String DataType => "DATA_TYPE";
        }

        public static class DILayer
        {
            public static String DeletedColumnName => "SqlDeleted";
            public static String RecnoColumnName => "SqlRecno";

        }
    }
}

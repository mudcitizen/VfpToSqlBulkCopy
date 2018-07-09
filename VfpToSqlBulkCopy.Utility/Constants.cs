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
            public static String Numeric => "Numeric";
        }
        public static class OleDbSchemaColumnNames
        {
            public static String Column => "COLUMN_NAME";
            public static String DataType => "DATA_TYPE";
            public static String NumericPrecision => "NUMERIC_PRECISION";
            public static String NumericScale => "NUMERIC_SCALE";
        }
        public static class SqlTypeNames
        {
            public static String Date => "date";
            public static String DateTime => "datetime";
        }

        /*
         * SqlDateMinValue is BS.  Many of the types in System.Data.SqlTypes
         * have a MinValue and MaxValue fields.  Eg SqlDateTime and SqlMoney
         * 
         * However, System.Data.SqlTypes doesn't have SqlDate.  All searches
         * refer to DateTime.  Must be we are the only app that uses it :o)
         */
        public static String SqlDateMinValue => "1899-12-30";

        public static class DILayer
        {
            public static String DeletedColumnName => "SqlDeleted";
            public static String RecnoColumnName => "SqlRecno";

        }

        public static class ConnectionNames
        {
            public static String Host => "HOST";
            public static String Sql => "SQL";
            public static String POS => "POS";
        }

        public static int DefaultBatchSize => 25000;

    }
}

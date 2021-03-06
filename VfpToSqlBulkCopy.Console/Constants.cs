﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Console
{
    public static class Constants
    {
        public static String TableProcessorEventsFileNameAppSettingsKey => "TableProcessorEventsFileName";
        public static String EFUploadConnectionNameAppSettingsKey => "EFUploadConnectionName";

        public static class AppSettingKeys
        {
            public static String RestartConnectionName => "RestartConnectionName";
            public static String RestartTableName => "RestartTableName";
        }
    }
}

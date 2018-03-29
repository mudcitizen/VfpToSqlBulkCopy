﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public interface ICommandStringProvider 
    {
        String GetCommandString(String sourceConnectionName, String sourceTableName);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Uploader u = new Uploader();
            u.Upload();
            System.Console.WriteLine("Process Complete");
            System.Console.ReadKey();
        }

    }
}

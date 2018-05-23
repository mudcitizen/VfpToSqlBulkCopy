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
            try
            {
                Uploader u = new Uploader();
                u.Upload();
                System.Console.WriteLine("Process Complete");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
            System.Console.ReadKey();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace SharedLibrary.Services
{
    public class LogService
    {
        public void Test()
        {
            Log.Information("Test");
        }
    }
}

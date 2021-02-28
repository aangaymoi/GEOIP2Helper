using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;

namespace GEOIP2HelperTest
{
    class Program
    {
        static void Main(string[] args)
        {
            
            // Lookup
            var kaka = "113.190.248.214".GetGeoIP2();

            // and get in the cached
            kaka = "113.190.248.214".GetGeoIP2();
        }
    }
}

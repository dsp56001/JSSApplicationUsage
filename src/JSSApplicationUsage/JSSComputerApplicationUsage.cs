using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSUsage.Computer_Application_Usage
{
    

        public class Rootobject
        {
            public Computer_Application_Usage[] computer_application_usage { get; set; }
        }

        public class Computer_Application_Usage
        {
            public string date { get; set; }
            public App[] apps { get; set; }
        }

        public class App
        {
            public string name { get; set; }
            public string version { get; set; }
            public int foreground { get; set; }
            public int open { get; set; }
        }

    
}

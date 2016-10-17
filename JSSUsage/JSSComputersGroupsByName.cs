using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSUsage.JSSComputersGroupsByName
{
    

        public class Rootobject
        {
            public Computer_Group computer_group { get; set; }
        }

        public class Computer_Group
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool is_smart { get; set; }
            public Site site { get; set; }
            public Criterion[] criteria { get; set; }
            public Computer[] computers { get; set; }
        }

        public class Site
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Criterion
        {
            public string name { get; set; }
            public int priority { get; set; }
            public string and_or { get; set; }
            public string search_type { get; set; }
            public string value { get; set; }
        }

        public class Computer
        {
            public int id { get; set; }
            public string name { get; set; }
            public string mac_address { get; set; }
            public string alt_mac_address { get; set; }
            public string serial_number { get; set; }
        }

    
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;
using CsvHelper;
using System.Configuration;

namespace JSSUsage
{
    class Program
    {
        static string CONF_jssServer, CONF_groupName, CONF_outFile, CONF_startDate, CONF_endDate;
        static string CONF_jssUsername, CONF_jssPassword;

        static int Main(string[] args)
        {
            DateTime startDate, endDate;
            endDate = System.DateTime.Now;
            startDate = endDate.AddYears(-1);
            
            CONF_jssServer = ConfigurationManager.AppSettings["jssServer"];
            CONF_groupName = ConfigurationManager.AppSettings["groupName"];
            CONF_outFile = ConfigurationManager.AppSettings["outFile"];
            CONF_startDate = startDate.ToShortDateString(); //2016-01-01
            CONF_endDate = endDate.ToShortDateString();
            CONF_jssUsername = ConfigurationManager.AppSettings["jssUsername"];
            CONF_jssPassword = ConfigurationManager.AppSettings["jssPassword"];


            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-help":
                    case "-h":
                        ConsoleHelp();
                        return 0;
                        
                    case "-jssServer":
                        CONF_jssServer = args[Array.IndexOf(args, "-jssServer") + 1]; 
                        break;
                    case "-jssUsername":
                        CONF_jssUsername = args[Array.IndexOf(args, "-jssUsername") + 1];
                        break;
                    case "-jssPassword":
                        CONF_jssPassword = args[Array.IndexOf(args, "-jssPassword") + 1];
                        break;
                    case "-groupName":
                        CONF_groupName = args[Array.IndexOf(args, "-groupName") + 1];
                        break;
                    case "-outFile":
                        CONF_outFile = args[Array.IndexOf(args, "-outFile") + 1];
                        break;
                    case "-startDate":
                        string strStartDate = args[Array.IndexOf(args, "-startDate") + 1];
                        DateTime.TryParse(strStartDate, out startDate);
                        CONF_startDate = startDate.ToShortDateString();
                        
                        break;
                    case "-endDate":
                        string strEndDate = args[Array.IndexOf(args, "-endDate") + 1];
                        DateTime.TryParse(strEndDate, out endDate);
                        
                        
                        CONF_endDate = endDate.ToShortDateString();
                        break;
                }
            }

            Console.WriteLine("JSSUsage -help for help");
            Console.Write(string.Format("jssServer : {0} \tgroupName : {1}", CONF_jssServer, CONF_groupName));
            Console.WriteLine(string.Format("\toutFile : {0}", CONF_outFile));
            Console.WriteLine(string.Format("Start Date : {0} \tEndDate : {1}", GetJSSDate(CONF_startDate), GetJSSDate(CONF_endDate)));
            
            UpdateWriteCVS();

            //Console.ReadKey();
            return 0;
        }

        private static void ConsoleHelp()
        {
            System.Console.WriteLine("JSSUsage outputs a csv file from a JSS server showing computer uages for a JSS groups");
            System.Console.WriteLine("Usage: -jssServer serverName [sma-jss.iam.local:8443]");
            System.Console.WriteLine("Usage: -groupName outpPutFileName [All CMI]");
            System.Console.WriteLine("Usage: -outFile outPutFileName [outfile.csv]");
            System.Console.WriteLine("Usage: -startDate []");
            System.Console.WriteLine("Usage: -endDate []");
        }

        static void UpdateWriteCVS()
        {
            CSVComputerUsageRecord record = new CSVComputerUsageRecord();
            bool fileExists = System.IO.File.Exists(CONF_outFile);
            using (StreamWriter writer = new StreamWriter(CONF_outFile, true))
            {
                var csv = new CsvWriter(writer);
                if (!fileExists)
                {
                    csv.WriteHeader<CSVComputerUsageRecord>();
                }
                JSSUsage.JSSComputersGroupsByName.Rootobject GroupsRequest = GetComputersByGroupName(CONF_groupName);
                if (GroupsRequest == null)
                {

                    Console.WriteLine("group not found");
                    return;
                }
                foreach (JSSUsage.JSSComputersGroupsByName.Computer c in GroupsRequest.computer_group.computers)
                {
                    var computerUsage = GetComputerUsageByName(c.name);
                    foreach (JSSUsage.Computer_Application_Usage.Computer_Application_Usage cau in computerUsage.computer_application_usage)
                    {
                        foreach (JSSUsage.Computer_Application_Usage.App app in cau.apps)
                        {
                            record = new CSVComputerUsageRecord()
                            {
                                appName = app.name,
                                computerId = c.id,
                                computerName = c.name,
                                date = cau.date,
                                foreground = app.foreground,
                                open = app.open,
                                version = app.version
                            };
                            csv.WriteRecord(record);
                        }
                    }
                    Console.WriteLine(c.name);
                }
            }
        }

        class CSVComputerUsageRecord
        {
            public int computerId { get; set; }
            public string computerName { get; set; }
            public string date { get; set; }
            public string appName { get; set; }
            public string version { get; set; }
            public int foreground { get; set; }
            public int open { get; set; }
            public string siteName { get; set; }
        }


        private static JSSUsage.JSSComputersGroupsByName.Rootobject GetComputersByGroupName(string GroupName)
        {
            string URL = string.Format("https://{1}/JSSResource/computergroups/name/{0}",
               HttpUtility.UrlEncode(GroupName), CONF_jssServer);
            string DATA = "";
            string JSON =  WebRequestinJson2(URL, DATA);
           
            JSSUsage.JSSComputersGroupsByName.Rootobject cgbn = JsonConvert.DeserializeObject<JSSComputersGroupsByName.Rootobject>(JSON);
            
            return cgbn;
        }

        static JSSUsage.Computer_Application_Usage.Rootobject GetComputerUsageByName(string ComputerName)
        {
            string URL = string.Format("https://{1}/JSSResource/computerapplicationusage/name/{0}/{2}_{3}",
               ComputerName, CONF_jssServer, GetJSSDate(CONF_startDate), GetJSSDate(CONF_endDate));

            //string URL = string.Format("https://{1}/JSSResource/computerapplicationusage/name/{0}/2016-01-01_2016-12-31",
            //   ComputerName, CONF_jssServer);

            //Console.WriteLine(URL);

            string DATA = "";
            string JSON = WebRequestinJson2(URL, DATA);

            JSSUsage.Computer_Application_Usage.Rootobject cau = JsonConvert.DeserializeObject<JSSUsage.Computer_Application_Usage.Rootobject>(JSON);
            
            return cau;
        }

        /// <summary>
        /// JSS API expects dates with yyy-MM-dd instead of /
        /// </summary>
        /// <param name="strDate"></param>
        /// <returns></returns>
        public static string GetJSSDate(string strDate)
        {
            DateTime dt = DateTime.Parse(strDate);
            return ((DateTime)dt).ToString(@"yyyy-MM-dd");
        }

        // to ignore SSL certificate errors
        public static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        static string WebRequestinJson2(string url, string postData)
        {
            Uri uri = new Uri(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri) as HttpWebRequest;
            request.Accept = "application/json";

            // authentication
            var cache = new CredentialCache();
            cache.Add(uri, "Basic", new NetworkCredential(CONF_jssUsername, CONF_jssPassword));
            request.Credentials = cache;

            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

            //var response = (HttpWebResponse)await Task.Factory
            //.FromAsync<WebResponse>(request.BeginGetResponse,
            //                request.EndGetResponse,
            //                null);
            WebResponse response;
            string ret = String.Empty;
            try
            {
                response = request.GetResponse();
                Stream resStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("Exception accessing {0}", url);
                Console.WriteLine(ex.Message);
            }
            return ret;
        }

        

        
    }
}

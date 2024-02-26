using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.API.Models
{
    public class WebJobs
    {
        public List<WebJob> value { get; set; }
    }

    public class WebJob
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string location { get; set; }
        public Properties properties { get; set; }
    }

    public class Properties
    {
        public string status { get; set; }
        public string detailed_status { get; set; }
        public string log_url { get; set; }
        public string name { get; set; }
        public string run_command { get; set; }
        public string url { get; set; }
        public string extra_info_url { get; set; }
        public string type { get; set; }
        public object error { get; set; }
        public bool using_sdk { get; set; }
        public Settings settings { get; set; }
    }

    public class Settings
    {
    }
}



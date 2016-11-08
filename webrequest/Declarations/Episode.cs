using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace webrequest.Declarations.Episodes
{

    public class Rootobject
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public int season { get; set; }
        public int number { get; set; }
        public string airdate { get; set; }
        public string airtime { get; set; }
        public DateTime airstamp { get; set; }
        public int runtime { get; set; }
        public Image image { get; set; }
        public string summary { get; set; }
        public _Links _links { get; set; }
    }

    public class Image
    {
        public string medium { get; set; }
        public string original { get; set; }
    }

    public class _Links
    {
        public Self self { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

}
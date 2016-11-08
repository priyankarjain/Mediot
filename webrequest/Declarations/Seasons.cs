namespace webrequest.Declarations.Seasons
{

    public class Rootobject
    {
        public int id { get; set; }
        public string url { get; set; }
        public int number { get; set; }
        public string name { get; set; }
        public int episodeOrder { get; set; }
        public string premiereDate { get; set; }
        public string endDate { get; set; }
        public Network network { get; set; }
        public object webChannel { get; set; }
        public Image image { get; set; }
        public string summary { get; set; }
        public _Links _links { get; set; }
    }

    public class Network
    {
        public int id { get; set; }
        public string name { get; set; }
        public Country country { get; set; }
    }

    public class Country
    {
        public string name { get; set; }
        public string code { get; set; }
        public string timezone { get; set; }
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
namespace webrequest.Declarations.People
{
    public class Image
    {
        public string medium { get; set; }
        public string original { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class Links
    {
        public Self self { get; set; }
    }

    public class Person
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public Image image { get; set; }
        public Links _links { get; set; }
    }

    public class Image2
    {
        public string medium { get; set; }
        public string original { get; set; }
    }

    public class Self2
    {
        public string href { get; set; }
    }

    public class Links2
    {
        public Self2 self { get; set; }
    }

    public class Character
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public Image2 image { get; set; }
        public Links2 _links { get; set; }
    }

    public class RootObject
    {
        public Person person { get; set; }
        public Character character { get; set; }
    }
}
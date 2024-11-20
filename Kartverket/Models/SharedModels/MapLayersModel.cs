namespace Kartverket.Models;

public class MapLayersModel
{
    public string type { get; set; }
    public List<Feature> features { get; set; }
}

public class Feature
{
    public string type { get; set; }
    public Dictionary<string, object> properties { get; set; }
    public Geometry geometry { get; set; }
}

public class Geometry
{
    public string type { get; set; }
    public List<object> coordinates { get; set; }
}


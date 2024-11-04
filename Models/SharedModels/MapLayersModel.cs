namespace Kartverket.Models;

public class MapLayersModel
{
    public string type { get; set; }
    public List<Feature> features { get; set; }
}

public class Feature
{
    public string type { get; set; }
    public Properties properties { get; set; }
    public Geometry geometry { get; set; }
}

public class Geometry
{
    public string type { get; set; }
    public List<List<List<double>>> coordinates { get; set; }
}

public class Properties
{
}

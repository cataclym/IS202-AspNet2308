namespace Kartverket.Models;

public class MapLayersModel
{
    public required string type { get; set; }
    public required List<Feature> features { get; set; }
}

public class Feature
{
    public required string type { get; set; }
    public required Dictionary<string, object> properties { get; set; }
    public required Geometry geometry { get; set; }
}

public class Geometry
{
    public required string type { get; set; }
    public required List<object> coordinates { get; set; }
}


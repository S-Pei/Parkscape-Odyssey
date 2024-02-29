public class LocationQuest : LocationQuest {
    private LatLon location;

    public LocationQuest(string label, Texture2D referenceImage, double[] featureVector, LatLon location) : base(label, referenceImage, featureVector)
    {
        this.location = location;
    }
}
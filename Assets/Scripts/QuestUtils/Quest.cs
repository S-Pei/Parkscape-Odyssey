using Microsoft.Geospatial;
using UnityEngine;

public class Quest
{
    // name of object to find
    public string Label { get; private set; }
    public Texture2D ReferenceImage { get; private set; }
    public bool IsCompleted { get; private set; }

    public Quest(string label, Texture2D referenceImage) {
        Label = label;
        ReferenceImage = referenceImage;
        IsCompleted = false;
    }
}

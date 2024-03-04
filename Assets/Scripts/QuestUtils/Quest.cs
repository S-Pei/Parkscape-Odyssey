using Microsoft.Geospatial;
using UnityEngine;
using System;

[Serializable]
public class Quest
{
    // name of object to find
    public string Label {
        get => _label;
        private set => _label = value;
    }

    public Texture2D ReferenceImage {
        get => _referenceImage;
        private set => _referenceImage = value;
    }

    public bool IsCompleted {
        get => _isCompleted;
        private set => _isCompleted = value;
    }

    // Private backing variables for properties so that this class can be properly serialized
    [SerializeField]
    private string _label;
    [SerializeField]
    private Texture2D _referenceImage;
    [SerializeField]
    private bool _isCompleted;

    public Quest(string label, Texture2D referenceImage) {
        Label = label;
        ReferenceImage = referenceImage;
        IsCompleted = false;
    }
}

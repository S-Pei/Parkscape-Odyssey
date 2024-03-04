using System;
using Microsoft.Geospatial;
using UnityEngine;

[Serializable]
public class QuestLocation {
    public double Latitude {
        get => _latitude;
        set => _latitude = value;
    }

    public double Longitude {
        get => _longitude;
        set => _longitude = value;
    }

    [SerializeField]
    private double _latitude;
    
    [SerializeField]
    private double _longitude;

    public QuestLocation(LatLon coordinates) {
        Latitude = coordinates.LatitudeInDegrees;
        Longitude = coordinates.LongitudeInDegrees;
    }
}

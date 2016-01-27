using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class GeoCoordinate
{
	/// <summary>
	/// Valeur entre -180 et 180 représentant est-ouest
	/// </summary>
	public float Longitude { get; set; }
	/// <summary>
	/// Valeur entre 90 et -90 representant nord-sud
	/// </summary>
	public float Latitude { get; set; }
	public float Altitude { get; set; }
	/// <summary>
	/// Precision of the coordinate
	/// </summary>
	public float Accuracy { get; set; }

	public GeoCoordinate()
	{
		Latitude = 0;
		Longitude = 0;
		Altitude = 0;
		Accuracy = 0;
	}

	public GeoCoordinate(Vector3 v, float _accuracy = 0)
	{
		Latitude = v.y;
		Longitude = v.x;
		Altitude = v.z;
		Accuracy = _accuracy;
	}

	public GeoCoordinate(float _latitude, float _longitude, float _altitude, float _accuracy = 0)
	{
		Latitude = _latitude;
		Longitude = _longitude;
		Altitude = _altitude;
		Accuracy = _accuracy;
	}

	public void SetCoordinate(Vector3 v)
	{
		Latitude = v.y;
		Longitude = v.x;
		Altitude = v.z;
	}

	/// <summary>
	/// create a vector from the coordinate
	/// (longitude, latitude, altitude)
	/// </summary>
	/// <returns>the vector</returns>
	public Vector3 ToVector()
	{
		return new Vector3(Longitude, Latitude, Altitude);
	}

	/// <summary>
	/// Get the distance in meter to another GeoCoordinate point
	/// </summary>
	/// <param name="_to">the coordinate</param>
	/// <returns>the distance in meter</returns>
	public double GetMeterDistanceTo(GeoCoordinate _to)
	{
		return HaversineInM(Latitude, Longitude, _to.Latitude, _to.Longitude);
	}

	// Source  : http://stackoverflow.com/questions/365826/calculate-distance-between-2-gps-coordinates
	private const double E_QUATORIAL_EARTH_RADIUS = 6378.1370D;
	private const double D2_R = (Math.PI / 180D);

	private double HaversineInM(double _lat1, double _long1, double _lat2, double _long2)
	{
		return 1000d * HaversineInKm(_lat1, _long1, _lat2, _long2);
	}

	private static double HaversineInKm(double _lat1, double _long1, double _lat2, double _long2)
	{
		double dlong = (_long2 - _long1) * D2_R;
		double dlat = (_lat2 - _lat1) * D2_R;
		double a = Math.Pow(Math.Sin(dlat / 2D), 2D) + Math.Cos(_lat1 * D2_R) * Math.Cos(_lat2 * D2_R) * Math.Pow(Math.Sin(dlong / 2D), 2D);
		double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));
		double d = E_QUATORIAL_EARTH_RADIUS * c;

		return d;
	}
}

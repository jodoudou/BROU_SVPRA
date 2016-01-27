using UnityEngine;
using System.Collections;
using System;

//Source : http://www.codeproject.com/Articles/86480/Real-Time-Position-Display-Software-Model-for-Goog
/// <summary>
/// Use to calculate position and geo coordinate
/// </summary>
public class MercatorProjection
{
	/// <summary>
	/// Unknown
	/// </summary>
	private float circumference;
	/// <summary>
	/// Unknown
	/// </summary>
	private float radius;
	/// <summary>
	/// Position of the center of the map
	/// </summary>
	private Vector2 centre;

	/// <summary>
	/// Initialize the object
	/// </summary>
	/// <param name="_zoomLevel"></param>
	public MercatorProjection(int _zoomLevel)
	{
		// 256 is the size of a tile in google image
		this.circumference = 256 * (float)Math.Pow(2, _zoomLevel);

		this.radius = (this.circumference) / (2 * Mathf.PI);
		// this is the centre of the of the whole projection. 
		this.centre = new Vector2(this.circumference / 2, this.circumference / 2);


	}

	/// <summary>
	/// Get the position in pixel from longitude
	/// </summary>
	/// <param name="_longInDegrees">Longitude</param>
	/// <returns>The position x in pixel</returns>
	public float GetXFromLongitude(float _longInDegrees)
	{
		float x = 0;

		float longInRadians = _longInDegrees * Mathf.Deg2Rad;

		x = this.radius * longInRadians;

		// False Easting....
		x = this.centre.x + x;

		return x;

	}

	/// <summary>
	/// Get the longitude from an x position
	/// </summary>
	/// <param name="_xValue">x</param>
	/// <returns>Longitude</returns>
	public float GetLongitudeFromX(float _xValue)
	{
		float longitude = 0;

		// False Easting

		_xValue = _xValue - this.centre.x;
		//
		longitude = _xValue / this.radius;

		longitude = longitude * Mathf.Deg2Rad;

		return longitude;
	}

	/// <summary>
	/// Get the position in pixel from latitude
	/// </summary>
	/// <param name="_latInDegrees"></param>
	/// <returns>The position y in pixel</returns>
	public float GetYFromLatitude(float _latInDegrees)
	{
		float y;
		float latInRadians = _latInDegrees * Mathf.Deg2Rad;

		// Log for the base of E i.e natural logarthim..
		float logVal = (float)(Math.Log(((1 + Math.Sin(latInRadians)) / (1 - Math.Sin(latInRadians))), Math.E));


		y = this.radius * 0.5f * logVal;

		// False Northing....
		y = y - this.centre.y;


		return y;
	}

	/// <summary>
	/// Get the latitude from an y position
	/// </summary>
	/// <param name="_yValue">y</param>
	/// <returns>Latitude</returns>
	public float GetLatitudeFromY(float _yValue)
	{
		float latitude = 0;

		_yValue = _yValue - this.centre.y;

		float invLog = _yValue / (this.radius * 0.5f);

		invLog = (float)Math.Pow(Math.E, invLog);

		latitude = (float)Math.Asin((invLog - 1) / (invLog + 1));


		latitude = latitude * Mathf.Deg2Rad;

		return latitude;

	}
}


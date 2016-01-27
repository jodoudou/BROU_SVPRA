using UnityEngine;
using System.Collections;

/// <summary>
/// Represent a position in the virtual world from a geo coordinate
/// </summary>
public class ARLocation
{
	/// <summary>
	/// 0,0,0 of the virtual world is this geo coord in the real world.
	/// </summary>
	private static readonly GeoCoordinate WORLD_CENTER = new GeoCoordinate(46.215315f, 5.241165f, 0);

	/// <summary>
	/// Geo pos of the object
	/// </summary>
	public GeoCoordinate latLng { get; private set; }
	/// <summary>
	/// Its position in the virtual world.
	/// </summary>
	public Vector3 position { get; private set; }

	/// <summary>
	/// Build a new AR Location
	/// </summary>
	/// <param name="_latLng">Its geo pos</param>
	public ARLocation(GeoCoordinate _latLng)
	{
		UpdateArLocation(_latLng);
	}

	public void UpdateArLocation(GeoCoordinate _latLng)
	{
		latLng = _latLng;
		Vector2 temp = AsVertex();
		position = new Vector3(temp.x, double.IsNaN(_latLng.Altitude) ? 0 : (float)_latLng.Altitude, temp.y);
	}

	/// <summary>
	/// Return the virtual position of the object
	/// </summary>
	/// <returns></returns>
	private Vector2 AsVertex()
	{
		return AsVertexRelativeTo(WORLD_CENTER);
	}

	/// <summary>
	/// Calculate the position of the object relative the a center
	/// </summary>
	/// <param name="_center"></param>
	/// <returns></returns>
	private Vector2 AsVertexRelativeTo(GeoCoordinate _center)
	{
		/*
		 *   center   lonDiffLoc
		 * 		+-------+
		 * 		|		|
		 * 		+-------+
		 * latDiffLoc  mLoc
		 */

		GeoCoordinate latDiffLoc = new GeoCoordinate(latLng.Latitude, _center.Longitude, 0);

		GeoCoordinate lonDiffLoc = new GeoCoordinate(_center.Latitude, latLng.Longitude, 0);

		double latDiff = _center.GetMeterDistanceTo(latDiffLoc);
		double lonDiff = _center.GetMeterDistanceTo(lonDiffLoc);

		//Les distances étant des valeurs absolues, il faut parfois appliquer un correctif
		if (latDiffLoc.Latitude < _center.Latitude)
			latDiff *= -1;

		//Ceci risque de ne pas fonctionner si les deux points sont autour du méridien de Greenwhich
		if (lonDiffLoc.Longitude < _center.Longitude)
			lonDiff *= -1;

		//Longitude est X car elle est est-ouest, tandis que latitude est Z car elle est nord-sud
		//La différence de latitude est mise au négatif car une valeur plus proche de l'équateur est plus petite
		return new Vector2((float)lonDiff, (float)(latDiff));
	}
}

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.Xml.Linq;
using System.Linq;

/// <summary>
/// See https://developers.google.com/maps/documentation/utilities/polylinealgorithm
/// Source: https://gist.github.com/shinyzhu/4617989
/// </summary>
public static class Polyline
{
	public static IEnumerable<GeoCoordinate> Decode(string encodedPoints)
	{
		if (string.IsNullOrEmpty(encodedPoints))
			throw new ArgumentNullException("encodedPoints");

		char[] polylineChars = encodedPoints.ToCharArray();
		int index = 0;

		int currentLat = 0;
		int currentLng = 0;
		int next5bits;
		int sum;
		int shifter;

		while (index < polylineChars.Length)
		{
			// calculate next latitude
			sum = 0;
			shifter = 0;
			do
			{
				next5bits = (int)polylineChars[index++] - 63;
				sum |= (next5bits & 31) << shifter;
				shifter += 5;
			} while (next5bits >= 32 && index < polylineChars.Length);

			if (index >= polylineChars.Length)
				break;

			currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

			//calculate next longitude
			sum = 0;
			shifter = 0;
			do
			{
				next5bits = (int)polylineChars[index++] - 63;
				sum |= (next5bits & 31) << shifter;
				shifter += 5;
			} while (next5bits >= 32 && index < polylineChars.Length);

			if (index >= polylineChars.Length && next5bits >= 32)
				break;

			currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

			yield return new GeoCoordinate((float)((currentLat) / 1E5), (float)((currentLng) / 1E5), 0);
		}
	}

	/// <summary>
	/// Encode it
	/// </summary>
	/// <param name="points"></param>
	/// <returns></returns>
	public static string Encode(IEnumerable<Vector2> points)
	{
		var str = new StringBuilder();

		var encodeDiff = (Action<int>)(diff =>
		{
			int shifted = diff << 1;
			if (diff < 0)
				shifted = ~shifted;

			int rem = shifted;

			while (rem >= 0x20)
			{
				str.Append((char)((0x20 | (rem & 0x1f)) + 63));

				rem >>= 5;
			}

			str.Append((char)(rem + 63));
		});

		int lastLat = 0;
		int lastLng = 0;

		foreach (Vector2 point in points)
		{
			int lat = (int)Math.Round(point.x * 1E5);
			int lng = (int)Math.Round(point.y * 1E5);

			encodeDiff(lat - lastLat);
			encodeDiff(lng - lastLng);

			lastLat = lat;
			lastLng = lng;
		}

		return str.ToString();
	}

	public static string GetPolylineFromXML(string xml)
	{
		XElement element = XElement.Parse(xml);
		string status = element.Elements().First().Value;
		if (status == "OK")
		{
			return element.Elements().Last().Element("overview_polyline").Elements().First().Value;
		}

		return "";
	}
}


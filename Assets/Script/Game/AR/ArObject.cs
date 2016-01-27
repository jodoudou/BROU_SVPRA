using UnityEngine;
using System.Collections;
using System;
/// <summary>
/// an game object for the augmented reality
/// </summary>
[Serializable]
public class ArObject: MonoBehaviour
{
	public string id { get; private set; }
	/// <summary>
	/// ARLocation of the object
	/// </summary>
	private ARLocation location;
	/// <summary>
	/// Scale of the object
	/// </summary>
	private float scale;
	/// <summary>
	/// Opacity of the object
	/// </summary>
	private float opacity;
	/// <summary>
	/// Heading of the object.
	/// </summary>
	private float orientation;
	/// <summary>
	/// Get the geo pos of the object
	/// </summary>
	public GeoCoordinate LatLng { get { return location.latLng; } }
	/// <summary>
	/// Get the virtual pos of the object
	/// </summary>
	public Vector3 Position { get { return location.position; } }
	/// <summary>
	/// Get the 3D model of the object
	/// </summary>

	public void ShowObject(bool show)
	{
		gameObject.SetActive(show);
	}

	public void Init(Element element)
	{
		id = element.id;
		name = element.title;
		if (element.ratio != null)
		{
			scale = float.Parse(element.ratio);
		}
		if (element.transparency != null)
		{
			opacity = float.Parse(element.transparency);
		}
		if (element.orientation != null)
		{
			orientation = float.Parse(element.orientation);
		}
		if (element.latitude != null || element.longitude != null || element.elevation != null)
		{
			GeoCoordinate temp = new GeoCoordinate(float.Parse(element.latitude), float.Parse(element.longitude), float.Parse(element.elevation));
			location = new ARLocation(temp);
		}
	}

	private void Start()
	{
		transform.localScale *= scale;
		transform.position = location.position;
		transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, orientation, transform.rotation.eulerAngles.z));
		if (GetComponent<MtlImporter>() != null)
		{
			GetComponent<MtlImporter>().AddTexture();
		}
	}
}

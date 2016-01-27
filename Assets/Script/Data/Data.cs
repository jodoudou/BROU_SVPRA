using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Data
{
	public static string attractionPath;
	public static string imagePath;
	public static string objPath;
	public static string textPath;
	public static string mtlPath;
	public static string texturePath;
	public static string routePath;
	/// <summary>
	/// all the attraction close to our position
	/// </summary>
	public Dictionary<string, Attraction> attractions { get; set; }
	/// <summary>
	/// the attracion chosen by the user
	/// </summary>
	public Attraction attraction { get; set; }
	/// <summary>
	/// the route chosen by the user
	/// </summary>
	public Route route { get; set; }
	public Element eleRoute { get; set; }
	/// <summary>
	/// all the objects downloaded for the augmented reality
	/// key is element id
	/// </summary>
	public Dictionary<string, ArObject> arObjects { get; set; }
	/// <summary>
	/// all the .mtl file downloaded for the arObject
	/// key is element filename
	/// </summary>
	public Dictionary<string, string> mtl { get; set; }
	/// <summary>
	/// all the texture downloaded for the arObject
	/// key is element fileName
	/// </summary>
	public Dictionary<string, Texture2D> textures { get; set; }

	public List<string> arObjToEnable { get; set; }

	static Data()
	{
		Data.attractionPath = Application.persistentDataPath + "/Attraction/";
		string ele = "/element";
		string ar = "/ar_object";
		Data.imagePath = ele + ar + "/image/";
		Data.objPath = ele + ar + "/obj/";
		Data.textPath = ele + ar + "/text/";
		Data.mtlPath = ele  + "/mtl/";
		Data.texturePath = ele  + "/texture/";
		Data.routePath = "/route/";
	}

	public Data()
	{
		arObjToEnable = new List<string>();
		arObjects = new Dictionary<string, ArObject>();
		mtl = new Dictionary<string, string>();
		textures = new Dictionary<string, Texture2D>();
	}

	public void ResetData()
	{
		attractions = null;
		attraction = null;
		route = null;
		DeleteArElement();
	}

	public void DeleteArElement()
	{
		foreach (ArObject o in arObjects.Values)
		{
			if (o != null && !o.Equals(null))
			{
				GameObject.DestroyObject(o.gameObject);
			}
		}
		arObjects.Clear();
		mtl.Clear();
		textures.Clear();
		System.GC.Collect();
	}
}

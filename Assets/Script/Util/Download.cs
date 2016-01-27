using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CielaSpike;

/// <summary>
/// Download object from the internet and process them for use in the application
/// </summary>
public class Download: SubjectMono
{
	/// <summary>
	/// A queue of download that need to be done
	/// </summary>
	public Queue<ToDownload> toDownload { get; set; }
	/// <summary>
	/// A second queue for more urgent download
	/// </summary>
	public Queue<ToDownload> priorityToDownload { get; set; }
	/// <summary>
	/// is the class doing something that should make him wait before starting the nest download
	/// </summary>
	public bool isWorking { get; set; }
	/// <summary>
	/// the last thing that has been downloaded
	/// </summary>
	public WWW lastDownload { get; set; }
	/// <summary>
	/// the current thing being downloaded
	/// </summary>
	public ToDownload theDownload { get; set; }

	private ToolBox toolBox;
	private Data data;
	/// <summary>
	/// To import obj file in the game
	/// </summary>
	private ObjImporter objImporter = new ObjImporter();
	/// <summary>
	/// All the GameObject created will have this GameObject as parent
	/// </summary>
	private GameObject parentGameObject;

	private void Awake()
	{
		toDownload = new Queue<ToDownload>();
		priorityToDownload = new Queue<ToDownload>();
		isWorking = false;
	}

	private void Start()
	{
		toolBox = ToolBox.Instance;
		data = toolBox.data;
	}

	private void OnLevelWasLoaded(int level)
	{
		parentGameObject = new GameObject("Resources");
	}

	/// <summary>
	/// Execute the next download when a download is finish
	/// </summary>
	private void Update()
	{
		if (!isWorking && toolBox.testDevice.internet)
		{
			if (priorityToDownload.Count > 0)
			{
				theDownload = priorityToDownload.Peek();
				priorityToDownload.Dequeue();
				StartCoroutine(StartDownload(theDownload.url));
			}
			else if (toDownload.Count > 0)
			{
				theDownload = toDownload.Peek();
				toDownload.Dequeue();
				StartCoroutine(StartDownload(theDownload.url));
			}
		}
	}

	/// <summary>
	/// Download something if its not already in the application
	/// if the download failed, the object is requeue again and we test the internet connection
	/// </summary>
	/// <param name="url">url of the download</param>
	/// <returns>wait for the download to finish</returns>
	private IEnumerator StartDownload(string url)
	{
		if (!IsAlreadyExist())
		{
			isWorking = true;
#if LOG
			Debug.Log("Downloading file: " + theDownload.url);
			Timer.StartTimer();
#endif
			WWW www = new WWW(theDownload.url);

			yield return www;
#if LOG
			Timer.StopTimer("DownloadTime: ");
#endif

			if (string.IsNullOrEmpty(www.error))
			{
				lastDownload = www;
				ProcessDownload();
			}
			else
			{
#if LOG
				Debug.Log(www.error);
#endif
				priorityToDownload.Enqueue(theDownload);
				toolBox.testDevice.TestInternetConnexion();
				isWorking = false;
			}
		}
#if LOG
		else
		{
			Debug.Log("File already exist: " + theDownload.url);
		}
#endif
	}

	/// <summary>
	/// check if the download already exist in the application
	/// </summary>
	/// <returns>true if it exist</returns>
	private bool IsAlreadyExist()
	{
		if (theDownload.type == DownloadType.Element)
		{
			Element ele = (Element)theDownload.other;
			if (ele.type == "Matériel")
			{
				return data.mtl.ContainsKey(ele.fileName) || data.textures.ContainsKey(ele.fileName);
			}
			else
			{
				return data.arObjects.ContainsKey(ele.id);
			}
		}

		return false;
	}

	/// <summary>
	/// Decide what to do to process a successful download
	/// </summary>
	private void ProcessDownload()
	{
		switch (theDownload.type)
		{
		case DownloadType.Element:
			if (!data.arObjects.ContainsKey(((Element)theDownload.other).id))
			{
				ProcessElement();
			}
			break;
		case DownloadType.Attractions:
			ProcessAttractions();
			break;
		case DownloadType.Route:
			ProcessRoute();
			break;
		default:
			isWorking = false;
			break;
		}

		if (theDownload.type != DownloadType.Element)
		{
			NotifyAllObservers();
#if LOG
			Debug.Log("Finish download: " + theDownload.url);
#endif
		}
	}

	/// <summary>
	/// Decide what to do to process an Element download
	/// </summary>
	private void ProcessElement()
	{
		Element ele = (Element)theDownload.other;
		switch (ele.type)
		{
		case "Schéma 3D":
			StartCoroutine(ProcessMesh());
			break;
		case "Texte":
			ProcessText();
			break;
		case "Matériel":
			ProcessMaterial();
			break;
		case "Image":
			ProcessImage();
			break;
		}
	}

	/// <summary>
	/// Process the download of an obj file
	/// Need to be call multiple time until the processing of the mesh is finish
	/// </summary>
	private IEnumerator ProcessMesh()
	{
		yield return this.StartCoroutineAsync(objImporter.StartImportation(lastDownload.text));
		GameObject go = objImporter.FinishMesh();
		if (go == null)
		{
			FinalizeObject(Instantiate<GameObject>(toolBox.defaultMesh));
		}
		else
		{
			FinalizeObject(go);
		}

	}

	/// <summary>
	/// Process a text download
	/// Create a TextMesh
	/// </summary>
	private void ProcessText()
	{
		GameObject go = new GameObject();
		TextMesh mesh = go.AddComponent<TextMesh>();
		mesh.text = lastDownload.text;
		mesh.font = toolBox.defaultFont;
		mesh.anchor = TextAnchor.LowerCenter;
		mesh.color = Color.black;
		go.GetComponent<Renderer>().material = toolBox.defaultMaterialFont;
		//go.AddComponent<LookAt>().lookAt = GameToolBox.Instance.arCamera.transform;

		FinalizeObject(go);
	}

	/// <summary>
	/// Process a material download, this can be a mtl file or a texture
	/// save to Data.cs
	/// </summary>
	private void ProcessMaterial()
	{
		Element ele = (Element)theDownload.other;

		if (ele.fileName.Contains(".mtl"))
		{
			data.mtl.Add(ele.fileName, lastDownload.text);
			List<string> textures = MtlImporter.GetTexturesFileName(lastDownload.text);
			foreach (string texture in textures)
			{
				Element eleTexture = toolBox.data.attraction.ContainFileName(texture);
				if (eleTexture != null)
				{
					EnqueueElement(eleTexture, DownloadType.Element);
				}
			}
		}
		else
		{
			data.textures.Add(ele.fileName, lastDownload.texture);
		}
#if LOG
		Debug.Log("Finish download: " + theDownload.url);
#endif
		isWorking = false;
	}

	/// <summary>
	/// Process an image download
	/// Put the image on a Quad
	/// </summary>
	private void ProcessImage()
	{
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
		Destroy(go.GetComponent<MeshCollider>());
		Texture2D texture = lastDownload.texture;
		go.GetComponent<MeshRenderer>().material.mainTexture = texture;
		go.transform.localScale = new Vector3(texture.width, texture.height, 1);
		FinalizeObject(go);
	}

	/// <summary>
	/// Process the download of Json for attractions
	/// </summary>
	private void ProcessAttractions()
	{
		try
		{
			data.attractions = Attraction.Parse(lastDownload.text);
		}
		catch (Exception e)
		{
#if LOG
			Debug.Log(e.ToString());
#endif
			toolBox.notification.ShowError(Notification.ERROR_ATTRACTION);
			data.attractions = null;
		}

		isWorking = false;
	}

	/// <summary>
	/// Process the download of XML for a route
	/// </summary>
	private void ProcessRoute()
	{
		try
		{
			data.route = Route.Parse(lastDownload.text);
		}
		catch (Exception e)
		{
#if LOG
			Debug.Log(e.ToString());
#endif
			toolBox.notification.ShowError(Notification.ERROR_ROUTE);
			data.route = null;
		}

		isWorking = false;
	}

	/// <summary>
	/// Finalize the processing of the download of an Element that is an Arobject
	/// </summary>
	/// <param name="go">The GameObject created from the download</param>
	private void FinalizeObject(GameObject go)
	{
		Element ele = (Element)theDownload.other;
		go.AddComponent<ArObject>().Init(ele);
		go.name = ele.title;
		go.transform.SetParent(parentGameObject.transform);
		go.SetActive(false);
		data.arObjects.Add(ele.id, go.GetComponent<ArObject>());
		isWorking = false;
#if LOG
		Debug.Log("Finish download: " + theDownload.url);
#endif
	}

	/// <summary>
	/// Enqueue for a priority download of all the attraction around coordinate
	/// </summary>
	/// <param name="coor">the coordinate</param>
	public void EnqueueAttraction(GeoCoordinate coor)
	{
		string url = "http://svpra.lp-metinet.com/element/export/lat/" + coor.Latitude + "/lon/" + coor.Longitude;
		priorityToDownload.Enqueue(new ToDownload(url, DownloadType.Attractions, null));
#if LOG
		Debug.Log("Enqueue: " + url);
#endif
	}

	/// <summary>
	/// Enqueue for download an element 
	/// </summary>
	/// <param name="ele">Element to enqueue</param>
	/// <param name="type">type of element</param>
	public void EnqueueElement(Element ele, DownloadType type)
	{
		string url = "http://svpra.lp-metinet.com/files/" + ele.fileName + "." + ele.id;
		url = url.Replace(" ", "%20");

		if (type == DownloadType.Element)
		{
			toDownload.Enqueue(new ToDownload(url, DownloadType.Element, ele));
		}
		else if (type == DownloadType.Route)
		{
			priorityToDownload.Enqueue(new ToDownload(url, DownloadType.Route, null));
		}
#if LOG
		Debug.Log("Enqueue: " + url);
#endif
	}

	/// <summary>
	/// Enqueue for a priority download a Google static map.
	/// </summary>
	/// <param name="coor">cooedinate of the center of the map</param>
	/// <param name="zoom">zoom level of the map</param>
	/// <param name="scale">scale of the map</param>
	/// <param name="width">pixel width of the image of the map</param>
	/// <param name="height">pixel height of the image of the map</param>
	public void EnqueueStaticMap(GeoCoordinate coor, int zoom, int scale, float width, float height)
	{
		string url = "https://maps.googleapis.com/maps/api/staticmap?center=" + coor.Latitude.ToString() + "," +
			coor.Longitude.ToString() + "&zoom=" + zoom + "&scale=" + scale + "&size=" + width + "x" + height;

		priorityToDownload.Enqueue(new ToDownload(url, DownloadType.Map, null));
#if LOG
		Debug.Log("Enqueue: " + url);
#endif
	}

	/// <summary>
	/// Enqueue for a priority download the direction from coordinate to coordinate
	/// </summary>
	/// <param name="from">from coordinate</param>
	/// <param name="to">coordinate of where to go</param>
	public void EnqueueDirection(GeoCoordinate from, GeoCoordinate to)
	{
		string url = "https://maps.googleapis.com/maps/api/directions/xml?origin=" + from.Latitude.ToString() + "," + from.Longitude.ToString() + "&" +
		"destination=" + to.Latitude.ToString() + "," + to.Longitude.ToString() + "&mode=walking";

		priorityToDownload.Enqueue(new ToDownload(url, DownloadType.Direction, null));
#if LOG
		Debug.Log("Enqueue: " + url);
#endif
	}
}

/// <summary>
/// structure that represent a download
/// </summary>
public struct ToDownload
{
	public DownloadType type;
	public string url;
	public System.Object other;

	public ToDownload(string _url, DownloadType _type, System.Object _other)
	{
		type = _type;
		url = _url;
		other = _other;
	}
}

/// <summary>
/// the download type
/// </summary>
public enum DownloadType
{
	Map,
	Direction,
	Attractions,
	Route,
	Element
}
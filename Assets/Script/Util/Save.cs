using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

public class Save: MonoBehaviour
{
	private Data data;
	private Download download;

	private void Start()
	{
		data = ToolBox.Instance.data;
		download = ToolBox.Instance.download;
	}

	/// <summary>
	/// Save the current route and all his element to the device disk
	/// <see cref="Data.cs"/>
	/// </summary>
	public void SaveRoute()
	{
		DownLoadElementToSave();
		BinaryFormatter bf = new BinaryFormatter();
		string rootFilePath = Data.attractionPath + data.attraction.Id;

		SaveToDisk(data.attraction, rootFilePath + "/" + data.attraction.Id + ".dat", bf);
		SaveToDisk(data.route, rootFilePath + Data.routePath + data.eleRoute.id + ".dat", bf);

		StartCoroutine(SaveElement(bf, rootFilePath));
	}

	/// <summary>
	/// Save all the downloaded element of a route to the device disk
	/// </summary>
	/// <param name="bf">to format the element</param>
	/// <param name="rootPath">root folder path to save the element</param>
	/// <returns>to wait for the end of the download</returns>
	private IEnumerator SaveElement(BinaryFormatter bf, string rootPath)
	{
		while (download.toDownload.Count > 0 || download.isWorking)
		{
			yield return new WaitForSeconds(0.1f);
		}

		foreach (ArObject a in data.arObjects.Values)
		{
			if (a.GetComponent<TextMesh>() != null)
			{
				SaveText s = new SaveText(a.GetComponent<TextMesh>().text);
				SaveToDisk(s, rootPath + Data.textPath + a.id + ".dat", bf);
			}
			else if (a.GetComponent<MtlImporter>() != null)
			{
				SaveObject3D s = new SaveObject3D(a.GetComponent<MtlImporter>());
				SaveToDisk(s, rootPath + Data.objPath + a.id + ".dat", bf);
			}
			else if (a.GetComponent<MeshRenderer>() != null)
			{
				SaveImage s = new SaveImage(a.GetComponent<MeshRenderer>().material.mainTexture, a.transform);
				SaveToDisk(s, rootPath + Data.imagePath + a.id + ".dat", bf);
			}
		}

		foreach (KeyValuePair<string, string> p in data.mtl)
		{
			SaveToDisk(p.Value, rootPath + Data.mtlPath + p.Key + ".dat", bf);
		}

		foreach (KeyValuePair<string, Texture2D> p in data.textures)
		{
			SaveToDisk(p.Value.EncodeToPNG(), rootPath + Data.texturePath + p.Key + ".dat", bf);
		}
		data.DeleteArElement();

		ToolBox.Instance.loadingScreen.enable(false);
	}

	/// <summary>
	/// Queue all the element of a route to get downloaded.
	/// </summary>
	private void DownLoadElementToSave()
	{
		foreach (Trigger t in data.route.Triggers.Values)
		{
			foreach (string s in t.postTriggerObjectIds)
			{
				if (data.attraction.elements.ContainsKey(s))
				{
					download.EnqueueElement(data.attraction.elements[s], DownloadType.Element);
				}
			}
			foreach (string s in t.preTriggerObjectIds)
			{
				if (data.attraction.elements.ContainsKey(s))
				{
					download.EnqueueElement(data.attraction.elements[s], DownloadType.Element);
				}
			}

			if (t is FindKeyTrigger)
			{
				{
					download.EnqueueElement(data.attraction.elements[((FindKeyTrigger)t).keyObject], DownloadType.Element);
				}
			}
		}

		IEnumerable<Element> mat = from element in data.attraction.elements where element.Value.type == "Matériel" select element.Value;
		foreach (Element ele in mat)
		{
			download.EnqueueElement(ele, DownloadType.Element);
		}
	}

	/// <summary>
	/// Save an object to the device disk
	/// </summary>
	/// <param name="o">the object to save</param>
	/// <param name="path">the path to save the object</param>
	/// <param name="bf">to serialize the object</param>
	private void SaveToDisk(System.Object o, string path, BinaryFormatter bf)
	{
		if (!File.Exists(path))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			using (FileStream file = File.Create(path))
			{
				bf.Serialize(file, o);
				file.Close();
#if LOG
				Debug.Log("Save to disk: " + path);
#endif
			}
		}
#if LOG
		else
		{
			Debug.Log("Already save on disk: " + path);
		}
#endif

	}

	public void DeleteAllSave()
	{
		if (Directory.Exists(Data.attractionPath))
		{
			Directory.Delete(Data.attractionPath, true);
			ToolBox.Instance.notification.ShowNotif(Notification.SAVE_DELETED);
		}
	}
}


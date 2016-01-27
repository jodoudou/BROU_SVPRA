using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// Load object from the device disk
/// </summary>
public class Load: MonoBehaviour
{
	private Data data;
	private BinaryFormatter bf = new BinaryFormatter();
	public Queue<Element> toLoadFromDisk { get; private set; }
	private GameObject parentGameObject;
	public bool isWorking = false;

	/// <summary>
	/// Check if the route of an Attraction is saved on the disk
	/// </summary>
	/// <param name="idRoute">id of the route</param>
	/// <param name="idAttraction">id of the attraction</param>
	/// <returns>true if the route is saved</returns>
	public static bool IsRouteSaved(string idRoute, string idAttraction)
	{
		return File.Exists(Data.attractionPath + idAttraction + Data.routePath + idRoute + ".dat");
	}

	private void Start()
	{
		toLoadFromDisk = new Queue<Element>();
		data = ToolBox.Instance.data;
	}

	private void OnLevelWasLoaded(int level)
	{
		parentGameObject = new GameObject("ResourcesLocal");
	}

	/// <summary>
	/// Every frame a new object is loaded from the queue
	/// </summary>
	private void Update()
	{
		if (toLoadFromDisk.Count > 0)
		{
			isWorking = true;
			LoadElement(toLoadFromDisk.Peek());
			toLoadFromDisk.Dequeue();
			isWorking = false;
		}
	}

	/// <summary>
	/// Load all the saved attraction from the disk
	/// <see cref=">Data.cs"/>
	/// </summary>
	public void LoadAttractions()
	{
		if (Directory.Exists(Data.attractionPath))
		{
			data.attractions = new Dictionary<string, Attraction>();
			String[] directories = Directory.GetDirectories(Data.attractionPath);
			foreach (string path in directories)
			{
				string fileName = path.Substring(path.LastIndexOf("/") + 1) + ".dat";
				Attraction a = LoadFromDisk<Attraction>(path + "/" + fileName);
				data.attractions.Add(a.Id, a);
			}
		}
	}

	/// <summary>
	/// Load the selected route from the disk
	/// <see cref="Data.cs"/>
	/// </summary>
	public void LoadRoute()
	{
		string rootFilePath = Data.attractionPath + data.attraction.Id + Data.routePath + data.eleRoute.id + ".dat";
		data.route = LoadFromDisk<Route>(rootFilePath);
	}

	/// <summary>
	/// load an object from the disk.
	/// </summary>
	/// <typeparam name="T">the type of the object loaded</typeparam>
	/// <param name="path">the path to the object</param>
	/// <returns>the object</returns>
	private T LoadFromDisk<T>(string path) where T: class
	{
#if LOG
		Debug.Log("Load from disk: " + path);
#endif
		if (File.Exists(path))
		{
			using (FileStream file = new FileStream(path, FileMode.Open))
			{
				T content = (T)bf.Deserialize(file);
				file.Close();
				return content;
			}
		}
		return null;
	}

	/// <summary>
	/// Check if an Element already exis t in the application
	/// </summary>
	/// <param name="ele"></param>
	/// <returns></returns>
	private bool IsAlreadyExist(Element ele)
	{
		if (ele.type == "Matériel")
		{
			return data.mtl.ContainsKey(ele.fileName) || data.textures.ContainsKey(ele.fileName);
		}
		else
		{
			return data.arObjects.ContainsKey(ele.id);
		}
	}

	/// <summary>
	/// Load an Element, restore it, and store it in the application
	/// </summary>
	/// <param name="ele">the element to load</param>
	private void LoadElement(Element ele)
	{
		if (!IsAlreadyExist(ele))
		{
			string rootPath = Data.attractionPath + data.attraction.Id;
			SaveAr saveAr;
			switch (ele.type)
			{
			case "Schéma 3D":
				saveAr = LoadFromDisk<SaveAr>(rootPath + Data.objPath + ele.id + ".dat");
				ProcessArSave(saveAr, ele);
				break;
			case "Image":
				saveAr = LoadFromDisk<SaveAr>(rootPath + Data.imagePath + ele.id + ".dat");
				ProcessArSave(saveAr, ele);
				break;
			case "Texte":
				saveAr = LoadFromDisk<SaveAr>(rootPath + Data.textPath + ele.id + ".dat");
				ProcessArSave(saveAr, ele);
				break;
			case "Matériel":
				if (ele.fileName.Contains(".mtl"))
				{
					string s = LoadFromDisk<string>(rootPath + Data.mtlPath + ele.fileName + ".dat");
					if (s != null)
					{
						data.mtl.Add(ele.fileName, s);
					}
				}
				else
				{
					byte[] b = LoadFromDisk<byte[]>(rootPath + Data.texturePath + ele.fileName + ".dat");
					if (b != null)
					{
						Texture2D t = new Texture2D(2, 2);
						t.LoadImage(b);
						data.textures.Add(ele.fileName, t);
					}
				}
				break;
			}
		}
	}

	/// <summary>
	/// Restore an Element and save it in the application
	/// </summary>
	/// <param name="saveAr">the object loaded from the disk</param>
	/// <param name="ele">the element</param>
	private void ProcessArSave(SaveAr saveAr, Element ele)
	{
		if (saveAr != null)
		{
			GameObject go = saveAr.Restore();
			go.SetActive(false);
			go.name = ele.title;
			go.AddComponent<ArObject>().Init(ele);
			go.transform.SetParent(parentGameObject.transform);
			data.arObjects.Add(ele.id, go.GetComponent<ArObject>());
		}
	}
}

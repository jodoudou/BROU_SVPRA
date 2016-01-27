using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manage the transition between navigation and augmented reality
/// </summary>
public class GameManager: MonoBehaviour
{
	[SerializeField]
	private GameObject navigation = null;
	[SerializeField]
	private GameObject augmentedReality = null;

	private ToolBox toolBox;

	private void Awake()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		toolBox = ToolBox.Instance;
	}

	/// <summary>
	/// start the download of all the material for the route
	/// </summary>
	private void Start()
	{
		toolBox.loadingScreen.enable(false);
		//IEnumerable<Element> mat = from element in toolBox.data.attraction.elements where element.Value.type == "Matériel" select element.Value;
		//foreach (Element ele in mat)
		//{
		//	if (toolBox.settings.savedRoute)
		//	{
		//		toolBox.load.toLoadFromDisk.Enqueue(ele);
		//	}
		//	else
		//	{
		//		toolBox.download.EnqueueElement(ele, DownloadType.Element);
		//	}
		//}
	}

	public void Switch(Text button)
	{
		if (augmentedReality.activeInHierarchy)
		{
			EnableAR(false);
			HideArObject();
			button.text = "AR";
		}
		else
		{
			if (toolBox.data.arObjToEnable != null && toolBox.data.arObjToEnable.Count > 0)
			{
				StartAR();
			}
			else
			{
				EnableAR(true);
			}
			button.text = "Nav";
		}
	}

	public void ShowArObject()
	{
		if (augmentedReality.activeInHierarchy)
		{
			foreach (KeyValuePair<string, ArObject> pair in toolBox.data.arObjects)
			{
				pair.Value.ShowObject(toolBox.data.arObjToEnable.Contains(pair.Key));
			}
		}
	}

	private void HideArObject()
	{
		foreach (KeyValuePair<string, ArObject> pair in toolBox.data.arObjects)
		{
			pair.Value.ShowObject(false);
		}
	}

	/// <summary>
	/// start the augmented reality and enable the object to see
	/// will wait for the all the object to be finish to download
	/// <see cref="Download.cs"/>
	/// <see cref="Load.cs"/>
	/// </summary>
	/// <param name="objects">all the object to see</param>
	private void StartAR()
	{
		if ((!toolBox.settings.savedRoute && toolBox.download.toDownload.Count <= 0 && !toolBox.download.isWorking)
			|| (toolBox.settings.savedRoute && toolBox.load.toLoadFromDisk.Count <= 0 && !toolBox.load.isWorking))
		{
			EnableAR(true);
			ShowArObject();
		}
		else
		{
			toolBox.loadingScreen.enable(true);
			StartCoroutine(WaitForDownload());
		}
	}

	/// <summary>
	/// active the ar
	/// </summary>
	/// <param name="enable">active or not</param>
	private void EnableAR(bool enable)
	{
		navigation.SetActive(!enable);
		augmentedReality.SetActive(enable);
		toolBox.sensorManager.orientation.EnableFullOrientation(enable);
		GameToolBox.Instance.arCamera.EnableWebcam(enable);
	}

	/// <summary>
	/// wait for the download of all the object to start the ar
	/// <see cref="Download.cs"/>
	/// <see cref="Load.cs"/>
	/// </summary>
	/// <returns></returns>
	private IEnumerator WaitForDownload()
	{

		while ((!toolBox.settings.savedRoute && (toolBox.download.toDownload.Count > 0 || toolBox.download.isWorking))
			|| (toolBox.settings.savedRoute && (toolBox.load.toLoadFromDisk.Count > 0 || toolBox.load.isWorking)))
		{
			yield return new WaitForSeconds(1);
		}
		toolBox.loadingScreen.enable(false);
		StartAR();
	}
}

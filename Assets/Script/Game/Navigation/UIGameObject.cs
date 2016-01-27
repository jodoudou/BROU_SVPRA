using UnityEngine;
using System.Collections;

/// <summary>
/// UI for the trigger
/// </summary>
public class UIGameObject: MonoBehaviour
{
	[SerializeField]
	private UIPrefab uiPrefab = null;
	[System.Serializable]
	private class UIPrefab
	{
		public GameObject questionAnswerTrigger = null;
		public GameObject multipleChoiceTrigger = null;
		public GameObject findKeyTrigger = null;
	}

	[HideInInspector]
	public GameObject questionAnswerTrigger { get; private set; }
	[HideInInspector]
	public GameObject multipleChoiceTrigger { get; private set; }
	[HideInInspector]
	public GameObject findKeyTrigger { get; private set; }


	private void Start()
	{
		questionAnswerTrigger = CreatePrefab(uiPrefab.questionAnswerTrigger);
		multipleChoiceTrigger = CreatePrefab(uiPrefab.multipleChoiceTrigger);
		findKeyTrigger = CreatePrefab(uiPrefab.findKeyTrigger);
	}

	private GameObject CreatePrefab(GameObject prefab)
	{
		GameObject go = (GameObject)GameObject.Instantiate(prefab);
		go.transform.SetParent(transform);
		return go;
	}
}



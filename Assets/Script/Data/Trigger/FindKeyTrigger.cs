using System;
using UnityEngine;


/// <summary>
/// Represent a trigger where the user will have to find a key
/// </summary>
[Serializable]
public class FindKeyTrigger: Trigger
{
	//<summary>
	//At which distance we can say the user has found the key.
	//</summary>
	public float toleranceRadius { get; set; }
	/// <summary>
	/// GPS position of the key.
	/// </summary>
	public GeoCoordinate keyPosition { get; set; }

	public string hint { get; set; }

	public string keyObject { get; set; }

	/// <summary>
	/// Build a new trigger
	/// </summary>
	/// <param name="_id">Id of the trigger</param>
	/// <param name="_toleranceRadius">At which distance we can say the user has found the key.</param>
	/// <param name="_keyPosition">GPS position of the key.</param>
	public FindKeyTrigger(String _id, float _toleranceRadius, GeoCoordinate _keyPosition, string _hint, string _keyObject)
		: base(_id)
	{
		this.toleranceRadius = _toleranceRadius;
		this.keyPosition = _keyPosition;
		hint = _hint;
		keyObject = _keyObject;
	}

	protected override void ExecuteTrigger()
	{
		ToolBox.Instance.data.arObjToEnable.Clear();
		ToolBox.Instance.data.arObjToEnable.Add(keyObject);
		GameToolBox.Instance.uiGameObject.findKeyTrigger.GetComponent<FindKeyUI>().Init(this);
		GameToolBox.Instance.gameManager.ShowArObject();
		ToolBox.Instance.notification.ShowNotif(Notification.GO_AR);
	}

	public void KeyIsFind()
	{
		ExecutePostTriggerObject();
	}
}


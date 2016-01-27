using UnityEngine;
using System.Collections;

/// <summary>
/// Camera for google map navigation
/// </summary>
public class NavigationCamera: MonoBehaviour
{
	[SerializeField]
	private Camera objCamera = null;

	void Start()
	{
		transform.localScale = new Vector3((float)Screen.width, (float)Screen.height, 1);
		objCamera.orthographicSize = (transform.localScale.y / 2f) - 2f;
	}
}

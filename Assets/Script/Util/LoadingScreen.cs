using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// loading screen
/// </summary>
public class LoadingScreen: MonoBehaviour
{
	/// <summary>
	/// image shown during the loading
	/// </summary>
	[SerializeField]
	private Transform image = null;

	public void enable(bool show)
	{
		gameObject.SetActive(show);
	}

	/// <summary>
	/// turn the image
	/// </summary>
	private void Update()
	{
		image.Rotate(new Vector3(0, 0, -360) * Time.deltaTime);
	}
}

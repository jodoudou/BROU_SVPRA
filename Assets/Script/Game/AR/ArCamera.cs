using UnityEngine;
using System.Collections;

/// <summary>
/// a camare for the augmented reality
/// </summary>
public class ArCamera: MonoBehaviour, Observer
{
	/// <summary>
	/// Field of view of the camera
	/// </summary>
	private const float FOV = 35;
	/// <summary>
	/// the game object on wich we will render the device webcam video.
	/// </summary>
	[SerializeField]
	private GameObject webcamObject = null;
	/// <summary>
	/// the device webcam video
	/// </summary>
	private WebCamTexture webcam;
	/// <summary>
	/// the index of the webcam that we use
	/// we use the webcam not facing the screen
	/// </summary>
	private int webcamDeviceIndex;
	/// <summary>
	/// the in game position of the camera 
	/// </summary>
	public ARLocation position { get; private set; }

	private void Start()
	{
		webcamDeviceIndex = GetMainWebcam();
		webcam = new WebCamTexture(WebCamTexture.devices[webcamDeviceIndex].name, Screen.width, Screen.height, 30);
		webcam.Play();
		StartCoroutine(InitArCamera());
		position = new ARLocation(new GeoCoordinate());
		ToolBox.Instance.sensorManager.gps.AddObserver(this);
		ToolBox.Instance.sensorManager.orientation.AddObserver(this);
	}

	/// <summary>
	/// Init the camera on the first webcam update
	/// Some of the webcam parameters are only initialize on the first update of the texture
	/// didupdateThisFrame does not work on IOS so we wait 1 seconde and hope its long enough for the webcam to update
	/// http://issuetracker.unity3d.com/issues/webcamtexture-dot-didupdatethisframe-not-working-on-ios
	/// </summary>
	/// <returns></returns>
	private IEnumerator InitArCamera()
	{
#if UNITY_IOS
		yield return new WaitForSeconds(1f);
#else
		while (!webcam.didUpdateThisFrame)
		{
			yield return new WaitForSeconds(0.05f);
		}
#endif

		GetComponent<Camera>().fieldOfView = FOV;
		GetComponent<Camera>().farClipPlane = CalculateFar();
		webcamObject.GetComponent<Renderer>().material.mainTexture = webcam;
		float scaleY = webcam.videoVerticallyMirrored ? -1.0f : 1.0f;
		webcamObject.transform.localScale = new Vector3(webcam.width, scaleY * webcam.height, 1);
		webcamObject.transform.localPosition = new Vector3(0, 0, GetComponent<Camera>().farClipPlane - 1);
	}

	private void OnDestroy()
	{
		EnableWebcam(false);
	}

	public void EnableWebcam(bool enable)
	{
		if (webcam != null)
		{
			if (enable)
			{
				webcam.Play();
			}
			else
			{
				webcam.Stop();
			}
		}
	}

	/// <summary>
	/// Call by the device sensor to orientate the camera 
	/// </summary>
	/// <param name="subject">the sensor</param>
	public void Notify(BaseSubject subject)
	{
		if (subject is GPS)
		{
			position.UpdateArLocation(((GPS)subject).currentPosition);
			transform.position = position.position + (3 * Vector3.up);//we don't use the gps altitude so we manualy put de camera above the ground.
		}
		else if (subject is Orientation)
		{
			Orientation o = (Orientation)subject;
			transform.rotation = Quaternion.Euler(new Vector3(o.pitch, o.azimut, o.roll));
		}
	}

	/// <summary>
	/// Calculate the far of the camera
	/// </summary>
	/// <returns></returns>
	private float CalculateFar()
	{
		float height = ((float)Screen.height / (float)Screen.width) * (float)webcam.width;
		return ((height / 2f) / Mathf.Tan(FOV / 2 * Mathf.Deg2Rad));
	}

	/// <summary>
	/// Get the webcam that is not facing the screen, the back webcam.
	/// Or the first webcam if no back webcam.
	/// </summary>
	/// <returns>the index of the webcam, -1 if no webcam found</returns>
	private int GetMainWebcam()
	{
		int nbWebcam = WebCamTexture.devices.Length;

		if (nbWebcam > 0)
		{
			int i = 0;
			while (i < nbWebcam)
			{
				if (!WebCamTexture.devices[i].isFrontFacing)
				{
					return i;
				}
				i++;
			}
			return 0;
		}
		return -1;
	}
}

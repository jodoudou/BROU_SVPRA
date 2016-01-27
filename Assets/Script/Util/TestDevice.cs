using UnityEngine;
using System.Collections;

public class TestDevice: MonoBehaviour
{
	private void Start()
	{
		TestAll();
	}

	public void TestAll()
	{
		TestInternetConnexion();
	}

	/// <summary>
	/// Test if the device hace an internet connection
	/// </summary>
	#region Test internet
	private const string URL_TEST_INTERNET = "http://svpra.lp-metinet.com/files/TestInternet.txt.713";
	public bool internet { get; private set; }
	public bool isTestingInternet { get; private set; }

	public void TestInternetConnexion()
	{
		if (!isTestingInternet)
		{
			internet = false;
			isTestingInternet = true;
			StartCoroutine(ContinueTestingInternet());
		}
	}

	private IEnumerator ContinueTestingInternet()
	{
		do
		{
			WWW www = new WWW(URL_TEST_INTERNET);
			yield return www;
			if (string.IsNullOrEmpty(www.error) && www.text == "true")
			{
				internet = true;
				isTestingInternet = false;
			}
			else
			{
				ToolBox.Instance.notification.ShowError(Notification.ERROR_INTERNET);
			}
			yield return new WaitForSeconds(5);
		} while (!internet);
	}

	#endregion
}
	
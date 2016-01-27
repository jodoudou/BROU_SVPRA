using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Notification: MonoBehaviour
{
	private const int MAX_TIME_NOTIF = 5;
	public const string ERROR_GPS = "Le GPS semble ne pas fonctionner ou mal fonctionner.";
	public const string ERROR_INTERNET = "Vous devez être connecté à internet.";
	public const string ERROR_ATTRACTION = "Nous avons été dans l'incapacité de récupéré les attractions.";
	public const string ERROR_ROUTE = "Nous avons été dans l'incapacité de récupéré le parcours.";
	public const string SAVE_DELETED = "La sauvegarde locale a été supprimée.";
	public const string NO_SAVE = "Aucun parcours n'est sauvegardé sur l'appareil.";
	public const string GO_AR = "Passer en mode réalité augmentée.";
	public const string FINISH_PARCOURS = "Vous avez terminés le parcours.";

	[SerializeField]
	private Text text = null;
	private float timer = 0;

	public void ShowError(string error)
	{
		text.color = Color.red;
		text.text = error;
		EnableNotif(true);
	}

	public void ShowNotif(string notif)
	{
		text.color = Color.white;
		text.text = notif;
		EnableNotif(true);
	}

	private void EnableNotif(bool show)
	{
		timer = 0;
		gameObject.SetActive(show);
	}

	private void Update()
	{
		if (gameObject.activeInHierarchy)
		{
			timer += Time.deltaTime;
			if (timer >= MAX_TIME_NOTIF)
			{
				EnableNotif(false);
			}
		}
	}
}

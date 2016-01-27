using UnityEngine;
using System.Collections;

public class GameToolBox: MonoBehaviour
{
	public ArCamera arCamera = null;
	public GameManager gameManager = null;
	public NextStep nextStep = null;
	public UIGameObject uiGameObject = null;

	public static GameToolBox Instance = null;

	private void Awake()
	{
		Instance = this;
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

/// <summary>
/// Manage all the user action in the menu
/// </summary>
public class MenuManager: MonoBehaviour, Observer
{
	/// <summary>
	/// route menu
	/// </summary>
	[SerializeField]
	private GameObject canvasRoute = null;
	/// <summary>
	/// attraction menu
	/// </summary>
	[SerializeField]
	private GameObject canvasAttraction = null;
	/// <summary>
	///  first menu
	/// </summary>
	[SerializeField]
	private GameObject canvasMainMenu = null;
	[SerializeField]
	private OptionRoute optionRoute = null;
	/// <summary>
	/// To build all the menu
	/// </summary>
	[SerializeField]
	private MenuBuilder menuBuilder = null;
	[SerializeField]
	private Save save = null;

	private ToolBox toolBox;
	private bool openRoute;

	private void Start()
	{
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
		toolBox = ToolBox.Instance;
		MenuButton.ClickMenuButton += ClickMenuButton;
		toolBox.download.AddObserver(this);
		toolBox.data.ResetData();
		toolBox.loadingScreen.enable(false);
	}

	/// <summary>
	/// To back in the menu
	/// Escape = back button for android
	/// </summary>
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ClickBackButton();
		}
	}

	private void OnDestroy()
	{
		toolBox.download.RemoveObserver(this);
		MenuButton.ClickMenuButton -= ClickMenuButton;
	}

	public void Notify(BaseSubject subject)
	{
		if (subject is Download)
		{
			DownloadType type = ((Download)subject).theDownload.type;
			if (type == DownloadType.Attractions)
			{
				if (toolBox.data.attractions != null)
				{
					BuildMenuAttractions();
				}
				else
				{
					ActiveMenuAttraction(false);
					ActiveMainMenu(true);
					toolBox.loadingScreen.enable(false);
				}
			}
			else if (type == DownloadType.Route)
			{
				if (toolBox.data.route != null)
				{
					if (openRoute)
					{
						GotoNavigation();
					}
					else
					{
						save.SaveRoute();
						toolBox.data.eleRoute = null;
						ActiveOptionRoute(false);
					}
				}
				else
				{
					toolBox.loadingScreen.enable(false);
				}
			}
		}
		else if (subject is GPS)
		{
			subject.RemoveObserver(this);
			toolBox.download.EnqueueAttraction(toolBox.sensorManager.gps.currentPosition);
		}
	}

	public void ClickBackButton()
	{
		if (toolBox.data.eleRoute != null)
		{
			toolBox.data.eleRoute = null;
			ActiveOptionRoute(false);
		}
		else if (toolBox.data.attraction != null)
		{
			toolBox.data.attraction = null;
			ActiveMenuAttraction(true);
			ActiveMenuRoute(false);
		}
		else
		{
			toolBox.data.attractions = null;
			ActiveMainMenu(true);
			ActiveMenuAttraction(false);
		}
	}

	/// <summary>
	/// Call on the click event of the button search in the main menu
	/// </summary>
	public void ClickSearch()
	{
		toolBox.settings.savedRoute = false;
		toolBox.loadingScreen.enable(true);
		toolBox.sensorManager.gps.AddObserver(this);
	}

	/// <summary>
	/// Call on the click event of the button load in the main menu
	/// </summary>
	public void ClickLoad()
	{
		toolBox.settings.savedRoute = true;
		toolBox.loadingScreen.enable(true);
		toolBox.load.LoadAttractions();
		if (toolBox.data.attractions != null)
		{
			BuildMenuAttractions();
		}
		else
		{
			toolBox.notification.ShowNotif(Notification.NO_SAVE);
			toolBox.loadingScreen.enable(false);
		}
	}

	public void ClickDelete()
	{
		save.DeleteAllSave();
	}

	/// <summary>
	/// Call when all the attractions are downloaded from the server.
	/// </summary>
	private void BuildMenuAttractions()
	{
		menuBuilder.CreateAttractionMenu(toolBox.data.attractions);
		ActiveMainMenu(false);
		ActiveMenuAttraction(true);
		toolBox.loadingScreen.enable(false);
	}

	/// <summary>
	/// Call when a button from the route and attraction menu is clicked.
	/// </summary>
	/// <param name="sender">the button</param>
	/// <param name="args"></param>
	public void ClickMenuButton(System.Object sender, EventArgs args)
	{
		MenuButton button = (MenuButton)sender;
		if (toolBox.data.attraction == null)
		{
			toolBox.loadingScreen.enable(true);
			ActiveMenuAttraction(false);
			toolBox.data.attraction = toolBox.data.attractions[button.id];
			menuBuilder.CreateRouteMenu(toolBox.data.attraction);
			ActiveMenuRoute(true);
			toolBox.loadingScreen.enable(false);
		}
		else
		{
			toolBox.data.eleRoute = toolBox.data.attraction.elements[button.id];
			//if (toolBox.settings.savedRoute)
			//{
			//	toolBox.loadingScreen.enable(true);
			//	toolBox.load.LoadRoute();
			//	GotoNavigation();
			//}
			//else
			//{
				optionRoute.Init(toolBox.data.eleRoute);
				ActiveOptionRoute(true);
			//}
		}
	}

	/// <summary>
	/// Call on the click event of one of the button in the OptionRoute menu
	/// </summary>
	/// <param name="open">true to open the route, false to save the route</param>
	public void ClickOptionRoute(bool open)
	{
		openRoute = open;
		toolBox.loadingScreen.enable(true);
		toolBox.download.EnqueueElement(toolBox.data.eleRoute, DownloadType.Route);
	}

	private void ActiveMenuAttraction(bool active)
	{
		canvasAttraction.SetActive(active);
	}

	private void ActiveMenuRoute(bool active)
	{
		canvasRoute.SetActive(active);
	}

	private void ActiveMainMenu(bool active)
	{
		canvasMainMenu.SetActive(active);
	}

	private void ActiveOptionRoute(bool active)
	{
		optionRoute.gameObject.SetActive(active);
	}

	/// <summary>
	/// Call when the route chosen by the user is downloaded from the server.
	/// </summary>
	private void GotoNavigation()
	{
		Application.LoadLevel("Game");
	}
}

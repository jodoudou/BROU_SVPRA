using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Build the menu of the main menu
/// </summary>
public class MenuBuilder: MonoBehaviour
{
	/// <summary>
	/// The button for the attraction and route menu.
	/// </summary>
	[SerializeField]
	private Button buttonPrefab = null;
	/// <summary>
	/// The parent for the attraction menu's button.
	/// </summary>
	[SerializeField]
	private Transform parentAttraction = null;
	/// <summary>
	/// The parent of the route menu's button
	/// </summary>
	[SerializeField]
	private Transform parentRoute = null;

	/// <summary>
	/// Create the a button for each attraction.
	/// </summary>
	/// <param name="attractions">the list of attraction that need a button</param>
	public void CreateAttractionMenu(Dictionary<string, Attraction> attractions)
	{
		if (parentAttraction.childCount > 0)
		{
			RemoveAllChild(parentAttraction);
		}

		int i = 0;
		foreach (Attraction attrac in attractions.Values)
		{
			Button button = (Button)Instantiate(buttonPrefab);
			button.GetComponent<MenuButton>().id = attrac.Id;
			button.GetComponentInChildren<Text>().text = attrac.name;
			button.transform.SetParent(parentAttraction, false);
			i++;
		}
	}

	/// <summary>
	/// Create a button for each route in a attraction
	/// </summary>
	/// <param name="attraction">The attration that contain the routes</param>
	public void CreateRouteMenu(Attraction attraction)
	{
		if (parentRoute.childCount > 0)
		{
			RemoveAllChild(parentRoute);
		}

		Dictionary<string, Element> elements = attraction.elements;
		IEnumerable<Element> route = from elmt in elements.Values where elmt.type == "Parcours" select elmt;

		int i = 0;
		foreach (Element ele in route)
		{
			if (!ToolBox.Instance.settings.savedRoute || (ToolBox.Instance.settings.savedRoute && Load.IsRouteSaved(ele.id, attraction.Id)))
			{
				Button button = (Button)Instantiate(buttonPrefab);
				button.GetComponent<MenuButton>().id = ele.id;
				button.GetComponentInChildren<Text>().text = ele.title;
				button.transform.SetParent(parentRoute, false);
				i++;
			}
		}
#if LOG
		Debug.Log("Number of route: " + i.ToString());
#endif
	}

	/// <summary>
	/// Remove all the child from a game object
	/// </summary>
	/// <param name="parent">the transform of a parent</param>
	private void RemoveAllChild(Transform parent)
	{
		while (parent.childCount > 0)
		{
			Transform child = parent.GetChild(0);
			child.SetParent(null);
			Destroy(child.gameObject);
		}

	}

}

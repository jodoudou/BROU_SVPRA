using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using System.Linq;
using System;

/// <summary>
/// Represent a route
/// </summary>
[Serializable]
public class Route
{
	/// <summary>
	/// Id of the starting waypoint
	/// </summary>
	public string Start { get; set; }
	/// <summary>
	/// Id of the last waypoint.
	/// </summary>
	public string End { get; set; }
	/// <summary>
	/// List of waypoint that composed the route.
	/// </summary>
	public Dictionary<string, Waypoint> Waypoints { get; set; }
	/// <summary>
	/// List of all trigger used in the route.
	/// </summary>
	public Dictionary<string, Trigger> Triggers { get; set; }

	/// <summary>
	/// Create a Route from an xml document
	/// </summary>
	/// <param name="element">The xml document for the route we want to parse</param>
	/// <param name="_userAssembly"></param>
	/// <returns>A newly created Route</returns>
	public static Route Parse(string xml)	
	{
		XElement element = XElement.Parse(xml);

		Route route = new Route();

		XElement waypointsNode = element.Element("Waypoints");//We get the first child of the root node. Which is the WaypointsNode

		//We get our start and end attributes.
		route.Start = waypointsNode.Attribute("start").Value;
		route.End = waypointsNode.Attribute("end").Value;
		route.Waypoints = new Dictionary<String, Waypoint>();

		//We are going to circle through all the waypoint of the route (All the Waypoint node child of Waypoints).
		foreach (XElement waypointNode in waypointsNode.Elements())
		{
			//We get all the infos we need from the node.
			Waypoint waypoint = new Waypoint();
			waypoint.Id = waypointNode.Attribute("id").Value;//Get the id from the attributes.
			waypoint.Title = waypointNode.Element("Title") != null ? waypointNode.Element("Title").Value : "";//We set the title if the node has a Title child.
			waypoint.LatLng = new GeoCoordinate(float.Parse(waypointNode.Element("Lat").Value), float.Parse(waypointNode.Element("Lng").Value), 0);//We parse the value from the Lat and Lng child node value to create the position of the waypoint.
			waypoint.Destinations = new List<String>();
			//We check that we have destinations and circle through all of them to add them to the waypoint.
			if (waypointNode.Element("Next") != null)
			{
				foreach (XElement next in waypointNode.Element("Next").Elements())
				{
					waypoint.Destinations.Add(next.Value);
				}
			}
			//Same for conditions.
			waypoint.Conditions = new List<String>();
			if (waypointNode.Element("Conditions") != null)
			{
				foreach (XElement tId in waypointNode.Element("Conditions").Elements())
				{
					waypoint.Conditions.Add(tId.Value);
				}
			}
			route.Waypoints.Add(waypoint.Id, waypoint);
		}

		route.Triggers = new Dictionary<string, Trigger>();

		if (element.Element("Triggers") != null)
			//We are going to loop through every trigger.
			foreach (XElement triggerElement in element.Element("Triggers").Elements())
			{
				//	For every trigger we get its factory and then we create a new trigger.
				TriggerFactory factory = TriggerFactory.GetFactory(triggerElement);
				Trigger t = factory.CreateTrigger(triggerElement);
				route.Triggers.Add(t.id, t);
			}

		return route;
	}
}

/// <summary>
/// Represent a waypoint of a route
/// </summary>
[Serializable]
public class Waypoint
{
	/// <summary>
	/// Unique id of the waypoint
	/// </summary>
	public String Id { get; set; }
	/// <summary>
	/// A title for this waypoint, optional
	/// </summary>
	public String Title { get; set; }
	/// <summary>
	/// Location of the waypoints
	/// </summary>
	public GeoCoordinate LatLng { get; set; }
	/// <summary>
	/// Waypoints where we can go from this point
	/// </summary>
	public List<String> Destinations { get; set; }
	/// <summary>
	/// Triggers associed with this trigger.
	/// </summary>
	public List<String> Conditions { get; set; }
}


using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Control all the navigation and direction to take between waypoint 
/// active the trigger.
/// </summary>
public class RouteNavigation: Observer
{
	/// <summary>
	/// distance to a goal to activate it
	/// </summary>
	private const float DISTANCE_GOAL_TRIGGER = 6;
	private ToolBox toolBox;
	private Data data;
	/// <summary>
	/// Waypoint where the user need to go
	/// </summary>
	public Waypoint goal { get; private set; }
	/// <summary>
	/// the next waypoint
	/// </summary>
	private Waypoint nextGoal = null;
	/// <summary>
	/// index of the condition of the waypoint we are in
	/// </summary>
	private int conditionIndex = 0;
	/// <summary>
	/// if user in a trigger
	/// </summary>
	private bool inTrigger = false;
	/// <summary>
	/// line that indicate the direction to take
	/// using google direction
	/// </summary>
	private CustomLineRenderer lineRenderer = null;
	/// <summary>
	/// the line that indicate the direction to take
	/// </summary>
	private LineRenderer directLineRenderer = null;
	/// <summary>
	/// the google map
	/// </summary>
	private GoogleMapNavigation mapNavigation;
	/// <summary>
	/// number of point in the lineRenderer
	/// </summary>
	private int vertexCount = 1;
	/// <summary>
	/// all the GPS point between us and the goal
	/// </summary>
	private List<GeoCoordinate> points = new List<GeoCoordinate>();

	public RouteNavigation(CustomLineRenderer line, LineRenderer directLine, GoogleMapNavigation nav)
	{
		lineRenderer = line;
		mapNavigation = nav;
		directLineRenderer = directLine;
		toolBox = ToolBox.Instance;
		data = toolBox.data;
		goal = data.route.Waypoints[data.route.Start];
		if (goal.Destinations.Count > 0)
		{
			nextGoal = data.route.Waypoints[goal.Destinations[0]];
		}
		toolBox.download.AddObserver(this);
	}

	/// <summary>
	/// add the next trigger objects for the augmented reality to the queue to get downloaded
	/// <see cref="Download.cs"/>
	/// <see cref="Load.cs"/>
	/// </summary>
	public void DownLoadGoal()
	{
		foreach (string triggerId in goal.Conditions)
		{
			Trigger trigger = data.route.Triggers[triggerId];
			foreach (string objectId in trigger.preTriggerObjectIds)
			{
				if (toolBox.settings.savedRoute)
				{
					toolBox.load.toLoadFromDisk.Enqueue(data.attraction.elements[objectId]);
				}
				else
				{
					toolBox.download.EnqueueElement(data.attraction.elements[objectId], DownloadType.Element);
				}
			}
			foreach (string objectId in trigger.postTriggerObjectIds)
			{
				if (toolBox.settings.savedRoute)
				{
					toolBox.load.toLoadFromDisk.Enqueue(data.attraction.elements[objectId]);
				}
				else
				{
					toolBox.download.EnqueueElement(data.attraction.elements[objectId], DownloadType.Element);
				}
			}
			if (trigger is FindKeyTrigger)
			{
				if (toolBox.settings.savedRoute)
				{
					toolBox.load.toLoadFromDisk.Enqueue(data.attraction.elements[((FindKeyTrigger)trigger).keyObject]);
				}
				else
				{
					toolBox.download.EnqueueElement(data.attraction.elements[((FindKeyTrigger)trigger).keyObject], DownloadType.Element);
				}
			}
		}
	}

	/// <summary>
	/// call by a trigger when the user finish it
	/// </summary>
	/// <param name="subject">the trigger</param>
	public void Notify(BaseSubject subject)
	{
		if (subject is Trigger)
		{
			if (subject is MultipleChoiceTrigger)
			{
				MultipleChoiceTrigger trigger = (MultipleChoiceTrigger)subject;
				if (data.route.Waypoints.ContainsKey(trigger.ChoiceMade.Waypoint))
				{
					nextGoal = data.route.Waypoints[trigger.ChoiceMade.Waypoint];
				}
			}
			subject.RemoveObserver(this);
			executeNextCondition();
		}
		else if (subject is Download)
		{
			Download download = (Download)subject;
			if (download.theDownload.type == DownloadType.Direction)
			{
				UpdateDirection(Polyline.GetPolylineFromXML(download.lastDownload.text));
			}
		}
	}

	/// <summary>
	/// Call when the user move
	/// update the lineRenderer and check if we are at the goal
	/// </summary>
	public void UpdateRouteNavigation()
	{
		if (!inTrigger)
		{
			directLineRenderer.SetPosition(0, mapNavigation.userPosition);
			lineRenderer.SetPoint(vertexCount - 1, mapNavigation.userPosition);
			for (int i = points.Count - (vertexCount - 1); i < points.Count; i++)
			{
				double distance = points[i].GetMeterDistanceTo(mapNavigation.userGPSLocation);
				if (distance < DISTANCE_GOAL_TRIGGER)
				{
					vertexCount -= (i + 1) - (points.Count - (vertexCount - 1));
					lineRenderer.SetNbPoint(vertexCount);
					lineRenderer.SetPoint(vertexCount - 1, mapNavigation.userPosition);

					if (vertexCount < 2)
					{
						directLineRenderer.SetVertexCount(1);
						inTrigger = true;
						executeNextCondition();
					}
					break;
				}
			}
		}
	}

	/// <summary>
	/// execute the next condition of the trigger.
	/// if no more condition, go to the next waypoint
	/// </summary>
	private void executeNextCondition()
	{
		if (conditionIndex < goal.Conditions.Count)
		{
			Trigger trigger = data.route.Triggers[goal.Conditions[conditionIndex]];
			trigger.AddObserver(this);
			trigger.Execute();
			conditionIndex++;
		}
		else
		{
			conditionIndex = 0;
			if (nextGoal != null)
			{
				goal = nextGoal;
				nextGoal = null;
				if (goal.Destinations.Count > 0)
				{
					nextGoal = data.route.Waypoints[goal.Destinations[0]];
				}
				DownLoadGoal();
				toolBox.download.EnqueueDirection(mapNavigation.userGPSLocation, goal.LatLng);
				lineRenderer.Reset();
				inTrigger = false;
			}
			else
			{
				toolBox.loadingScreen.enable(true);
				toolBox.notification.ShowNotif(Notification.FINISH_PARCOURS);
				toolBox.sensorManager.orientation.EnableFullOrientation(false);
				Application.LoadLevel(1);
			}
		}
	}

	/// <summary>
	/// update the line renderer position
	/// </summary>
	public void UpdateLine()
	{
		if (points != null && !inTrigger)
		{
			int j = points.Count;
			directLineRenderer.SetPosition(1, UpdateLineRendererPoint(0, goal.LatLng.Longitude, goal.LatLng.Latitude));
			for (int i = vertexCount - 1; i >= 1; i--)
			{
				UpdateLineRendererPoint(i - 1, points[j - i].Longitude, points[j - i].Latitude);
			}
			lineRenderer.SetPoint(vertexCount - 1, mapNavigation.userPosition);
		}
	}

	/// <summary>
	/// init the linerenderer from a polyline
	/// </summary>
	/// <param name="polyline">the polyline</param>
	private void UpdateDirection(string polyline)
	{
		points = new List<GeoCoordinate>(Polyline.Decode(polyline));
		points.Add(goal.LatLng);
		vertexCount = points.Count + 1;
		lineRenderer.SetNbPoint(vertexCount);
		directLineRenderer.SetVertexCount(2);
		directLineRenderer.SetPosition(0, UpdateLineRendererPoint(1, goal.LatLng.Longitude, goal.LatLng.Latitude));
		for (int i = points.Count - 1; i >= 0; i--)
		{
			UpdateLineRendererPoint(vertexCount - (i + 2), points[i].Longitude, points[i].Latitude);
		}
		lineRenderer.SetPoint(vertexCount - 1, mapNavigation.userPosition);
	}

	/// <summary>
	/// update a point of the line renderer
	/// </summary>
	/// <param name="index">index of the point</param>
	/// <param name="lng">longitude of the point</param>
	/// <param name="lat">latitude of the point</param>
	private Vector2 UpdateLineRendererPoint(int index, float lng, float lat)
	{
		Vector2 vec = new Vector2();
		vec.x = (mapNavigation.projection.GetXFromLongitude(lng) - mapNavigation.screenMapCenter.x) * mapNavigation.mapExpandRatio;
		vec.y = (mapNavigation.projection.GetYFromLatitude(lat) - mapNavigation.screenMapCenter.y) * mapNavigation.mapExpandRatio;
		lineRenderer.SetPoint(index, vec);
		return vec;
	}
}

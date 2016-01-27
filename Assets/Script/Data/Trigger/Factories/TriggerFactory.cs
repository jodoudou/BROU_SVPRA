using System;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;


/// <summary>
/// Base class for trigger factory, see abstract factory design pattern
/// </summary>
public abstract class TriggerFactory
{
	/// <summary>
	/// The method for creating a trigger
	/// </summary>
	/// <param name="_trigger">The xml element containing the trigger description (Root node = Trigger)</param>
	/// <returns>The newly created trigger.</returns>
	public abstract Trigger CreateTrigger(XElement _trigger);

	/// <summary>
	/// Returns a factory to create a trigger of the given type
	/// </summary>
	/// <param name="_trigger">XElement containing all the informations of the trigger</param>
	/// <param name="_userAssembly"></param>
	/// <returns>A factory for that trigger</returns>
	public static TriggerFactory GetFactory(XElement _trigger)
	{
		TriggerFactory factory = null;
		//We create the type of the factory, by convention tha factory type = "<TriggerType>Factory"
		String triggerFactoryName = _trigger.Elements().First().Name + "Factory";

		Type triggerFactoryType = Type.GetType(triggerFactoryName);

		if (triggerFactoryType != null)
		{
			factory = (TriggerFactory)Activator.CreateInstance(triggerFactoryType);
		}

		//We return the factory
		return factory;
	}
}


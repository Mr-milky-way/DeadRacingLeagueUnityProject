using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class AkEnumFlagAttribute : PropertyAttribute
{
	public Type Type;

	public AkEnumFlagAttribute(Type type)
	{
		Type = type;
	}
}

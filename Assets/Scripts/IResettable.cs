using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface IResettable
{
	/// <summary>
	/// Resets certain turn state variables
	/// </summary>
	void ResetUnit ();
}

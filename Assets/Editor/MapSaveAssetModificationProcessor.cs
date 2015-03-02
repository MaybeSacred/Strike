// //Created by Jon Tyson : jtyson3@gatech.edu
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.IO;
using UnityEngine;


public class MapSaveModificationProcessor : AssetModificationProcessor
{
	public static string[] OnWillSaveAssets (string[] paths)
	{
		// Get the name of the scene to save.
		string scenePath = string.Empty;
		string sceneName = string.Empty;
		foreach (string path in paths) {
			if (path.Contains ("/Maps") && path.Contains (".unity")) {
				scenePath = Path.GetDirectoryName (path);
				sceneName = Path.GetFileNameWithoutExtension (path);
			}
		}
		if (sceneName.Length == 0) {
			return paths;
		}
		// do stuff
		MapDataExporter.ExportMapDataToFile ();
		return paths;
	}
}

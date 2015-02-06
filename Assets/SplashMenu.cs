using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class SplashMenu : MonoBehaviour {
	public GUIStyle splashMenuGuiStyle;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void OnLoadGameMenu(){
		Application.LoadLevel("GameMenu");
	}
	public void OnLoadOptions(){
		Application.LoadLevel("Options");
	}
#if UNITY_STANDALONE
	private bool checkIfJavaIsInstalled()
	{
		bool ok = false;
		Process process = new Process();
		try
		{
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.Arguments = "/c \"" + "java -version " +  "\"";
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			
			process.WaitForExit();
			
			ok = (process.ExitCode == 0);
		}
		catch
		{
			
		}
		
		return (ok);
	}
#endif
}

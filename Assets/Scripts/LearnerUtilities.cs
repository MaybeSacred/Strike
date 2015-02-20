// //Created by Jon Tyson : jtyson3@gatech.edu

using System;
using System.Diagnostics;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;


public class LearnerUtilities
{
	private static String jrePath = "C:\\Program Files (x86)\\Java\\jre7\\bin\\javaw.exe";
	private static String wekaClassPath = Application.dataPath + @"\Data\weka.jar";
	private static String dataPath = Application.dataPath + @"\Data\";
	public static String dataFileName = "ProductionTrainingData";
	public static String reinforcementDataFileName = "ProductionTrainingReinforcementData";
	public static string reinforcementEventDataFileName = "ProductionEventReinforcementData";
	public static String eventDataFileName = "ProductionEventData";
	public static float learningRate;
	public static float learningMomentum;
	public static String currentClassifier = "weka.classifiers.trees.LADTree";
	private static Process exeProcess;
	private static float classificationThreshold = .075f;
	private static Process neuralNetTrainer;
	private static string logFile = "log.txt";
#if UNITY_STANDALONE
	public static void SetJREPath ()
	{
		LearnerUtilities.jrePath = LearnerUtilities.GetJavaInstallationPath ();
	}
	public static void SetWekaPath (String path)
	{
		LearnerUtilities.wekaClassPath = path;
	}
	public static void SetDataPath (String path)
	{
		LearnerUtilities.dataPath = path;
	}
	public static String GetJREPath ()
	{
		return LearnerUtilities.jrePath;
	}
	public static String GetWekaPath ()
	{
		return LearnerUtilities.wekaClassPath;
	}
	public static String GetDataPath ()
	{
		return LearnerUtilities.dataPath;
	}
	public static void TrainCurrentClassifier (string file)
	{
		TrainLADTree (file);
	}
	/// <summary>
	/// Launch Weka with settings for Neural Net.
	/// </summary>
	private static void TrainBFTree (string file)
	{
		Process exeProcess = new Process ();
		exeProcess.StartInfo.CreateNoWindow = false;
		exeProcess.StartInfo.UseShellExecute = false;
		exeProcess.StartInfo.FileName = jrePath;
		//exeProcess.StartInfo.Arguments = "-Dfile.encoding-Cp1252 -classpath \"" + LearnerUtilities.wekaClassPath + "\" weka.classifiers.meta.RandomCommittee -S 1 -I 10 -W weka.classifiers.trees.BFTree -- -S 1 -M 2 -N 5 -C 1.0 -P POSTPRUNED -t " + LearnerUtilities.dataPath + file + ".arff -k -d " + LearnerUtilities.dataPath + file + ".model";
		exeProcess.StartInfo.Arguments = "-Dfile.encoding-Cp1252 -classpath \"" + LearnerUtilities.wekaClassPath + "\" weka.classifiers.trees.J48 -C 0.25 -M 2 -t " + LearnerUtilities.dataPath + file + ".arff -k -d " + LearnerUtilities.dataPath + file + ".model";
		exeProcess.Start ();
	}
	private static void TrainLADTree (string file)
	{
		Process exeProcess = new Process ();
		exeProcess.StartInfo.CreateNoWindow = false;
		exeProcess.StartInfo.UseShellExecute = false;
		exeProcess.StartInfo.FileName = jrePath;
		exeProcess.StartInfo.Arguments = "-Dfile.encoding-Cp1252 -classpath \"" + LearnerUtilities.wekaClassPath + "\" weka.classifiers.trees.LADTree -B 10 -t " + LearnerUtilities.dataPath + file + ".arff -k -d " + LearnerUtilities.dataPath + file + ".model";
		exeProcess.Start ();
	}
	/// <summary>
	/// Launch Weka with settings for Neural Net.
	/// </summary>
	private static void TrainNeuralNet (string file)
	{
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor) {
			neuralNetTrainer = new Process ();
			neuralNetTrainer.StartInfo.CreateNoWindow = false;
			neuralNetTrainer.StartInfo.UseShellExecute = false;
			neuralNetTrainer.StartInfo.FileName = jrePath;
			neuralNetTrainer.EnableRaisingEvents = true;
			neuralNetTrainer.Exited += new EventHandler (myProcess_Exited);
			neuralNetTrainer.StartInfo.Arguments = "-Dfile.encoding-Cp1252 -classpath \"" + LearnerUtilities.wekaClassPath + "\" weka.classifiers.functions.MultilayerPerceptron -t " + LearnerUtilities.dataPath + file + ".arff -k -d " + LearnerUtilities.dataPath + file + ".model";
			neuralNetTrainer.Start ();
			WriteIDToFile (neuralNetTrainer.Id);
		}
	}
	private static void WriteIDToFile (int id)
	{
		try {
			FileStream fs = File.Create (dataPath + logFile);
			BinaryWriter bw = new BinaryWriter (fs);
			string data = id.ToString ();
			ASCIIEncoding asen = new ASCIIEncoding ();
			byte[] ba = asen.GetBytes (data);
			bw.Write (ba);
			bw.Close ();
			fs.Close ();
		} catch (Exception e) {
			UnityEngine.Debug.Log (e);
		}
	}
	private static int ReadIDFromFile ()
	{
		using (var sr = new StreamReader(dataPath + logFile)) {
			string line = sr.ReadLine ();
			return int.Parse (line);
		}
	}
	public static void TerminateNeuralTraining ()
	{
		Process[] processes = Process.GetProcesses ();
		int id = ReadIDFromFile ();
		for (int i = 0; i < processes.Length; i++) {
			try {
				if (processes [i].Id == id) {
					processes [i].Kill ();
				}
			} catch (InvalidOperationException e) {
				
			}
		}
	}
	private static void myProcess_Exited (object sender, System.EventArgs e)
	{
		UnityEngine.Debug.Log ("Neural net ended");
	}
	
	public static void BeginProductionClassification (string file)
	{
		exeProcess = new Process ();
		exeProcess.StartInfo.RedirectStandardError = true;
		exeProcess.StartInfo.RedirectStandardOutput = true;
		exeProcess.StartInfo.CreateNoWindow = false;
		exeProcess.StartInfo.UseShellExecute = false;
		exeProcess.StartInfo.FileName = jrePath;
		exeProcess.StartInfo.Arguments = "-Dfile.encoding-Cp1252 -classpath \"" + LearnerUtilities.wekaClassPath + "\" " + LearnerUtilities.currentClassifier + " -l \"" + LearnerUtilities.dataPath + LearnerUtilities.dataFileName + ".model\" -T \"" + LearnerUtilities.dataPath + LearnerUtilities.eventDataFileName + ".arff\" -p 0 -distribution";
		UnityEngine.Debug.Log (exeProcess.StartInfo.Arguments);
		exeProcess.Start ();
	}
	public static void BeginProductionReinforcementClassification (string file)
	{
		exeProcess = new Process ();
		exeProcess.StartInfo.RedirectStandardError = true;
		exeProcess.StartInfo.RedirectStandardOutput = true;
		exeProcess.StartInfo.CreateNoWindow = false;
		exeProcess.StartInfo.UseShellExecute = false;
		exeProcess.StartInfo.FileName = jrePath;
		exeProcess.StartInfo.Arguments = "-Dfile.encoding-Cp1252 -classpath \"" + LearnerUtilities.wekaClassPath + "\" " + LearnerUtilities.currentClassifier + " -l \"" + LearnerUtilities.dataPath + LearnerUtilities.reinforcementDataFileName + ".model\" -T \"" + LearnerUtilities.dataPath + LearnerUtilities.reinforcementEventDataFileName + ".arff\" -p 0 -distribution";
		
		UnityEngine.Debug.Log (exeProcess.StartInfo.Arguments);
		exeProcess.Start ();
	}
	public static UnitName CheckProductionClassification ()
	{
		if (exeProcess.HasExited) {
			UnityEngine.Debug.Log (exeProcess.StandardError.ReadToEnd ());
			return GetUnitNameFromWekaString (exeProcess.StandardOutput.ReadToEnd ());
		}
		return UnitName.Headquarters;
	}
	public static List<UnitName> CheckProductionClassificationRanked ()
	{
		if (exeProcess.HasExited) {
			UnityEngine.Debug.Log (exeProcess.StandardError.ReadToEnd ());
			return GetUnitNamesFromWekaString (exeProcess.StandardOutput.ReadToEnd ());
		}
		return null;
	}
	/// <summary>
	/// Checks whether the jvm has returned a classification yet
	/// </summary>
	/// <returns>The production classification reinforcement.</returns>
	public static List<UnitName> CheckProductionClassificationReinforcement ()
	{
		if (exeProcess.HasExited) {
			return GetUnitNamesFromWekaString (exeProcess.StandardOutput.ReadToEnd ());
		}
		return null;
	}
	/// <summary>
	/// Gets a single name from weka string.
	/// For use with weka classification that returns one best class
	/// </summary>
	/// <returns>The unit name from weka string.</returns>
	/// <param name="input">Input.</param>
	private static UnitName GetUnitNameFromWekaString (string input)
	{
		String[] split = input.Split (":".ToCharArray (), StringSplitOptions.None);
		split = split [2].Split (" ".ToCharArray (), StringSplitOptions.None);
		split [0] = split [0].Trim (" ".ToCharArray ());
		return GetUnitNameFromString (split [0]);
	}
	/// <summary>
	/// Gets the unit names from weka classification string.
	/// </summary>
	/// <returns>List of unitNames</returns>
	/// <param name="input">Input.</param>
	private static List<UnitName> GetUnitNamesFromWekaString (string input)
	{
		UnityEngine.Debug.Log (input);
		String[] split = input.Split (" ".ToCharArray (), StringSplitOptions.None);
		split = split [split.Length - 2].Split (new string[]{",", "*"}, StringSplitOptions.RemoveEmptyEntries);
		return ParseUnitNames (split);
	}
	/// <summary>
	/// Parses a weka classification distribution to find unitnames larger than classification threshold
	/// </summary>
	/// <returns>The unit names.</returns>
	/// <param name="inArray">In array.</param>
	private static List<UnitName> ParseUnitNames (string[] inArray)
	{
		Array values = System.Enum.GetValues (typeof(UnitName));
		List<UnitName> likeliestValues = new List<UnitName> ();
		for (int i = 0; i < inArray.Length; i++) {
			if (float.Parse (inArray [i]) >= classificationThreshold) {
				likeliestValues.Add ((UnitName)values.GetValue (i));
			}
		}
		return likeliestValues;
	}
	/// <summary>
	/// Gets the unit name from a string returned from weka classification process
	/// </summary>
	/// <returns>The unit name from string.</returns>
	/// <param name="input">Input.</param>
	private static UnitName GetUnitNameFromString (string input)
	{
		foreach (string name in System.Enum.GetNames(typeof(UnitName))) {
			if (name.Contains (input)) {
				return (UnitName)System.Enum.Parse (typeof(UnitName), name, true);
			}
		}
		throw new Exception ();
	}
	/// <summary>
	/// Gets the java installation path of local computer
	/// </summary>
	/// <returns>The java installation path.</returns>
	private static string GetJavaInstallationPath ()
	{
		string environmentPath = Environment.GetEnvironmentVariable ("JAVA_HOME");
		if (!string.IsNullOrEmpty (environmentPath)) {
			return environmentPath;
		}
		string javaKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment\\";
		using (Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(javaKey)) {
			string currentVersion = rk.GetValue ("CurrentVersion").ToString ();
			using (Microsoft.Win32.RegistryKey key = rk.OpenSubKey(currentVersion)) {
				return System.IO.Path.Combine (key.GetValue ("JavaHome").ToString (), "bin\\java.exe");
			}
		}
	}
#endif
}


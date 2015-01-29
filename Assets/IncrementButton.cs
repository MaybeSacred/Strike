using UnityEngine;
using UnityEngine.UI;
using System.Collections;
/// <summary>
/// Controls an input field or text with two buttons that in/decrement the value contained within
/// </summary>
public class IncrementButton : MonoBehaviour {
	/// <summary>
	/// Value to increase/decrease by
	/// </summary>
	public int minValue, maxValue, incrementValue;
	public Button incrementer, decrementer;
	public UnityEngine.Events.UnityAction settings;
	InputField inputField;
	// Use this for initialization
	void Start () {
		inputField = GetComponent<InputField>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	/// <summary>
	/// Increment the text
	/// </summary>
	public void Increment(){
		int curValue = int.Parse(inputField.text);
		curValue = Clamp(curValue + incrementValue);
		inputField.text = curValue.ToString();
	}
	/// <summary>
	/// Decrement the text
	/// </summary>
	public void Decrement(){
		int curValue = int.Parse(inputField.text);
		curValue = Clamp(curValue - incrementValue);
		inputField.text = curValue.ToString();
	}
	/// <summary>
	/// Validate input to input field, clamping it
	/// </summary>
	/// <param name="input">Input.</param>
	public void Validate(string input){
		int curValue = 0;
		int.TryParse(input, out curValue);
		curValue = Clamp(curValue);
		inputField.text = curValue.ToString();
	}
	public int GetValue(){
		return int.Parse(inputField.text);
	}
	public string GetRawValue(){
		return inputField.text;
	}
	/// <summary>
	/// Clamps a numerical value to between 0 and 10^x - 1
	/// </summary>
	/// <param name="value">Value.</param>
	int Clamp(int value){
		if(value < minValue){
			value = minValue;
		}
		else if(value > maxValue){
			value = maxValue;
		}
		return value;
	}
}

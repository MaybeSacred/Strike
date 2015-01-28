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
	public int incrementValue;
	public Button incrementer, decrementer;
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
	/// <summary>
	/// Clamps a numerical value to between 0 and 10^x - 1
	/// </summary>
	/// <param name="value">Value.</param>
	int Clamp(int value){
		if(value < 0){
			value = 0;
		}
		else if(value > Mathf.Pow(10, GetComponent<InputField>().characterLimit - 1)){
			value = Mathf.RoundToInt(Mathf.Pow(10, GetComponent<InputField>().characterLimit - 1));
		}
		return value;
	}
}

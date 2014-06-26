using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameButtons : MonoBehaviour
{
	public static GameButtons Instance;
	
	private Color primaryGUIColor;
	private Color secondaryGUIColor;
	
	private bool hasSetGUI = false;
	private int currentColor;
	
	private Vector2 buttonSize = new Vector2(100, 64);
	private int buttonVerticalSpacing = 32;
	
	#region Monobehaviour
	
	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		if (this != Instance)
			Destroy(gameObject);
	}
	
	void Start()
	{
		primaryGUIColor = MainMenu.Instance.primaryGUIColor;
		secondaryGUIColor = MainMenu.Instance.secondaryGUIColor;
		hasSetGUI = false;
	}
	
	void OnDisable()
	{
		hasSetGUI = false;
	}
	
	void OnGUI() 
	{
		if (!hasSetGUI) {
			ColoredGUISkinMobile.Instance.UpdateGuiColors(primaryGUIColor, secondaryGUIColor);
			hasSetGUI = true;
		}
		
		GUI.skin = ColoredGUISkinMobile.Skin;
		
		DrawGameButtons();
	}
	
	void DrawGameButtons()
	{
		GUILayout.BeginArea(new Rect(0,0, Screen.width, Screen.height));

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		GUILayout.BeginVertical();

		bool isPaused = Time.timeScale == 0;
		string pauseBtnTitle = isPaused ? "Resume" : "Pause";
		bool togglePause = GUILayout.Button(pauseBtnTitle, GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y));
		if (togglePause)
		{
			GameManager.Instance.TogglePause();
		}

		GUILayout.Space(-20);

		string soundBtnTitle = Sound.Instance.mute ? "unmute" : "mute";
		bool toggleSound = GUILayout.Button(soundBtnTitle, GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y));
		if (toggleSound)
		{
			Sound.Instance.mute = !Sound.Instance.mute;
		}
	
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();
	}
	
	#endregion Monobehaviour
	
	#region Setup the Game
	
	//	void StartSoloPlay()
	//	{
	//
	//		Application.LoadLevel("Game");
	//	}
	
	#endregion Setup the Game
}

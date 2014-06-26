using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
	public static MainMenu Instance;
	
	public Color primaryGUIColor;
	public Color secondaryGUIColor;

	private bool hasSetGUI = false;
	private int currentColor;
	
	private Vector2 buttonSize = new Vector2(260, 80);
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
		hasSetGUI = false;
	}

	void OnDisable()
	{
		hasSetGUI = false;
	}

	void OnGUI() 
	{
		if (!Application.loadedLevelName.Equals("Initial"))
		{
			hasSetGUI = false;
			return;
		}

		if (!hasSetGUI) {
			ColoredGUISkinMobile.Instance.UpdateGuiColors(primaryGUIColor, secondaryGUIColor);
			hasSetGUI = true;
		}

		GUI.skin = ColoredGUISkinMobile.Skin;

		DrawMenuButtons();
	}

	void DrawMenuButtons()
	{
		GUILayout.BeginArea(new Rect(0,0, Screen.width, Screen.height));

		GUILayout.Space(100);

		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		GUILayout.BeginVertical();

//		GUI.color = new Color(171/255f, 190/255f, 63/255f);

		//Different menu options depending on player compliance to the experiment
		if (GamEXP.isEnabled)
		{
			bool playSingle = GUILayout.Button("Play and customize", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y));
			if (playSingle)
			{
				GamEXP.Instance.StartSoloPlay();
			}

		
			bool playtest = GUILayout.Button("Play and rate", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y));
			if (playtest)
			{
				GamEXP.Instance.LoadPlaytest();
			}

			GUILayout.Space(-20);
		}
		else
		{
			GUILayout.Space(-50);

			bool playSolo = GUILayout.Button("Play!", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y));
			if (playSolo)
			{
				StartPlaying();
			}
		}		

		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();

		GUILayout.EndArea();
	}

	#endregion Monobehaviour

	#region Setup the Game

	void StartPlaying()
	{

		Application.LoadLevel("Game");
	}
	
	#endregion Setup the Game
}

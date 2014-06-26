using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//One big class packing the most for the GamEXP
public class GamEXP : MonoBehaviour
{
	public enum Window
	{
		AcceptTerms,
		ServerGreet,
		SoloPlayer,
		Playtest,
		Pause,
		GameOver,
		Feedback,
		Configuration,
		ProcessingRequest,
		FailedRequest,
		SuccessfulRequest,
		None
	}

	public enum State
	{
		SoloPlayer,
		Playtest,
//		Feedback
	}


	public static GamEXP Instance;

	//Configurations
	public static GameConfiguration PlaytestConfig;
	public static GameConfiguration PlayerConfig;
	private int minPlaytestsToSendConfig = Constants.MIN_PLAYTEST_COUNT;
	private GameConfiguration customizingConfig;

	private static Player player;

	public static bool isEnabled = false;
	public static State state = State.SoloPlayer;

	//UI customization
	public Color primaryGUIColor;
	public Color secondaryGUIColor;
//	public Texture starOn;
//	public Texture starOff;
	private bool hasSetGUI = false;

	//Window layout
	private Rect smallWindowRect;
	private Rect mediumWindowRect;
	private Rect bigWindowRect;

	private Vector2 buttonSize = new Vector2 (240, 64);
	private Vector2 smallButtonSize = new Vector2 (150, 64);
	private float verticalSpacing = -20;
	private float largeVerticalSpacing = -15;

	//GUI state
	private Color guiColor;
	private static bool showGUI = true;
	private bool processingRequest = false;
	//Frame rate independent using Time.realTimeSinceStartup
	private float windowTimer = Constants.UNDEFINED;
	private float processingRequestTimer = Constants.UNDEFINED;
	public static Window window = Window.ServerGreet;
	private Window previousWindow = Window.None;

	//Feedback
	private Feedback feedback;
	public string text = "test";

	private delegate WWW RequestDelegate(string url, byte[] data, Hashtable header);

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
		Setup();

//		ShowMainMenu();
	}

	void Setup()
	{
		smallWindowRect = new Rect(0, 0, 320, 240);
		mediumWindowRect = new Rect(0, 0, 400, 320);
		bigWindowRect = new Rect(0, 0, 400, 450);

		//Put the window on the middle of the screen
		smallWindowRect.center = new Vector2(Screen.width/2f, Screen.height/2f);
		mediumWindowRect.center = smallWindowRect.center;
		bigWindowRect.center = smallWindowRect.center;

		if (window == Window.AcceptTerms)
			BecomeActive(true);

		hasSetGUI = false;
		
		guiColor = primaryGUIColor; 

		//Set a default configuration
		PlayerConfig = new GameConfiguration();
	}

	void OnGUI()
	{
		if (window == Window.None)
			return;

		if (!showGUI)
		{
			hasSetGUI = false;
			return;
		}

		if (!hasSetGUI)
		{
			ColoredGUISkinMobile.Instance.UpdateGuiColors(primaryGUIColor, secondaryGUIColor);
			hasSetGUI = true;
		}
		
		GUI.skin = ColoredGUISkinMobile.Skin;
	
//		if (windowTimer != Parameters.UNDEFINED) windowTimer -= Time.deltaTime; 

		//Participation window
//		if (window == Window.AcceptTerms)
//		{
//			GUI.ModalWindow(0, mediumWindowRect, WindowAcceptTerms, "Participate on the tests!");
//		}
		//Connect to the server
		if (window == Window.ServerGreet || window == Window.ProcessingRequest)
		{
			GUI.Window (1, smallWindowRect, WindowProcessingLoginRequest, "Processing");
		}
		else if (window == Window.SoloPlayer || window == Window.None)
		{
			BecomeActive(false);
		}
		else if (window == Window.FailedRequest)
		{
			if (Time.realtimeSinceStartup < windowTimer)
				GUI.Window(2, smallWindowRect, WindowFailedRequest, "Oops!");
			else 
				window = previousWindow;
		}
		else if (window == Window.SuccessfulRequest)
		{
			if (Time.realtimeSinceStartup < windowTimer)
				GUI.Window(3, smallWindowRect, WindowSuccessfulRequest, "Success!");
			else
				window = Window.None;
		}
		else if (window == Window.Playtest)
		{
			GUI.Window(4, smallWindowRect, WindowProcessingGetConfig, "Loading");
		}
		else if (window == Window.Feedback)
		{
			GUI.Window(5, mediumWindowRect, WindowFeedback, "Rate the last played round!");
		}
		else if (window == Window.Configuration)
		{
			string windowTitle = GameManager.Instance.isGameOver ? "Tweak the Game!" : "Game Paused";
			GUI.Window(6, bigWindowRect, WindowConfiguration, windowTitle);
		}
		else if (window == Window.Pause || window == Window.GameOver)
		{
			GUI.Window(7, mediumWindowRect, WindowPause, window == Window.GameOver? "Game Over" : "Game Paused");
		}
	}

	#endregion Monobehaviour

	#region Windows
	void WindowAcceptTerms(int windowID)
	{
		string encouragingText = "Take part on customization and tests of the game to make it better!";
		string termsText = "By clicking \"accept\", you'll be sending us information about your hardware and actions made in the game";
//		GUI.backgroundColor = Color.white;
//		GUI.color = new Color(1f, 1f, 1f, 1f);

		GUILayout.BeginVertical();

		//The terms
		GUILayout.Space(64);

		GUILayout.Label(encouragingText, GUILayout.Width(mediumWindowRect.width - 20f));

		GUILayout.Space(10);
		GUILayout.Label(termsText);

		GUILayout.Space(20);
		//Buttons Accept | Decline
		GUILayout.BeginHorizontal();

//		GUI.backgroundColor = Color.green;
		bool acceptBtn = GUILayout.Button("Accept", GUILayout.Width(mediumWindowRect.width/2f - 10));

		GUI.color = Color.red;
		bool declineBtn = GUILayout.Button("Decline", GUILayout.Width(mediumWindowRect.width/2f - 10));

		GUI.color = guiColor;

		if (acceptBtn || declineBtn)
		{
			if (acceptBtn)
			{
				window = Window.ServerGreet;
			}
			else
			{
				window = Window.SoloPlayer;
				ShowMainMenu();
			}
		}

		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
//		GUI.DragWindow();
	}

	void WindowProcessingLoginRequest(int windowID)
	{
		previousWindow = Window.None;
		
		if (!processingRequest)
		{
			processingRequest = true;
			ResetProcessingRequestTimer();
			StartCoroutine(RequestLogin());
		}
		else
		{
			processingRequestTimer -= Time.deltaTime; //To fix a firefux bug
			if (processingRequestTimer + 6f < Time.realtimeSinceStartup || processingRequestTimer < 0)
			{
				FailedRequest();
				return;
			}
		}

		GUILayout.BeginVertical();

		GUILayout.Space(50);

		string processingText = "Working...";
		GUI.backgroundColor = Color.white;

		GUILayout.Space(20);
		
		GUILayout.Label(processingText);
		
		GUILayout.Space(20);
		GUILayout.EndVertical();
	}

	void WindowProcessingGetConfig(int windowID)
	{
		if (!processingRequest)
		{
			processingRequest = true;
			StartCoroutine(RequestCommunityConfig());
		}
		GUILayout.BeginVertical();

		GUILayout.Space(50);

		string processingText = "Loading...";
				
		GUILayout.Space(20);

		GUILayout.Label(processingText);
		
		GUILayout.Space(20);
		GUILayout.EndVertical();
	}
//	private int feedbackSendCount = 0;

	void WindowFeedback(int windowID)
	{
		previousWindow = window;

		GUILayout.BeginVertical();

		GUILayout.Space(32);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.color = Color.white;
		feedback.text = GUILayout.TextField(feedback.text, 200, GUILayout.Width(300));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

//		GUILayout.BeginHorizontal();
//		GUILayout.Label("Please rate your last played round: ");
//		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		bool dislike = feedback.rating == -1;
		GUI.color = dislike? Color.white : new Color(9f, 0.4f, 0.4f); //Kinda red
		dislike = GUILayout.Toggle(dislike, "Dislike");
		if (dislike)
		{
			feedback.rating = - 1;
		}

		bool neutral = feedback.rating == 0;
		GUI.color = neutral? Color.white : Color.yellow;
		neutral = GUILayout.Toggle(neutral, "Meh");
		if (neutral)
		{
			feedback.rating = 0;
		}

		bool like = feedback.rating == 1;
		GUI.color = like ? Color.white : Color.green;
		like = GUILayout.Toggle(like, "Like!");
		if (like)
		{
			feedback.rating = 1;
		}

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(largeVerticalSpacing);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		bool didChooseRating = feedback.rating != Constants.UNDEFINED;
		string submitBtnText = didChooseRating? "Submit feedback" : "Choose a rating";
		Color btnColor = didChooseRating && !feedback.sent? Color.green : Constants.LIGHT_GRAY_COLOR;

		if (feedback.sent)
		{
			window = Window.GameOver;
			submitBtnText = "Sent, Thanks!";
		}
		GUI.color = btnColor;

		bool submit = GUILayout.Button(submitBtnText, GUILayout.Width(buttonSize.x));
		if (submit && !processingRequest && feedback.rating != Constants.UNDEFINED && !feedback.sent)
		{
			processingRequest = true;
			StartCoroutine(RequestSendFeedback());
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

//		GUILayout.Space(-20);
//
//		GUI.color = Color.white;
//		GUILayout.BeginHorizontal();
//		GUILayout.FlexibleSpace();
//		bool replayTest = GUILayout.Button("Play it again", GUILayout.Width(200));
//		if (replayTest)
//		{
//			StartGame();
//		}
//		GUILayout.FlexibleSpace();
//		GUILayout.EndHorizontal();
//
//		GUILayout.Space(-20);
//
//		GUILayout.BeginHorizontal();
//		GUILayout.FlexibleSpace();
//		bool otherTest = GUILayout.Button("Play other", GUILayout.Width(200));
//		if (otherTest)
//		{
//			LoadPlaytest();
//		}
//		GUILayout.FlexibleSpace();
//		GUILayout.EndHorizontal();
//
		GUILayout.Space(largeVerticalSpacing);
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.color = Color.white;
		bool mainMenu = GUILayout.Button("Main Menu", GUILayout.Width(smallButtonSize.x));
		if (mainMenu)
		{
			ShowMainMenu();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.EndVertical();
	}

	enum ConfigSubmitButtonState
	{
		Enabled,
		Disconnected,
		Playtest,
		Sending,
		Sent
	}
	private ConfigSubmitButtonState submitButtonState;
	
	void WindowConfiguration(int windowID)
	{
		previousWindow = window;

		GameConfiguration lastConfig = GameManager.Instance.gameConfig;

		GUILayout.BeginVertical();
		GUILayout.Space(32);

		GUILayout.Label("Starting position: " + PlayerConfig.initialPlayerPosition);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("0");

		//		GUI.color = Color.yellow;
		PlayerConfig.initialPlayerPosition = (int)GUILayout.HorizontalSlider(PlayerConfig.initialPlayerPosition, 0f, 100f, GUILayout.Width(200));

		GUILayout.Label("100");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		

		GUILayout.Label("Speed: " + PlayerConfig.initialGameSpeed);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("0");

//		GUI.color = Color.white;
		PlayerConfig.initialGameSpeed = (int)GUILayout.HorizontalSlider(PlayerConfig.initialGameSpeed, 0, 100, GUILayout.Width(200));

		GUILayout.Label("100");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Label("Aceleration: " + PlayerConfig.speedUpPerTick);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("0");
		
//		GUI.color = Color.blue;
		PlayerConfig.speedUpPerTick = (int)GUILayout.HorizontalSlider(PlayerConfig.speedUpPerTick, 0, 100, GUILayout.Width(200));

		GUILayout.Label("100");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(-10);

		GUI.color = Color.white;
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		bool replayTest = GUILayout.Button("Play this config.!", GUILayout.Width(buttonSize.x));
		if (replayTest)
		{
			StartSoloPlay();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
					
		string labelText = "";		
		string submitBtnText = "";
//		bool shouldTest = false;

		if (!isEnabled)
		{

			labelText = "Not connected, submission disabled";
			submitBtnText = "Disconnected";
			submitButtonState = ConfigSubmitButtonState.Disconnected;
		}
		else
		{
			if (player.playtestCount < minPlaytestsToSendConfig)
			{
				labelText = "Please rate " + (minPlaytestsToSendConfig - player.playtestCount) + " configs. to enable submission";
				submitBtnText = "Test and rate!";
				submitButtonState = ConfigSubmitButtonState.Playtest;
			}
			else if (customizingConfig.sent) 
			{
				submitBtnText = "Sent, Thanks!";
				submitButtonState = ConfigSubmitButtonState.Sent;
			}
			else if (processingRequest)
			{
				submitBtnText = "Sending...";
				submitButtonState = ConfigSubmitButtonState.Sending;
			}
			else if (player.playtestCount >= minPlaytestsToSendConfig)
			{
				labelText = "";
				submitBtnText = "Submit!";
				submitButtonState = ConfigSubmitButtonState.Enabled;
			}

		}

		GUILayout.Space(-15);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		GUILayout.Label(labelText);
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(-10);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (submitButtonState == ConfigSubmitButtonState.Enabled || submitButtonState == ConfigSubmitButtonState.Playtest)
			GUI.color = Color.green;
		else
			GUI.color = Constants.LIGHT_GRAY_COLOR;

		bool submit = GUILayout.Button(submitBtnText, GUILayout.Width(buttonSize.x));

		if (submit)
		{
			if (submitButtonState == ConfigSubmitButtonState.Enabled)
			{
				processingRequest = true;

				customizingConfig.initialPlayerPosition = PlayerConfig.initialPlayerPosition;
				customizingConfig.initialGameSpeed = PlayerConfig.initialGameSpeed;
				customizingConfig.speedUpPerTick = PlayerConfig.speedUpPerTick;

				StartCoroutine(RequestSendConfig());
			}
			else if (submitButtonState == ConfigSubmitButtonState.Sent)
			{
				//Nothing to do here
			}
			else if (submitButtonState == ConfigSubmitButtonState.Disconnected)
			{
				//Do nothing
			}
			else if (submitButtonState == ConfigSubmitButtonState.Playtest)
			{
				LoadPlaytest();
			}
		}

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(verticalSpacing);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.color = Color.white;
		bool mainMenu = GUILayout.Button("Main Menu", GUILayout.Width(smallButtonSize.x));
		if (mainMenu)
		{
			ShowMainMenu();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.EndVertical();
	}

	void WindowPause(int windowID)
	{
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		
		bool shouldHighlightCustomization = player.playtestCount >= minPlaytestsToSendConfig;
		if (shouldHighlightCustomization)
		{
			GUI.color = Color.green;
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			bool customize = GUILayout.Button("Play and customize", GUILayout.Width(buttonSize.x));
			if (customize)
			{
				StartSoloPlay();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.Space(largeVerticalSpacing);
		}

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.color = !shouldHighlightCustomization ? Color.green : Color.white;

		bool otherTest = GUILayout.Button("Play other config.", GUILayout.Width(buttonSize.x));
		if (otherTest)
		{
			LoadPlaytest();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(largeVerticalSpacing);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.color = Color.white;

		bool replayTest = GUILayout.Button("Play this config. again", GUILayout.Width(buttonSize.x));
		if (replayTest)
		{
			StartGame();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(largeVerticalSpacing);
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		bool mainMenu = GUILayout.Button("Main Menu", GUILayout.Width(smallButtonSize.x));
		if (mainMenu)
		{
			ShowMainMenu();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
	}

	void WindowFailedRequest(int windowID)
	{
		GUILayout.BeginVertical();

		GUILayout.Space(50);

		GUI.color = Color.yellow;

		string processingText = "Sorry!\nConnection failed.";
		GUI.backgroundColor = Color.white;
		
		GUILayout.Space(20);
		
		GUILayout.Label(processingText);
		
		GUILayout.Space(20);
		GUILayout.EndVertical();

		GUI.color = guiColor;
	}

	void WindowSuccessfulRequest(int windowID)
	{
		GUILayout.BeginVertical();
		GUILayout.Space(50);

		GUI.color = Color.green;
		
		string processingText = "Alright!\nSuccesfully connected!";

		GUILayout.Space(20);
		
		GUILayout.Label(processingText);
		
		GUILayout.Space(verticalSpacing);
		GUILayout.EndVertical();
		
		GUI.color = guiColor;
	}

	#endregion Windows

	#region Requests

	IEnumerator RequestLogin()
	{
		window = Window.ProcessingRequest;

		if (player == null)
		{
			player = new Player();
			player.deviceID = SystemInfo.deviceUniqueIdentifier;
		}

		Hashtable header = MakeHeader();

		byte[] postData = PlayerJSON();

		WWW request = new WWW(Constants.BASE_URL + "/players", postData, header);
		print (request.url);

		yield return request;

		//DO NOT ATTEMPT TO ACCESS REQUEST.TEXT WITHOUT CHECKING FOR ERROR FIRST!
		//CAUSES CRAZY BEHAVIOUR!
		if (request.error == null)
		{
			JSONObject jsonResponse = new JSONObject(request.text);

			player = MakePlayer(jsonResponse);

			if (player != null)
			{
				window = Window.SuccessfulRequest;
				isEnabled = true;
			}
			else
			{
				print ("response with error: " + request.text);
				FailedRequest();
				isEnabled = false;
			}
		}
		else
		{
			print ("error: " + request.error );
			FailedRequest();
			isEnabled = false;
		}

		ResetWindowTimer();

		processingRequest = false;
	}

	IEnumerator RequestCommunityConfig()
	{

		WWW request = new WWW(Constants.BASE_URL + "/games/" + player.game_id + "/playtest_conf.json");
		print ("request url: " + request.url);
		
		yield return request;
		
		//DO NOT ATTEMPT TO ACCESS REQUEST.TEXT WITHOUT CHECKING FOR ERROR FIRST!
		//CAUSES CRAZY BEHAVIOUR!
		if (request.error == null)
		{
			JSONObject responseJSON = new JSONObject(request.text);

			PlaytestConfig = MakeConfig(responseJSON);
			
			if (PlaytestConfig != null)
			{
				print ("success: " + request.text);
				state = State.Playtest;

				StartGame();
			}
			else
			{
				print ("response with error: " + request.text);
				FailedRequest();
				state = State.Playtest;
			}
		}
		else
		{
			print ("error: " + request.error );
			FailedRequest();
			state = State.Playtest;
		}
		
		ResetWindowTimer();
		
		processingRequest = false;
	}

	IEnumerator RequestSendFeedback()
	{
//		state = State.Feedback;
		processingRequest = true;

		Hashtable header = MakeHeader();
		
		byte[] postData = FeedbackJSON();
		
		WWW request = new WWW(Constants.BASE_URL + "/feedbacks", postData, header);
		
		yield return request;
		
		//DO NOT ATTEMPT TO ACCESS REQUEST.TEXT WITHOUT CHECKING FOR ERROR FIRST!
		//CAUSES CRAZY BEHAVIOUR!
		if (request.error == null)
		{
			print ("response: " + request.text);

			JSONObject responseJSON = new JSONObject(request.text);
			bool success = false;

			if (responseJSON != null)
			{
				try
				{
					success = responseJSON["success"].b;
				}
				catch 
				{
					success = false;
				}
			}
			//Feedback sent
			if (success)
			{
				feedback.sent = true;
				player.playtestCount ++;
				state = State.Playtest;
			}
			else
			{
				FailedRequest();
				feedback.sent = false;
				state = State.Playtest;
			}
		}
		else
		{
			print ("error: " + request.error );
			feedback.sent = false;
			FailedRequest();
			state = State.Playtest;
		}
		
		ResetWindowTimer();
		
		processingRequest = false;
	}

	IEnumerator RequestSendConfig()
	{
		//		state = State.Feedback;
		processingRequest = true;
		
		Hashtable header = MakeHeader();
		
		byte[] postData = ConfigurationJSON();
		
		WWW request = new WWW(Constants.BASE_URL + "/confs", postData, header);
		
		yield return request;
		
		//DO NOT ATTEMPT TO ACCESS REQUEST.TEXT WITHOUT CHECKING FOR ERROR FIRST!
		//CAUSES CRAZY BEHAVIOUR!
		if (request.error == null)
		{
			print ("response: " + request.text);
			
			JSONObject responseJSON = new JSONObject(request.text);
			bool success = false;
			
			if (responseJSON != null)
			{
				try
				{
					success = responseJSON["success"].b;
				}
				catch 
				{
					success = false;
				}
			}
			// Sent config
			if (success)
			{
				customizingConfig.sent = true;
				player.ResetPlaytestCount();
			}
			else
			{
				FailedRequest();
				customizingConfig.sent = false;
			}
		}
		else
		{
			print ("error: " + request.error );
			feedback.sent = false;
			FailedRequest();
			state = State.Playtest;
		}
		
		ResetWindowTimer();
		
		processingRequest = false;
	}

	void FailedRequest()
	{
		processingRequest = false;
		window = Window.FailedRequest;
		ResetWindowTimer();
	}

	byte[] PlayerJSON()
	{
		JSONObject jsonPost = new JSONObject(JSONObject.Type.OBJECT);
		jsonPost.AddField("custom", player.deviceID);
		jsonPost.AddField("name", player.name);
		jsonPost.AddField("game_name", Constants.GAME_NAME);
		string jsonString = jsonPost.ToString();

		print (jsonString);
		System.Text.Encoding encoding = new System.Text.UTF8Encoding();  

		return encoding.GetBytes(jsonString);
	}

	byte[] FeedbackJSON()
	{
		JSONObject jsonPost = new JSONObject(JSONObject.Type.OBJECT);
		jsonPost.AddField("rating", feedback.rating);
		jsonPost.AddField("text", feedback.text.Equals(PlaceholderFeedback())? null : feedback.text);
		jsonPost.AddField("player_id", player.identifier);
		jsonPost.AddField("game_id", player.game_id);
		jsonPost.AddField("conf_id", PlaytestConfig.identifier);
		string jsonString = jsonPost.ToString();
		print (jsonString);

		System.Text.Encoding encoding = new System.Text.UTF8Encoding();

		return encoding.GetBytes(jsonString);
	}

	byte[] ConfigurationJSON()
	{
		JSONObject jsonPost = new JSONObject(JSONObject.Type.OBJECT);

		JSONObject conf = new JSONObject(JSONObject.Type.OBJECT);
		conf.AddField("custom", "Player Configuration");
		conf.AddField("player_id", player.identifier);
		conf.AddField("game_id", player.game_id);

		jsonPost.AddField("conf", conf);

		JSONObject paramsList = new JSONObject(JSONObject.Type.ARRAY);

		JSONObject param1 = new JSONObject(JSONObject.Type.OBJECT);
		param1.AddField("name", "initialPlayerPosition");
		param1.AddField("value", customizingConfig.initialPlayerPosition.ToString());
	
		JSONObject param2 = new JSONObject(JSONObject.Type.OBJECT);
		param2.AddField("name", "initialGameSpeed");
		param2.AddField("value", customizingConfig.initialGameSpeed.ToString());

		JSONObject param3 = new JSONObject(JSONObject.Type.OBJECT);
		param3.AddField("name", "speedUpPerTick");
		param3.AddField("value", customizingConfig.speedUpPerTick.ToString());

		paramsList.Add(param1);
		paramsList.Add(param2);
		paramsList.Add(param3);

		jsonPost.AddField("params", paramsList);

		string jsonString = jsonPost.ToString();
		print (jsonString);
		
		System.Text.Encoding encoding = new System.Text.UTF8Encoding();
		
		return encoding.GetBytes(jsonString);
	}

	Hashtable MakeHeader()
	{
		Hashtable postHeader = new Hashtable();  
		postHeader.Add("Content-type", "application/json"); 
		postHeader.Add("Accept", "application/json");
		
		return postHeader;
	}

	Player MakePlayer(JSONObject responseJSON)
	{
		if (responseJSON == null || !responseJSON["success"].b)
			return null;
				

		player = new Player();

		try
		{
			player.identifier = (int)responseJSON["id"].n;
			player.game_id = (int)responseJSON["game_id"].n;
			player.name = responseJSON["name"].str;
			player.deviceID = responseJSON["custom"].str;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	
		return player;
	}

	GameConfiguration MakeConfig(JSONObject responseJSON)
	{
		if (responseJSON == null || !responseJSON["success"].b)
			return null;

		JSONObject paramsJsonObj = responseJSON["params"];
		List<JSONObject> paramsList;

		if (paramsJsonObj != null)
			paramsList = responseJSON["params"].list;
		else
			return null;

		Dictionary<string, int> configParams = new Dictionary<string, int>();

		if (paramsList != null)
		{
			foreach (JSONObject jObj in paramsList)
			{
				string pName = jObj["name"].str;
				int pValue = (int)jObj["value"].n;
				configParams.Add(pName, pValue);
			}
			GameConfiguration config = new GameConfiguration();

			try
			{
				config.initialGameSpeed = configParams["initialGameSpeed"];
				config.speedUpPerTick = configParams["speedUpPerTick"];
				config.initialPlayerPosition = configParams["initialPlayerPosition"];
				config.identifier = (int)responseJSON["id"].n;
				config.isPlaytest = true;
			}
			catch (KeyNotFoundException)
			{
				return null;
			}
		
			return config;
		}
		else
		{
			return null;
		}
	}

	#endregion Requests

	#region Interface

	public void ShowTerms()
	{
		window = Window.AcceptTerms;
		showGUI = true;
		MainMenu.Instance.enabled = false;
	}

	public void LoadPlaytest()
	{
		PlaytestConfig = null;
		window = Window.Playtest;
		state = State.Playtest;
		BecomeActive(true);
	}
	
	public void StartSoloPlay()
	{
		state = State.SoloPlayer;
		StartGame();
	}
	
	public void ShowFeedbackForm()
	{
		ResetFeedbackForm();
		window = Window.Feedback;
//		state = State.Feedback;
		BecomeActive(true);
	}

	public void ShowConfigurationForm(bool shouldShow)
	{
		if (shouldShow && window == Window.Configuration)
			return;

		if (shouldShow)
		{
			ResetConfigurationForm();
			window = Window.Configuration;
			BecomeActive(true);
		}
		else
		{
			BecomeActive(false);
		}
	}

	public void ShowPauseWindow(bool show)
	{
		if (!show && window == Window.Pause)
		{
			BecomeActive(false);
			return;
		}
		else if (show && (window == Window.Configuration || window == Window.Pause))
			return;

		if (state == State.SoloPlayer)
		{
			ShowConfigurationForm(show);
		}
		else if (state == State.Playtest)
		{
			window = Window.Pause;
			BecomeActive(true);
		}
	}

	#endregion Interface


	#region Utils
	void ResetWindowTimer()
	{
		windowTimer = Time.realtimeSinceStartup + 1.5f;
	}

	void ResetProcessingRequestTimer()
	{
		processingRequestTimer = Time.realtimeSinceStartup + 6f;
	}

	void ResetFeedbackForm()
	{
		feedback = new Feedback();
		feedback.text = PlaceholderFeedback();
		feedback.rating = Constants.UNDEFINED;
	}

	void ResetConfigurationForm()
	{
		customizingConfig = new GameConfiguration();
		customizingConfig.initialPlayerPosition = PlayerConfig.initialPlayerPosition;
		customizingConfig.initialGameSpeed = PlayerConfig.initialGameSpeed;
		customizingConfig.speedUpPerTick = PlayerConfig.speedUpPerTick;
		customizingConfig.sent = false;
	}

	string PlaceholderFeedback()
	{
		return "Type something... (optional)";
	}

	void BecomeActive(bool activate)
	{
		MainMenu.Instance.enabled = !activate;
		showGUI = activate;

		if (!activate)
			window = Window.None;
	}

	void ShowMainMenu()
	{
		BecomeActive(false);

		string mainMenuLevelName = "Initial";
		if (!Application.loadedLevelName.Equals(mainMenuLevelName))
		{
			Application.LoadLevel(mainMenuLevelName);
		}
	}

	void StartGame()
	{
		BecomeActive(false);
		Application.LoadLevel("Game");
	}

	class Player
	{
		public string name;
		public int identifier;
		public string deviceID;
		public int game_id;
		public int playtestCount = 0;

		public Player()
		{

		}

		public Player(string name, int identifier, string deviceID, int game_id, int playtestCount)
		{
			this.name = name;
			this.identifier = identifier;
			this.deviceID = deviceID;
			this.game_id = game_id;
			this.playtestCount = playtestCount;
		}

		public void ResetPlaytestCount()
		{
			playtestCount = 0;
		}
	}

	class Feedback
	{
		public int identifier;
		public int rating;
		public string text;
		public bool sent;
	}

	#endregion Utils
}

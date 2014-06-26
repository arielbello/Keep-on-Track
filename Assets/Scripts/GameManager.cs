using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager Instance;

	public GUIText scoreText;
	public GUIText restartText;

	private GameObject level;
	private GameObject player;
	private MrTeeAI mrTee;
	private float score;
	public bool isGameOver = false;
	public bool isPaused = false;

	//Level Control
	[HideInInspector]
	public GameConfiguration gameConfig;
	private float speedUpTimer;
	private float gameSpeed;
	private float acceleration;
	private Vector2 loseWorldPos;

	//GamEXP control
	private float gameOverTimer = 2f;
	private bool isGamEXPGUIActive;

	void Awake ()
	{
		QualitySettings.vSyncCount = 1;
		Instance = this;
	}

	void Start () 
	{
		Time.timeScale = 1;
	
		player = PlayerController.Instance.gameObject;
		mrTee = MrTeeAI.Instance;
		level = GameObject.FindWithTag("Level");
		ResetSpeedUpTimer();
		isGamEXPGUIActive = false;
		Vector2 bottomLeftWorldPos = Utils.BottomLeftWorldPos();
		loseWorldPos = new Vector2 (bottomLeftWorldPos.x + PlayerController.Instance.GetSize().x/2, bottomLeftWorldPos.y);

		if (GamEXP.state == GamEXP.State.Playtest && GamEXP.PlaytestConfig != null)
			SetGameConfiguration(GamEXP.PlaytestConfig);
		else if (GamEXP.state == GamEXP.State.SoloPlayer && GamEXP.PlayerConfig != null)
			SetGameConfiguration(GamEXP.PlayerConfig);
		else
			SetGameConfiguration(new GameConfiguration());
	}

	public void SetGameConfiguration(GameConfiguration gameConfig)
	{
		this.gameConfig = gameConfig;

		PlayerController playerController = player.GetComponent<PlayerController>();
		float playerPosX = Camera.main.ViewportToWorldPoint(new Vector3(gameConfig.initialPlayerPosition/100f, 0)).x;
		playerPosX = Mathf.Clamp(playerPosX, Utils.BottomLeftWorldPos().x + 1f, Utils.TopRightWorldPos().x - playerController.GetSize().x);
		player.transform.position = new Vector3(playerPosX, 0, player.transform.position.z);

		gameSpeed = (gameConfig.initialGameSpeed)/10f + Constants.OFF_TRACK_SLOW + 1f;

		acceleration = gameConfig.speedUpPerTick + 1f;

		SetGlobalSpeed(gameSpeed);
	}

	void Update () 
	{
		//Score
		score += gameSpeed * Time.deltaTime;
		scoreText.text = "Score: " + score.ToString("0.00") + "m";

		//Speed UP!
		speedUpTimer -= Time.deltaTime;
		if (speedUpTimer <= 0)
		{
			ResetSpeedUpTimer();
			gameSpeed += (acceleration * 2)/100f;

			SetGlobalSpeed(gameSpeed);
		}

		//Pause
		if (Input.GetKeyDown(KeyCode.P))
		{
			TogglePause();
		}
		//Game Over
		if (player.transform.position.x <= loseWorldPos.x && !isGameOver)
		{
			GameOver();
		}
//		if (MrTeeAI.Instance.catchedPlayer && !isGameOver)
//		{
//			GameOver();
//		}
		if (isGameOver && gameOverTimer < Time.realtimeSinceStartup && gameConfig.isPlaytest && !isGamEXPGUIActive)
		{
			isGamEXPGUIActive = true;
			GamEXP.Instance.ShowFeedbackForm();
		}
		if (isGameOver && gameOverTimer < Time.realtimeSinceStartup && !gameConfig.isPlaytest && !isGamEXPGUIActive)
		{
			ShowConfigurationForm(true);
		}
		if (isGameOver && Input.GetKeyDown(KeyCode.R))
		{
			RestartLevel();
		}
		if (isGameOver && Input.GetKeyDown(KeyCode.Escape))
		{
			Application.LoadLevel("Initial");	
		}
	}

	void Pause(bool shouldPause)
	{
		if (shouldPause)
			Time.timeScale = 0;
		else if (!isGameOver)
			Time.timeScale = 1;

		isPaused = shouldPause;
	}

	public void TogglePause()
	{
		if (Time.timeScale == 0)
		{
			Pause(false);
			GamEXP.Instance.ShowPauseWindow(false);
		}
		else
		{
			Pause(true);
			GamEXP.Instance.ShowPauseWindow(true);
		}
	}

	void ShowConfigurationForm(bool shouldShow)
	{
		if (gameConfig.isPlaytest)
			return;

		isGamEXPGUIActive = shouldShow;
		GamEXP.Instance.ShowConfigurationForm(shouldShow);
	}

	void GameOver()
	{
		ResetGameOverTimer();
		isGameOver = true;
		Pause(true);
		GameOverText.Instance.ShowGameOverText(true);
//		GUIText gameOverOutline = gameOverText.gameObject.GetComponentInChildren<GUIText>();
//		gameOverOutline.enabled = true;
		restartText.enabled = true;
	}

	public void RestartLevel()
	{
		isGameOver = false;
		GameOverText.Instance.ShowGameOverText(false);
		//		GUIText gameOverOutline = gameOverText.gameObject.GetComponentInChildren<GUIText>();
//		gameOverOutline.enabled = false;
		restartText.enabled = false;

		if (gameConfig.isPlaytest)
			GamEXP.Instance.ShowFeedbackForm();
		else
		{
			Pause(false);
			Application.LoadLevel("Game");
		}
	}

	void SetGlobalSpeed(float speed)
	{
		Mover levelMover = level.GetComponent<Mover>();
		levelMover.xSpeed = -gameSpeed;
		PlayerController playerController = player.GetComponent<PlayerController>();
		playerController.ySpeed = gameSpeed;
//		mrTee.ySpeed = gameSpeed;
	}

	void ResetSpeedUpTimer()
	{
		speedUpTimer = Constants.SPEED_UP_TIMER;
	}

	void ResetGameOverTimer()
	{
		gameOverTimer = Time.realtimeSinceStartup + 2f;
	}
}

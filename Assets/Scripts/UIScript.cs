using UnityEngine;
using System.Collections;

public class UIScript : MonoBehaviour {

	public GUIText levelStartText;
	public GUIText tutorialText;

	private bool showingStartLevelText;
	private float startTextTimer;

	void Start () 
	{
		ShowLevelStartText(true);
	}

	void Update () 
	{
		if (showingStartLevelText)
			startTextTimer -= Time.deltaTime;

		if (showingStartLevelText && startTextTimer <= 0)
		{
			ShowLevelStartText(false);
		}
		else if (showingStartLevelText && startTextTimer > 0)
		{
			levelStartText.text = "Get on Track! " + startTextTimer.ToString("0.0");
		}
	}

	void ShowLevelStartText(bool shouldShow)
	{
		showingStartLevelText = shouldShow;
		levelStartText.enabled = shouldShow;
		tutorialText.enabled = shouldShow;
		ResetStartTextTimer();
	}

	void ResetStartTextTimer()
	{
		startTextTimer = 3f;
	}
}

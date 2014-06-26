using UnityEngine;
using System.Collections;

public class GameOverText : MonoBehaviour {

	public static GameOverText Instance;
	public GUIText gameOverText;
	public GUIText gameOverTextOutline;

	void Awake ()
	{
		Instance = this;
	}

	void Start () {
	
	}

	void Update () {
	
	}

	public void ShowGameOverText(bool show)
	{
		gameOverText.enabled = show;
		gameOverTextOutline.enabled = show;
	}
}

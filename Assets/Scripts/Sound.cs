using UnityEngine;
using System.Collections;

public class Sound : MonoBehaviour 
{
	public static Sound Instance;

	public AudioSource music;
	public AudioSource moveFX;

	private bool _mute = false;
	public bool mute { 
		get { return _mute; }
		set {
			_mute = value;
			AudioListener.volume = _mute ? 0f : 1f;
		}
	}

	private float speedUpTimer;

	void Awake()
	{
		Instance = this;
	}

	void Start () 
	{
		ResetSpeedUpTimer();
	}
	
	void Update () 
	{
//		speedUpTimer -= Time.deltaTime;
//
//		if (speedUpTimer <= 0)
//		{
//			ResetSpeedUpTimer();
//			music.pitch += 0.05f;
//		}
	
	}

	#region Interface

	public void PlayMoveSound()
	{
		moveFX.Play(); 
	}

	#endregion Interface

	void ResetSpeedUpTimer()
	{
		speedUpTimer = Constants.SPEED_UP_TIMER;
	}
}

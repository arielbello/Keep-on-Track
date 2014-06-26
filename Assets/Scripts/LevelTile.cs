using UnityEngine;
using System.Collections;

public class LevelTile : MonoBehaviour 
{
	public enum Type
	{
		BottomUp,
		BottomUpCon,
		BottomDown,
		BottomDownCon,
		TopUp,
		TopUpCon,
		TopDown,
		TopDownCon,
		Fill1,
		Fill2,
		Track
	}

	public Type type;
	[HideInInspector]
	public float offTrackSlow;
	public bool canDrawOnTop = true;

	
	void Start() 
	{
		offTrackSlow = Constants.OFF_TRACK_SLOW;
	}

	void Update() 
	{

	}

	void OnTriggerEnter(Collider other)
	{
		PlayerController player = other.GetComponent<PlayerController>();
		if (player != null)
		{
			print ("player entered trigger");
		}
		else 
		{
			print ("smth else entered the trigger");
		}
	}
}

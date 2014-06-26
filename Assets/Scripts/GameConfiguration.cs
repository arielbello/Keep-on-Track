using UnityEngine;
using System.Collections;

//Customizable game parameters used for the GamEXP
public class GameConfiguration
{
	//between 0-100, the GameManager makes sense of them
	public int initialGameSpeed = 40;
	public int initialPlayerPosition = 70;
	public int speedUpPerTick = 50; 
	public bool isPlaytest = false;
	public bool sent = false;
	//GamEXP server state
	public int identifier = Constants.UNDEFINED;

	public GameConfiguration()
	{
	}

	public GameConfiguration(int initialGameSpeed, int initialPlayerPos, int speedUpPerTimer, bool isPlaytest, int identifier)
	{
		this.initialGameSpeed = initialGameSpeed;
		this.initialPlayerPosition = initialPlayerPos;
		this.speedUpPerTick = speedUpPerTimer;
		this.isPlaytest = isPlaytest;
		this.identifier = identifier;
	}

	public bool HasSameValues(GameConfiguration other)
	{
		return	this.initialGameSpeed == other.initialGameSpeed &&
				this.initialPlayerPosition == other.initialPlayerPosition &&
				this.speedUpPerTick == other.speedUpPerTick;
			   
	}
}

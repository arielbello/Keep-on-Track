using UnityEngine;
using System.Collections;

public static class Constants
{
	//GamEXP server settings
	public static string BASE_URL_HOME = "http://127.0.0.1:3000";
	public static string BASE_URL = "Your server base URL here!";
	public static int MIN_PLAYTEST_COUNT = 10;

	public static string GAME_NAME = "Your game name here!";

	//Generation constants
	public static int MAX_TREES = 12;
	public static int MAX_CABINS = 8;
	public static int MAX_ANIMALS = 5;

	//AI
	public static float MIN_DISTANCE_TO_CHASE = 4f;
		
//	public static int MAX_VERTICAL_TILES = 8;

	//Default configuration
	public static int GAME_SPEED = 6;
	public static float OFF_TRACK_SLOW = 3;
	public static float SPEED_UP_TIMER = 3f;	
	public static int SPEED_UP_PER_TIME = 50;
	public static int INITIAL_PLAYER_POS = 30;

	//Track
//	public static float TRACK_HEIGHT = (TILE_HEIGHT * 2)/10f; //Two tiles high
	public static int MIN_TRAIL_LENGTH = 2;
//	public static int MAX_TRAIL_LENGTH = Mathf.FloorToInt(TRACK_HEIGHT - 2);
	public static int TRAIL_OFFSET = 1;

	//UTILITY
	public static Color LIGHT_GRAY_COLOR = new Color(0.8f, 0.8f, 0.8f);
	public static int UNDEFINED = int.MinValue;
}

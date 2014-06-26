using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MrTeeAI : MonoBehaviour 
{
	public static MrTeeAI Instance;

	[HideInInspector]
	public bool catchedPlayer;

	public float ySpeed;
	public float xSpeed;

	private float BOTTOM_BOUNDARY;
	private float TOP_BOUNDARY;
	private float LEFT_BOUNDARY;

	//Pathfinding :P
	private CheckPoint checkPoint;
	private PlayerController player;
	private bool isChasingPlayer;
	private Vector2 targetPosition;
	private float distanceThreshold;
	public float bonusYChasingSpeed = Constants.GAME_SPEED;

	private SpriteRenderer spriteRenderer;

	#region Monobehaviour

	void Awake()
	{
		Instance = this;
	}

	void Start () 
	{
		catchedPlayer = false;
		bonusYChasingSpeed = 0f;
		player = PlayerController.Instance;
//		ySpeed = Parameters.GAME_SPEED;
		xSpeed = 0;
		spriteRenderer = (SpriteRenderer)this.renderer;
		BOTTOM_BOUNDARY = Utils.BottomLeftWorldPos().y;
		TOP_BOUNDARY = Utils.TopRightWorldPos().y;
		LEFT_BOUNDARY = Utils.BottomLeftWorldPos().x;
		targetPosition = Utils.BottomLeftWorldPos();
	}

	void Update () 
	{
		if (checkPoint == null)
			checkPoint = new CheckPoint(LevelGenerator.Instance.tilesGrid[0][0], Utils.BottomLeftWorldPos());
		else if (checkPoint.tile == null)
		{
			checkPoint.tile = LevelGenerator.Instance.tilesGrid[0][0];
		}

		float xDistanceToPlayer = Mathf.Abs(transform.position.x - player.transform.position.x);
		isChasingPlayer = xDistanceToPlayer <= Constants.MIN_DISTANCE_TO_CHASE;

		//Just Passed the target position, should set another
		if (!isChasingPlayer && transform.position.x > checkPoint.tile.transform.position.x)
		{
			List<List<LevelTile>> tilesMatrix = LevelGenerator.Instance.tilesGrid;
			Track track = LevelGenerator.Instance.track;
			int index = 0;
			
			for (int i = 0; i < tilesMatrix.Count; i++)
			{
				float xPos = tilesMatrix[i][0].transform.position.x;
				
				if (xPos > transform.position.x)
				{
					index = i;
					break;
				}
			}
			int indexInColumn = track.slices[index].index;
			LevelTile trackTile = tilesMatrix[index][indexInColumn];
			checkPoint.tile = trackTile;
			
			targetPosition = new Vector2(trackTile.transform.position.x + LevelGenerator.Instance.tileSize.x/2f,
			                             trackTile.transform.position.y + LevelGenerator.Instance.tileSize.y);
			
			//Need to change pathfinding in corners, going deep in the path
			if (track.slices[index].direction != track.slices[index+1].direction)
			{
				checkPoint.tile = tilesMatrix[index+1][indexInColumn];
				if (track.slices[index].direction == Track.Direction.Up)
				{
					targetPosition.x += LevelGenerator.Instance.tileSize.x/2f;
					targetPosition.y += LevelGenerator.Instance.tileSize.y/2f;
				}
				else
				{
					targetPosition.x += LevelGenerator.Instance.tileSize.x/2f;
					targetPosition.y -= LevelGenerator.Instance.tileSize.y/2f;
				}
			}
		}
		//Player is close and Mr. Tee should go after him, ignoring the track
		else if (isChasingPlayer)
		{
			targetPosition = player.transform.position;
		}

		//Move Mr. Tee to the right position
		int yDirection;
		if (isChasingPlayer)
			yDirection = targetPosition.y > transform.position.y + spriteRenderer.bounds.size.y/2f ? 1 : -1;
		else
			yDirection = targetPosition.y > transform.position.y ? 1 : -1;

		float xTranslation = xSpeed * Time.deltaTime;
		float yTranslation;
		if (isChasingPlayer) yTranslation = (bonusYChasingSpeed + ySpeed) * yDirection * Time.deltaTime;
		else                 yTranslation = ySpeed * yDirection * Time.deltaTime;

		transform.Translate(xTranslation, yTranslation, 0, Space.World);

		//Keep Mr. Tee inside the screen bounds
		if (transform.position.x - spriteRenderer.bounds.size.x/2f <= LEFT_BOUNDARY)
		{
			Vector3 correctedPos = transform.position;
			correctedPos.x = LEFT_BOUNDARY + spriteRenderer.bounds.size.x/2f;
			transform.position = correctedPos;
		}
		if (transform.position.y + yTranslation  <= BOTTOM_BOUNDARY && yDirection < 0 ||
		    transform.position.y + yTranslation + spriteRenderer.bounds.size.y >= TOP_BOUNDARY && yDirection > 0)
		{
			Vector3 correctedPos = transform.position;
			if (yDirection > 0)
				correctedPos.y = TOP_BOUNDARY - spriteRenderer.bounds.size.y;
			else
				correctedPos.y = BOTTOM_BOUNDARY;
			
			transform.position = correctedPos;
		}
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		//End game condition
		if (other.gameObject == player.gameObject)
		{
			catchedPlayer = true;
		}
	}

	#endregion Monobehaviour

	#region Utils

	public Vector2 GetSize()
	{
		return spriteRenderer.bounds.size;
	}

	private class CheckPoint
	{
		public LevelTile tile;
		public Vector2 position;

		public CheckPoint(LevelTile tile, Vector2 position)
		{
			this.tile = tile;
			this.position = position;
		}
	}

	#endregion Utils
}

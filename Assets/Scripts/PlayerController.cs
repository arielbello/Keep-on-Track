using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
	public static PlayerController Instance;
	public Sprite slowHead;
	public Sprite happyHead;
	private Sprite normalHead;
	
	public float xSpeed = 0;
	public float ySpeed = Constants.GAME_SPEED;
	public bool isSlow;
	public bool isRocking;
	private float previousYDircetion;
	private float yDirection = 1f;
	private float invulnerabilityTimer;
	
	private int offTrackCollidersCount = 0;
	private float slowTimer;
	private float onTrackTimer;
	private SpriteRenderer spriteRenderer;
	private float BOTTOM_BOUNDARY;
	private float TOP_BOUNDARY;
	private float LEFT_BOUNDARY;
	
	#region Monobehaviour
	
	void Awake()
	{
		Instance = this;
	}
	
	void Start () 
	{
		previousYDircetion = yDirection;
		xSpeed = 0f;
//		ySpeed = Parameters.GAME_SPEED; //Set by Game Manager
		isSlow = false;
		offTrackCollidersCount = 0;
		invulnerabilityTimer = 3f;
		slowTimer = 0;
		ResetOnTrackTimer();
		spriteRenderer = GetComponent<SpriteRenderer>();
		normalHead = spriteRenderer.sprite;
		BOTTOM_BOUNDARY = Utils.BottomLeftWorldPos().y;
		TOP_BOUNDARY = Utils.TopRightWorldPos().y;
		LEFT_BOUNDARY = Utils.BottomLeftWorldPos().x;
	}
	
	void OnTriggerStay2D(Collider2D other)
	{
		if (invulnerabilityTimer > 0)
		{
			slowTimer = 0;
			SetSlow (false);
			return;
		}
	
		LevelTile tile = other.gameObject.GetComponent<LevelTile>();
		
		if (tile != null)
		{
			if (tile.type != LevelTile.Type.Track)
			{
				SetRocking(false);
				SetSlow(true);
			}
		}
	}
	
	void FixedUpdate()
	{
		invulnerabilityTimer -= Time.deltaTime;
		slowTimer -= Time.deltaTime;
		onTrackTimer -= Time.deltaTime;

		if (onTrackTimer <= 0)
			SetRocking(true);

		else if (slowTimer <= 0)
			SetSlow(false);
	}
	
	void Update () 
	{
		if (GameManager.Instance.isPaused)
			return;

		//Change direction
		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
		{
			yDirection = 1f;
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
		{
			yDirection = -1f;
		}
		
		//Play direction change sound
		if (previousYDircetion != yDirection)
		{
			Sound.Instance.PlayMoveSound();
		}
		previousYDircetion = yDirection; //Cache direction to know when it changes
		
		//Translate
		float xTranslation = xSpeed * Time.deltaTime;
		float yTranslation = ySpeed * yDirection * Time.deltaTime;
		
		transform.Translate(xTranslation, yTranslation, 0, Space.World);
		
		//Rotate to face the track direction
		Quaternion rotation = yDirection > 0 ? Quaternion.AngleAxis(45, Vector3.forward) : Quaternion.AngleAxis(-45, Vector3.forward);
		
		transform.rotation = rotation;
		
		//Keep the player inside the screen bounds
//		if (transform.position.x - spriteRenderer.bounds.size.x/2f <= LEFT_BOUNDARY)
//		{
//			Vector3 correctedPos = transform.position;
//			correctedPos.x = LEFT_BOUNDARY + spriteRenderer.bounds.size.x/2f;
//			transform.position = correctedPos;
//		}
		if (transform.position.y + yTranslation  - spriteRenderer.bounds.size.y/2f <= BOTTOM_BOUNDARY && yDirection < 0 ||
		    transform.position.y + yTranslation + spriteRenderer.bounds.size.y/2f >= TOP_BOUNDARY && yDirection > 0)
		{
			Vector3 correctedPos = transform.position;
			if (yDirection > 0)
				correctedPos.y = TOP_BOUNDARY - spriteRenderer.bounds.size.y/2f;
			else
				correctedPos.y = BOTTOM_BOUNDARY + spriteRenderer.bounds.size.y/2f;
			
			transform.position = correctedPos;
		}
	}
	
	public void SetSlow(bool slow)
	{
		if (!slow && slowTimer <= 0 && isSlow)
		{
			spriteRenderer.sprite = normalHead;
			xSpeed = 0;
		}
		else if (slow && isSlow)
		{
			ResetSlowTimer();
		}
		else if (slow && !isSlow)
		{
			spriteRenderer.sprite = slowHead;
			xSpeed = - Constants.OFF_TRACK_SLOW;
		}

		isSlow = slow;
	}

	void SetRocking(bool rocking)
	{
		if (!rocking && isRocking)
		{
			spriteRenderer.sprite = normalHead;
			ResetOnTrackTimer();
		}
		else if (!rocking && !isRocking)
		{
			ResetOnTrackTimer();
		}
		else if (rocking && !isRocking)
		{
			spriteRenderer.sprite = happyHead;
		}
		this.isRocking = rocking;
	}
	
	#endregion Monobehaviour

	public Vector2 GetSize()
	{
		return spriteRenderer.bounds.size;
	}

	void ResetSlowTimer()
	{
		slowTimer = 0.25f;
	}

	void ResetOnTrackTimer()
	{
		onTrackTimer = 10f;
	}
}

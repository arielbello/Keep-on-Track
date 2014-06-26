using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class LevelGenerator : MonoBehaviour 
{
	public static LevelGenerator Instance;

	public List<GameObject> tileGameObjects;

	[HideInInspector]
	//Tile matrix
	public List<List<LevelTile>> tilesGrid;

	private List<SceneObject> sceneObjects;
	private Dictionary<LevelTile.Type, GameObject> tilesDict;

	//AI Path
	public List<Vector2> trackPath;
	public List<LevelTile> tilePath;

	public Track track;

	//Generation
	private int generatorIndex;

	//Sizes and positions
	public Vector3 tileSize;
	private Vector3 bottomLeftWorldPoint;
	private Vector3 topRightWorldPoint;
	private int MAX_COLUMN_LENGTH;
	private int MAX_LINE_LENGTH;
	private int LINE_BUFFER_SIZE = 2;
	private float DESTROY_BOUNDARY;

	private bool hasPopulated = false;

	#region MonoBehaviour

	void Awake()
	{
		Instance = this;
	}

	void Start () 
	{
		LoadAttributes();
		SetupLevel();

	}
	
	void Update () 
	{
		SeekAndDestroyWhatsLeftBehind();
		RecreationDay();
	}

	#endregion MonoBehaviour

	#region Setup
	void LoadAttributes()
	{
		//Create dictionary with references to the tile prefabs
		tilesDict = new Dictionary<LevelTile.Type, GameObject>();
		
		foreach (GameObject go in tileGameObjects)
		{
			LevelTile tile = go.GetComponent<LevelTile>();
			tilesDict.Add(tile.type, go);
		}

		//References scene objects used
		sceneObjects = new List<SceneObject>();
		//AI Path
		trackPath = new List<Vector2>();
		tilePath = new List<LevelTile>();

		//Other stuff
		tilesGrid = new List<List<LevelTile>>();
		tileSize = GetTileSize();
		generatorIndex = Constants.UNDEFINED;

		//Translate the screen bounds to world points
		Camera mainCamera = Camera.main;
		bottomLeftWorldPoint = mainCamera.ScreenToWorldPoint(Vector3.zero);
		topRightWorldPoint = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.nearClipPlane));
		//Maximum visible size of the tiles grid
		MAX_COLUMN_LENGTH = Mathf.CeilToInt((topRightWorldPoint.y - bottomLeftWorldPoint.y)/tileSize.y);
		MAX_LINE_LENGTH = Mathf.CeilToInt((topRightWorldPoint.x - bottomLeftWorldPoint.x)/tileSize.x);
		//To clip the level
		DESTROY_BOUNDARY = Mathf.Ceil(bottomLeftWorldPoint.x - tileSize.x * 2);
	}

	Vector3 GetTileSize()
	{
		GameObject go = (GameObject)Instantiate(tilesDict[LevelTile.Type.Fill1]);
		Sprite tileSp = go.GetComponent<SpriteRenderer>().sprite;
		Vector3 size = new Vector3(tileSp.bounds.size.x, tileSp.bounds.size.y, tileSp.bounds.size.z);
		Destroy(go);
		return size;
	}

	void SetupLevel()
	{
		track = new Track(MAX_LINE_LENGTH + LINE_BUFFER_SIZE, MAX_COLUMN_LENGTH - GetTrackHeight());
		track.GenerateInitialTrack();

		for (int i = 0; i < track.Length; i++)
		{
			GenerateTilesLine(i);
		}

		//PoolManager is used to Populate the scenario with pooled objects
//		PoolManager.Instance.Setup();
//		PupulateScenario(0, track.Length);
	}

	#endregion Setup

	public void PrintSmth()
	{
		print("Bottom Left World Point: " + bottomLeftWorldPoint + "\n" + "Top Right: " + topRightWorldPoint);
	}

	public void GenerateSmth()
	{
		StringBuilder sb = new StringBuilder();

		int index = 0;
		foreach (TrackSlice ts in track.slices)
		{
			string dir = ts.direction == Track.Direction.Up? "Up" : "Down";
			sb.Append(index + " column: " + ts.index + " Trail Index : " + ts.currentDirectionIndex + " Direction: " + dir + "\n");
			index++;
		}

		print (sb);
	}

	#region Level Generation

	//Searching.... Seek and Destroy! (The track that is way to the left and not visible anymore)
	//Metallica + Slipknot, awesome method name
	void SeekAndDestroyWhatsLeftBehind()
	{
		float xPos;

		//Destroy tiles
		do
		{
			LevelTile leftMostTile = tilesGrid[0][0];
			xPos = leftMostTile.transform.position.x;

			if (xPos < DESTROY_BOUNDARY)
			{
				foreach (LevelTile tile in tilesGrid[0])
				{
					Destroy(tile.gameObject);
				}
				tilesGrid.RemoveAt(0);
				track.RemoveSlicesFromBeginning(1);
			}

		}while (xPos < DESTROY_BOUNDARY);

		//Destroy scene objects
		for (int i = 0; i < sceneObjects.Count; i++)
		{
			SceneObject leftMostObj = sceneObjects[i];
			xPos  = leftMostObj.transform.position.x + leftMostObj.size.x * tileSize.x;
			if (xPos < DESTROY_BOUNDARY)
			{
				//put back in the pool
				leftMostObj.isAvailable = true;
				sceneObjects.RemoveAt(i);
			}
			else
				break;
		}
	}
	void RecreationDay()
	{
		if (tilesGrid.Count < MAX_LINE_LENGTH + LINE_BUFFER_SIZE)
		{
			int fromColumn = tilesGrid.Count;
			int toColumn = fromColumn + track.AddSlicesToEnd(LINE_BUFFER_SIZE);

			for (int i = fromColumn; i < toColumn; i++)
			{
				GenerateTilesLine(i);
			}

//			PupulateScenario(fromColumn, toColumn);
		}
	}

	//Draw Trees, Cabins and Stuff
	void PupulateScenario(int fromColumn, int toColumn)
	{
//		int objsPerColumn = 2;
		SceneObject.Type lastUsedType = SceneObject.Type.None;

		for (int i = fromColumn; i < toColumn; i++)
		{
			SceneObject.Type type;
				
			for (int j = 0; j < tilesGrid[i].Count; j++)
			{
				int randPlacement = Random.Range(0, 3); //1/3 chance of not drawing anything

				if (randPlacement == 0)
					type = SceneObject.Type.None;
				else if (randPlacement == 1 && i % 3 == 0 && lastUsedType != SceneObject.Type.Cabin) //Reducing the chances of drawing a Cabin
					type = SceneObject.Type.Cabin;
				else
					type = SceneObject.Type.Tree;

				SceneObject sceneObj = PoolManager.Instance.GetPooledSceneObject(type);
			
				LevelTile tile = tilesGrid[i][j];
				//Stop if there isn't an obj available, or tile isn't available
				if (sceneObj == null || !tile.canDrawOnTop)
					continue;

				//Check if there's enough space for the prop
				bool canDrawOnTop = tile.canDrawOnTop;
				for (int x = 0; x < sceneObj.size.x; x++) //size in tiles
				{
					for (int y = 0; y < sceneObj.size.y; y++)
					{
						if (x + i < tilesGrid.Count && y + j < tilesGrid[i].Count)
							if (!tilesGrid[i+x][j+y].canDrawOnTop)
								canDrawOnTop = false;
						if (x + i >= tilesGrid.Count || y + j >= tilesGrid[i].Count)
							canDrawOnTop = false;
					}
				}
				//Then draw it!
				if (canDrawOnTop)
				{
					sceneObj.isAvailable = false;
					sceneObjects.Add(sceneObj);
					sceneObj.gameObject.transform.localPosition = tile.transform.localPosition;
					lastUsedType = sceneObj.type;

					for (int x = 0; x < sceneObj.size.x; x++) //size in tiles
					{
						for (int y = 0; y < sceneObj.size.y; y++)
						{
							if (x + i < tilesGrid.Count && y + j < tilesGrid[i].Count)
							{
								tilesGrid[i+x][j+y].canDrawOnTop = false;
							}
						}
					}
				}
			}
		}
	}

	int GenerateTilesLine(int index)
	{
		if (tilesGrid.Count <= index)
			tilesGrid.Add(new List<LevelTile>(MAX_COLUMN_LENGTH));

		generatorIndex = 0;

		TrackSlice trackSlice = track.slices[index];
		Track.Direction direction = trackSlice.direction;
		int trackStartIndex = trackSlice.index;

		//Fill terrain from bottom till first ramp
		FillTerrain(index, generatorIndex, trackStartIndex - 2, GetRandomFillTileType());

		//Setup the types needed
		LevelTile.Type preRampBottom;
		LevelTile.Type rampBottom;
		LevelTile.Type preRampTop;
		LevelTile.Type rampTop;

		if (direction == Track.Direction.Up)
		{
			preRampBottom = LevelTile.Type.BottomUpCon;
			rampBottom = LevelTile.Type.BottomUp;
			preRampTop = LevelTile.Type.TopUpCon;
			rampTop = LevelTile.Type.TopUp;
		}
		else //if (direction == Track.Direction.Down)
		{
			preRampBottom = LevelTile.Type.BottomDownCon;
			rampBottom = LevelTile.Type.BottomDown;
			preRampTop = LevelTile.Type.TopDownCon;
			rampTop = LevelTile.Type.TopDown;
		}

		//Create the bottom ramp
		if (trackStartIndex >= 1)
		{
			CreateTileAtColumn(index, generatorIndex, preRampBottom);
		}

		CreateTileAtColumn(index, generatorIndex, rampBottom);
	
//		//The Gap
//		int trackHeight = GetTrackHeight();
//		FillTerrain(index, generatorIndex - 1, trackStartIndex + trackHeight, LevelTile.Type.Track);
//		generatorIndex -= 2;

		//Top ramp
		CreateTileAtColumn (index, generatorIndex, rampTop);
		CreateTileAtColumn (index, generatorIndex, preRampTop);

		//Fill the rest
		FillTerrain(index ,generatorIndex, MAX_COLUMN_LENGTH - 1, GetRandomFillTileType());

		return 0;

	}

	void FillTerrain(int column, int fromIndex, int toIndex, LevelTile.Type type)
	{
		for (int i = fromIndex; i <= toIndex; i++)
		{
			if (type == LevelTile.Type.Fill1 || type == LevelTile.Type.Fill2)
				CreateTileAtColumn(column, i, GetRandomFillTileType());
			else
				CreateTileAtColumn(column, i, type);
		}
	}

	void CreateTileAtColumn(int column, int index, LevelTile.Type type)
	{
		GameObject go = (GameObject)Instantiate(tilesDict[type]);
		LevelTile tile = go.GetComponent<LevelTile>();
		//Setup the tile
		go.transform.parent = transform.parent;
		Vector3 pos = Vector3.one;

		//Position the new tile at the end of the level
		if (column == 0)
		{
			pos.x = bottomLeftWorldPoint.x + column * tileSize.x;
			pos.y = bottomLeftWorldPoint.y + index * tileSize.y;
		}
		else if (index > 0)
		{
			GameObject refGO = tilesGrid[column][0].gameObject;

			pos.x = refGO.transform.localPosition.x;
			pos.y = bottomLeftWorldPoint.y + index * tileSize.y;
		}
		else  //Index == 0 && column != 0
		{
			GameObject refGO = tilesGrid[column - 1][0].gameObject;

			pos.x = refGO.transform.localPosition.x + tileSize.x;
			pos.y = bottomLeftWorldPoint.y + index * tileSize.y;
		}

		go.transform.localPosition = pos;

		//Create the AI Path
		if (type == LevelTile.Type.BottomUp || type == LevelTile.Type.BottomDown)
		{
			Vector2 checkPoint = new Vector2(go.transform.localPosition.x + tileSize.x/2f, go.transform.localPosition.y + tileSize.y);
			trackPath.Add(checkPoint);
			tilePath.Add(tile);
		}

		tilesGrid[column].Add(tile);
		generatorIndex++;
	}

	#endregion Level Generation


	#region Utility

	public List<Vector2> TrackToPath()
	{
		List<Vector2> path = new List<Vector2>();

		for (int i = 0; i < track.slices.Count; i++)
		{
			TrackSlice ts = track.slices[i];
			Vector2 position = new Vector2(bottomLeftWorldPoint.x + i * tileSize.x,
			                               bottomLeftWorldPoint.y + ts.index * tileSize.y);
			path.Add(position);
		}

		return path;
	}

	LevelTile.Type GetRandomFillTileType()
	{
		int rand = Random.Range(0, 1 + 1);
	
		if (rand == 1)
			return LevelTile.Type.Fill1;
		else
			return LevelTile.Type.Fill2;
	}

	int GetTrackHeight()
	{
//		return Mathf.CeilToInt(Screen.height/tileSize.y);
		return 2;
	}

	#endregion Utility
}

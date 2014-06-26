using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Track
{

	public enum Direction
	{
		Up,
		Down
	}

	//This composes a track
	public List<TrackSlice> slices;

	public int Length
	{
		get { return slices.Count; }
	}

	//Generator states
	private Direction currentTrailDirection;
	//Maximum number of track slices
	private int MAX_LENGTH;
	//Maximum number of vertical positions for track slices
	private int MAX_COLUMN_LENGTH;


	//Required constructor
	public Track (int maxLength, int maxColumnLength)
	{
		MAX_LENGTH = maxLength;
		MAX_COLUMN_LENGTH = maxColumnLength;
		slices = new List<TrackSlice>(MAX_LENGTH);
	}

	#region Generation

	public void GenerateInitialTrack()
	{
		//Actually starts with Direction.Up, because AddSlicesToEnd() change direction
		currentTrailDirection = Direction.Down;

		AddSlicesToEnd(MAX_LENGTH);

//		string debug = "";
//		foreach (TrackSlice ts in slices)
//		{
//			debug += " index[" + ts.index + "]:";
//			debug += ts.direction == Direction.Up ? " Up" : " Down";
//		}
//		Debug.Log (debug);
	}

	public void RemoveSlicesFromBeginning(int slicesCount)
	{
		slices.RemoveRange(0, slicesCount);
	}

	public int AddSlicesToEnd(int minSlicesCount)
	{
		int slicesCreated = 0;
		int minTrackSize = slices.Count + minSlicesCount;
		int columnIndex = 1;

		if (slices.Count > 0)
		{
			TrackSlice lastSlice = slices.Last();
			columnIndex = lastSlice.index;
		}


		while (slices.Count < minTrackSize)
		{
			currentTrailDirection = currentTrailDirection == Direction.Up ? Direction.Down : Direction.Up;
			int directionInspector = currentTrailDirection == Direction.Up? 1 : -1;

			int trailLength = RandomTrailLength(columnIndex, currentTrailDirection);
			
			for (int trailIndex = 0; trailIndex < trailLength; trailIndex++)
			{
				TrackSlice slice = new TrackSlice(columnIndex, trailIndex, currentTrailDirection);
				
				slices.Add(slice);
				slicesCreated++;
				//Don't change in the last iteration
				if (trailIndex != trailLength -1)
					columnIndex += currentTrailDirection == Direction.Up ? 1 : -1;
			}
		}
		return slicesCreated;
	}

	public void RegenerateSlices(int minSlicesCount)
	{
		RemoveSlicesFromBeginning(minSlicesCount);
		AddSlicesToEnd(minSlicesCount);
	}

	#endregion Generation

	#region Utility

	int RandomTrailIndex()
	{
		return Random.Range(1, MAX_COLUMN_LENGTH);
	}

	int RandomTrailLength(int startIndex, Direction direction)
	{
		int maxLength;

		if (direction == Direction.Up)
		{
			maxLength = MAX_COLUMN_LENGTH - Constants.TRAIL_OFFSET - startIndex;
		}
		else
		{
			maxLength = startIndex; //- Parameters.TRAIL_OFFSET;
		}

		if (maxLength < Constants.MIN_TRAIL_LENGTH)
		{
			return Constants.MIN_TRAIL_LENGTH;
		}

		int randomLength = Random.Range(Constants.MIN_TRAIL_LENGTH, maxLength + 1);

		return randomLength;
	}

	#endregion Utility
}

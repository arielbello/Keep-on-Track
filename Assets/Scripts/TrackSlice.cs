using UnityEngine;
using System.Collections;

public class TrackSlice
{
	public int index;
	public int currentDirectionIndex;

	public Track.Direction direction;

	Vector2 positionInLevel;

	public TrackSlice(int columnIndex, int currentDirectionIndex, Track.Direction direction)
	{
		this.index = columnIndex;
		this.currentDirectionIndex = currentDirectionIndex;
		this.direction = direction;
	}
}

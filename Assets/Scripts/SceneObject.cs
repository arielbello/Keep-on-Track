using UnityEngine;
using System.Collections;

public class SceneObject : MonoBehaviour 
{

	public enum Type
	{
		Tree,
		Cabin,
		Animal,
		None
	}

	public Type type;
	private bool _isAvailable = true;
	public bool isAvailable {
		get { return _isAvailable; } 
		set { 
			_isAvailable = value;
			this.gameObject.SetActive(!value);
		} 
	}
	public Vector2 size;


	void Start() 
	{
	
	}


	void Update() 
	{
	
	}
}

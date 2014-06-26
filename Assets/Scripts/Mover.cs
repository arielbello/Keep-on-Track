using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour {

	public float xSpeed = -Constants.GAME_SPEED;
	public float ySpeed = 0;
	private Camera mainCamera;


	void Start () 
	{
		mainCamera = Camera.main;
//		xSpeed = -Parameters.GAME_SPEED; //Set by Game Manager
	}
	

	void Update () 
	{
		float yTranslate = Time.deltaTime *ySpeed;

		transform.Translate(xSpeed * Time.deltaTime, ySpeed * Time.deltaTime, 0);
	}
}

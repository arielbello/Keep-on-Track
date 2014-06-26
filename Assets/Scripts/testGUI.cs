using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class testGUI : MonoBehaviour
{
	//GamEXP urls
	private string baseURLHome = "http://127.0.0.1:3000";
	private string baseURL = "http://gamexp-test.herokuapp.com";

	public LevelGenerator generator;

	private Vector2 boxSize = new Vector2(120, 100);
	private Vector2 topRightCorner;
	private Vector2 textOffSet = new Vector2(10, 50);
	private string responseBody = null;

	void Awake()
	{
		generator = GameObject.Find("LevelGenerator").GetComponent<LevelGenerator>();
		topRightCorner = new Vector2(Screen.width, Screen.height);
	}

	void OnGUI()
	{
		// Make a background box
		GUI.Box (new Rect(topRightCorner.x - textOffSet.x - boxSize.x, textOffSet.y, boxSize.x, boxSize.y), "GamEXP");


		// Make the first button. If it is pressed, execute the code between {}
		if (GUI.Button(new Rect(topRightCorner.x - textOffSet.x - boxSize.x, textOffSet.y  + 20, boxSize.x, 20), "Make a request"))
		{
//			generator.PrintSmth();
//			print(SystemInfo.deviceUniqueIdentifier);
			print ("Make request!" + Time.time);

			StartCoroutine(FetchRequest());

		}
		else if (GUI.Button(new Rect(topRightCorner.x - textOffSet.x - boxSize.x, textOffSet.y + 20 * 2, boxSize.x, 20), "DeviceID"))
		{
//			generator.GenerateSmth();
			responseBody = SystemInfo.deviceUniqueIdentifier;
		}
		else if (GUI.Button(new Rect(topRightCorner.x - textOffSet.x - boxSize.x, textOffSet.y + 20 * 3, boxSize.x, 20), "Create a config"))
		{
			StartCoroutine(PostParams());
		}

		if (responseBody != null)
		{
			GUI.TextArea(new Rect(10, 10, 400, 300), responseBody);
		}
	}

	IEnumerator FetchRequest()
	{
		
		WWW test = new WWW (baseURLHome + "/");
		yield return test;
		ProcessRequest(test.text);
	}

	IEnumerator PostParams()
	{
		var postHeader = new Hashtable();  
		var encoding = new System.Text.UTF8Encoding();  
		postHeader.Add("Content-type", "application/json"); 
		postHeader.Add("Accept", "application/json");
		var jsonStr = ConfString();    
		print (jsonStr);
		byte[] postData = encoding.GetBytes(jsonStr);


		var request = new WWW(baseURLHome + "/confs", postData, postHeader);

		yield return request;

		//DO NOT ATTEMPT TO ACCESS REQUEST.TEXT WITHOUT CHECKING FOR ERROR FIRST!
		//CAUSES CRAZY BEHAVIOUR!

//		try {
//
			if (request != null)
			print (request);
//			{
				if (request.error != null)
			print (" ERRO !");
		else
			print (" NO ERRO!");
//				{
//					responseBody = request.text;
//					print ("post request text:" + request.text);
//				}
//				else
//					print (request.ToString());
//			}
//		}
//		catch
//		{
//			print ("catched!");
//		}


	}

	string ConfString()
	{
		JSONObject jsonPost = new JSONObject(JSONObject.Type.OBJECT);
		jsonPost.AddField("custom", "Unity test");
		jsonPost.AddField("player_id", 1);
		jsonPost.AddField("game_id", 1);
	
		return jsonPost.ToString();
	}
	
	void ProcessRequest(string text)
	{
		//		print ("this.request: " + request.response);
		
//		JSONObject jsonResponse = new JSONObject(text);
		
		responseBody = text;
		print (responseBody);
		//Create a param name, value dictionary
		Dictionary<string, float> gameParams = new Dictionary<string, float>();
		
		//It's a params list
//		foreach (JSONObject jObj in jsonResponse.list)
//		{
//			string name = jObj.GetField("name").str;
//			float value = jObj.GetField("value").f;
//			gameParams.Add(name,value);
//		}
		
//		Params.waveWait = gameParams["waveWait"];
//		Params.waveWaitDecrease = gameParams["waveWaitDecrease"];
//		Params.enemySpeedIncrease = gameParams["enemySpeedIncrease"];
//		Params.playerFireRate = gameParams["playerFireRate"];
//		Params.spawnWait = gameParams["spawnWait"];
//		Params.spawnWaitDecrease = gameParams["spawnWaitDecrease"];
		
//		accessData (jsonResponse);
		
		//		print (pRequest.response.Text);
		

		
		//		if (jsonResponse != null)
		//		{
		//			print(jsonResponse);
		//		}
		//		else
		//			print ("Could not parse JSON");
	}
}

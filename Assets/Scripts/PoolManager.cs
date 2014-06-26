using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PoolManager : MonoBehaviour 
{
	public static PoolManager Instance;
	public List<GameObject> trees;
	public List<GameObject> cabins;
	public List<GameObject> animals;
//	public List<GameObject> sceneObjects;

	public Dictionary<SceneObject.Type, List<SceneObject>> sceneObjectsPool;

	private Dictionary<SceneObject.Type, int> randIndexes;
	private bool didSetup = false;

	#region Monobehaviour

	void Awake()
	{
		Instance = this;
	}

	void Start () 
	{
		if (!didSetup)
			Setup();
	}

	#endregion Monobehaviour

	public void Setup ()
	{
		if (didSetup)
			return;

		sceneObjectsPool = new Dictionary<SceneObject.Type, List<SceneObject>>();
		randIndexes = new Dictionary<SceneObject.Type, int>();
		
		LoadSceneObjects(trees, Constants.MAX_TREES);
		LoadSceneObjects(cabins, Constants.MAX_CABINS);
		LoadSceneObjects(animals, Constants.MAX_ANIMALS);
	}

	#region Interface

	public SceneObject GetPooledSceneObject(SceneObject.Type type)
	{
		if (sceneObjectsPool.ContainsKey(type) && sceneObjectsPool[type].Count > 0)
		{
			SceneObject sceneObj;
			//			List<SceneObject> pool = sceneObjectsPool[type];
			for (int i = 0; i < sceneObjectsPool[type].Count; i++)
			{
				int index = (randIndexes[type] + i) % sceneObjectsPool[type].Count;
				sceneObj = sceneObjectsPool[type][index];
				
				if (sceneObj.isAvailable)
				{
					randIndexes[type] = index;
					return sceneObj;
				}
			}
		}

		return null;
	}

	#endregion Interface


	#region Util

	void LoadSceneObjects(List<GameObject> objs, int poolSize)
	{
		if (objs == null || objs.Count == 0)
			return;

		//Add a random index, to have a different seed to start generating this type of object
		int randIndex = Random.Range(0, objs.Count);
		List<SceneObject> poolList = new List<SceneObject>();
		SceneObject refSceneObj = objs.FirstOrDefault().GetComponent<SceneObject>();

		for (int i = 0; poolList.Count < poolSize || poolList.Count < objs.Count; i++)
		{
			int index = i % objs.Count;
			GameObject go = (GameObject)Instantiate(objs[index]);
			go.transform.parent = LevelGenerator.Instance.transform.parent;
			SceneObject newSceneObj = go.GetComponent<SceneObject>();
			newSceneObj.isAvailable = true;

			poolList.Add(newSceneObj);
		}
		randIndexes.Add(refSceneObj.type, randIndex);
		//Add to the pool
		sceneObjectsPool.Add(refSceneObj.type, poolList);
	}
//
//	void LoadSceneObjects(SceneObject sceneObj ,int poolSize)
//	{
//		for (int i = 0; i < poolSize; i++)
//		{
//			GameObject gameObj = (GameObject)Instantiate(sceneObj.gameObject);
//			gameObj.transform.parent = LevelGenerator.Instance.transform.parent;
//			SceneObject newSceneObj = gameObj.GetComponent<SceneObject>();
//			newSceneObj.isAvailable = true;
//
//			AddSceneObjectToPool(newSceneObj);
//		}
//	}


	void AddSceneObjectToPool(SceneObject sceneObj)
	{
		if (sceneObjectsPool.ContainsKey(sceneObj.type))
		{
			sceneObjectsPool[sceneObj.type].Add(sceneObj);
		}
		else
		{
			List<SceneObject> sceneObjsList = new List<SceneObject>();
			sceneObjsList.Add(sceneObj);
			sceneObjectsPool.Add(sceneObj.type, sceneObjsList);
		}
	}
	#endregion Util
}

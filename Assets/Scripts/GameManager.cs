using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
	#region Variables / Properties
	#region Public
	// -- Properties --
	public Player PlayerObject { get => m_PlayerObject; }
	public int Lanes { get => m_Lanes; }

	// -- Singleton --
	public static GameManager Instance { get; private set; }
	#endregion

	#region Private
	// -- Editable in Inspector --
	[Header("Parameters")]
	[SerializeField] int m_Lanes = 2;
	[Header("References")]
	[SerializeField] Block m_BlockPrefab = null;
	[SerializeField] Player m_PlayerObject = null;
	[SerializeField] TextMeshProUGUI m_DistanceCounter = null;
	[SerializeField] GameObject[] m_WallPrefabs = null;
	// -- Cached Components

	// -- Input --

	// -- Misc --
	List<Block> m_BlockList = new List<Block>();
	List<GameObject> m_WallList = new List<GameObject>();
	private float m_FurthestDistanceSpawned = 0.0f;
	private float m_FurthestWallSpawned = 0.0f;
	#endregion
	#endregion

	#region Unity Functions
	/// <summary>
	/// Initializes singleton.
	/// </summary>
	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		SpawnBlocks(true);
		SpawnWalls(true);
	}

	void Update()
	{
		m_DistanceCounter.text = PlayerObject.transform.position.z.ToString("#") + "m";

		if (m_PlayerObject.transform.position.z >= m_FurthestDistanceSpawned - 75.0f)
			SpawnBlocks();

		if (m_PlayerObject.transform.position.z >= m_FurthestWallSpawned - 750.0f)
			SpawnWalls();
	}
	#endregion

	#region Public Functions
	public void Restart()
	{
		// Reset player
		PlayerObject.GetComponent<Player>().Reset();

		// Reset and create new blocks
		SpawnBlocks(true);
		SpawnWalls(true);
	}

	/// <summary>
	/// Creates block walls.
	/// </summary>
	/// <param name="fromScratch">Erases all existing blocks and creates a few from the starting position.</param>
	private void SpawnBlocks(bool fromScratch = false)
	{
		// Clear existing blocks if spawning from scratch
		if (fromScratch)
		{
			for (int i = m_BlockList.Count - 1; i >= 0; i--)
			{
				Destroy(m_BlockList[i].gameObject);
				m_BlockList.RemoveAt(i);
			}
			m_BlockList.Clear();
			m_FurthestDistanceSpawned = 28.75f;
		}

		// Set initial spawn points
		float spacing = PlayerObject.GetComponent<Player>().Speed * 3.0f;
		float x = -3.0f;
		float z = m_FurthestDistanceSpawned + spacing;
		
		for (int i = 0; i < (fromScratch ? 5 : 1); i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (Random.value < 0.3f)
					continue;

				// Spawn new block
				float ySpawn = -10.0f - (fromScratch ? (10.0f * i) - (10.0f * j) : Random.Range(10.0f, 40.0f));
				Vector3 spawnPos = new Vector3(x + (j * 3.0f), ySpawn, z + (i * spacing));

				Block newBlock = Instantiate(m_BlockPrefab, spawnPos, Quaternion.identity);
				if (Random.value < 0.5f)
					newBlock.TogglePolarity();

				m_BlockList.Add(newBlock);
				m_FurthestDistanceSpawned = spawnPos.z;






				// Spawn new block
				ySpawn = 10.0f + Random.Range(30.0f, 70.0f);
				spawnPos = new Vector3(x + (j * 3.0f), ySpawn, z + (i * spacing));

				newBlock = Instantiate(m_BlockPrefab, spawnPos, Quaternion.identity);
				if (Random.value < 0.5f)
					newBlock.TogglePolarity();

				m_BlockList.Add(newBlock);
				m_FurthestDistanceSpawned = spawnPos.z;
			}
		}
	}

	/// <summary>
	/// Culls walls currently out of view, and creates enough walls to see into the distance.
	/// </summary>
	/// <param name="fromScratch">Erases all existing walls and creates from the starting position</param>
	private void SpawnWalls(bool fromScratch = false)
	{
		// Cull all walls that are out of view
		if (m_WallList != null)
		{
			for (int i = m_WallList.Count - 1; i >= 0; i--)
			{
				if (fromScratch || m_WallList[i].transform.position.z < PlayerObject.transform.position.z - 50.0f)
				{
					Destroy(m_WallList[i]);
					m_WallList.RemoveAt(i);
				}
			}
		}

		if (fromScratch)
			m_FurthestWallSpawned = -25.0f;

		// Create new walls
		float z = m_FurthestWallSpawned + 25.0f;
		for (int i = 0; i < 15; i++)
		{
			for (int j = -2; j < 2; j++)
			{
				Vector3 spawnPosL = new Vector3((fromScratch ? -35.0f : -75.0f), (j * 25.0f), z + (i * 25.0f));
				Vector3 spawnPosR = new Vector3((fromScratch ? 35.0f : 75.0f), (j * 25.0f), z + (i * 25.0f));
				GameObject newWallL = Instantiate(m_WallPrefabs[Random.Range(0, m_WallPrefabs.Length)], spawnPosL, Quaternion.identity);
				GameObject newWallR = Instantiate(m_WallPrefabs[Random.Range(0, m_WallPrefabs.Length)], spawnPosR, Quaternion.Euler(new Vector3(0.0f, 180.0f, 0.0f)));

				m_WallList.Add(newWallL);
				m_WallList.Add(newWallR);
				m_FurthestWallSpawned = spawnPosL.z;
			}
		}
	}
	#endregion
}

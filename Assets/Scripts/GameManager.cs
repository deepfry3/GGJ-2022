using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
	#region Variables / Properties
	#region Public
	// -- Properties --
	public Player PlayerObject { get => m_PlayerObject; }

	// -- Singleton --
	public static GameManager Instance { get; private set; }
	#endregion

	#region Private
	// -- Editable in Inspector --
	[Header("Parameters")]
	[SerializeField] bool m_SkipTutorial = false;
	[Header("References")]
	[SerializeField] AudioSource m_EnvironmentAudioSource = null;
	[SerializeField] AudioSource m_MusicAudioSource = null;
	[SerializeField] GameObject m_TutorialPanel = null;
	[SerializeField] GameObject m_TutorialQuotePanel = null;
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
	private float m_TutorialQuoteCountdown = -1.0f;
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

		Time.timeScale = 0.0f;
	}

	void Update()
	{
		if (Time.timeScale == 0.0f)
		{
			if (m_TutorialQuoteCountdown > 0.0f)
			{
				m_TutorialQuoteCountdown -= Time.unscaledDeltaTime;
				if (m_TutorialQuoteCountdown <= 0.0f)
				{
					Time.timeScale = 1.0f;
					m_EnvironmentAudioSource.Play();
					m_TutorialQuotePanel.SetActive(false);
				}
			}
		}

		m_DistanceCounter.text = PlayerObject.DistanceTravelled.ToString("#");

		if (m_PlayerObject.transform.position.z >= m_FurthestDistanceSpawned - 75.0f)
			SpawnBlocks();

		if (m_PlayerObject.transform.position.z >= m_FurthestWallSpawned - 225.0f)
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
	/// Skips the tutorial if currently open
	/// </summary>
	public void SkipTutorial()
	{
		m_TutorialPanel.SetActive(false);
		m_TutorialQuotePanel.SetActive(true);
		m_TutorialQuoteCountdown = 5.5f;
	}
	#endregion

	#region Private Functions
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
		float spacing = PlayerObject.GetComponent<Player>().Speed + 15.0f;
		float x = -3.0f;
		float z = m_FurthestDistanceSpawned + spacing;
		
		for (int i = 0; i < (fromScratch ? 5 : 1); i++)
		{
			bool[] topRow = new bool[3], bottomRow = new bool[3];
			do
			{
				topRow[0] = Random.Range(0, 2) == 1; topRow[1] = Random.Range(0, 2) == 1; topRow[2] = Random.Range(0, 2) == 1;
				bottomRow[0] = Random.Range(0, 2) == 1; bottomRow[1] = Random.Range(0, 2) == 1; bottomRow[2] = Random.Range(0, 2) == 1;
			} while ((!topRow[0] && !topRow[1] && !topRow[2]) || (!bottomRow[0] && !bottomRow[1] && !bottomRow[2]));

			for (int j = 0; j < 3; j++)
			{
				if (topRow[j])
				{
					float y = 18.0f + Random.Range(30.0f, 70.0f);
					Vector3 spawnPos = new Vector3(x + (j * 3.0f), y, z + (i * spacing));

					Block newBlock = Instantiate(m_BlockPrefab, spawnPos, Quaternion.identity);
					if (Random.value < 0.5f)
						newBlock.TogglePolarity();

					m_BlockList.Add(newBlock);
					m_FurthestDistanceSpawned = spawnPos.z;
				}

				if (bottomRow[j])
				{
					float y = -10.0f - (fromScratch ? (10.0f * i) - (10.0f * j) : Random.Range(10.0f, 40.0f));
					Vector3 spawnPos = new Vector3(x + (j * 3.0f), y, z + (i * spacing));

					Block newBlock = Instantiate(m_BlockPrefab, spawnPos, Quaternion.identity);
					if (Random.value < 0.5f)
						newBlock.TogglePolarity();

					m_BlockList.Add(newBlock);
					m_FurthestDistanceSpawned = spawnPos.z;
				}
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
			m_FurthestWallSpawned = -24.5f;

		// Create new walls
		float z = m_FurthestWallSpawned + 24.5f;
		for (int i = 0; i < 10; i++)
		{
			for (int j = -2; j < 3; j++)
			{
				Vector3 spawnPosL = new Vector3((fromScratch ? -35.0f : -75.0f), (j * 24.5f), z + (i * 24.5f));
				Vector3 spawnPosR = new Vector3((fromScratch ? 35.0f : 75.0f), (j * 24.5f), z + (i * 24.5f));
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

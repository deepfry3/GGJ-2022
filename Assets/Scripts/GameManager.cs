using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	// -- Cached Components

	// -- Input --

	// -- Misc --
	List<Block> m_BlockList = new List<Block>();
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
		float z = 20.75f;
		float x = -3.0f;
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (Random.value < 0.5f)
					continue;

				// Spawn new block
				Vector3 spawnPos = new Vector3(x + (j * 3.0f), 0.0f, z + (i * 10.0f));
				Block newBlock = Instantiate(m_BlockPrefab, spawnPos, Quaternion.identity);
				if (Random.value < 0.5f)
					newBlock.TogglePolarity();

				m_BlockList.Add(newBlock);
			}
		}
	}
	#endregion

	#region Public Functions
	public void Restart()
	{
		// Reset player
		PlayerObject.GetComponent<Player>().Reset();

		// Reset blocks
		for (int i = m_BlockList.Count - 1; i >= 0; i--)
		{
			Destroy(m_BlockList[i].gameObject);
			m_BlockList.RemoveAt(i);
		}
		m_BlockList.Clear();

		// Create new blocks
		float z = 20.75f;
		float x = -3.0f;
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (Random.value < 0.5f)
					continue;

				// Spawn new block
				Vector3 spawnPos = new Vector3(x + (j * 3.0f), 0.0f, z + (i * 10.0f));
				Block newBlock = Instantiate(m_BlockPrefab, spawnPos, Quaternion.identity);
				if (Random.value < 0.5f)
					newBlock.TogglePolarity();

				m_BlockList.Add(newBlock);
			}
		}
	}

	#endregion
}

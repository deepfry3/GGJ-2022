﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	#region Variables / Properties
	#region Public
	// -- Properties --
	#endregion

	#region Private
	// -- Editable in Inspector --
	[Header("References")]
	[SerializeField] Block m_BlockPrefab = null;
	// -- Cached Components

	// -- Input --

	#endregion
	#endregion

	// Start is called before the first frame update
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
			}
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

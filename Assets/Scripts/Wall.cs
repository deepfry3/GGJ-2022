using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
	#region Variables / Properties
	#region Public
	// -- Properties --
	#endregion

	#region Private
	// -- Editable in Inspector --
	[Header("References")]
	[SerializeField] GameObject m_SparksObject = null;
	#endregion
	#endregion

	#region Unity Functions
	/// <summary>
	/// Randomly enables/disables sparks and rotates wall pieces randomly
	/// </summary>
	void Start()
	{
		// Rotate randomly
		int r = Random.Range(0, 4);
		float x = (r == 0) ? 0.0f : (r == 1) ? 90.0f : (r == 2) ? 180.0f : (r == 3) ? 270.0f : 0.0f;
		transform.localEulerAngles = new Vector3(x, transform.localEulerAngles.y, transform.localEulerAngles.z);

		if (Random.value < 0.75f)
		{
			m_SparksObject.SetActive(false);
			return;
		}

		m_SparksObject.SetActive(true);
		m_SparksObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Random.Range(0.0f, 50.0f));
	}

	/// <summary>
	/// Animate in
	/// </summary>
	void Update()
	{
		if (transform.position.x < 0.0f && transform.position.x != -35.0f)
		{
			Vector3 target = new Vector3(-35.0f, transform.position.y, transform.position.z);
			transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 1.5f);
		}
		else if (transform.position.x > 0.0f && transform.position.x != 35.0f)
		{
			Vector3 target = new Vector3(35.0f, transform.position.y, transform.position.z);
			transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 1.5f);
		}
	}
	#endregion
}

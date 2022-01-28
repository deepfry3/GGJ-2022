using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
	#region Variables / Properties
	#region Public
	// -- Properties --
	public bool IsRed { get => m_IsRed; set => m_IsRed = value; }
	public bool TouchingPlayer { get => GameManager.Instance.PlayerObject.BlocksTouching.Contains(gameObject); }
	#endregion

	#region Private
	// -- Editable in Inspector --
	[Header("Parameters")]
	[SerializeField] bool m_IsRed = false;
	[SerializeField] float m_Force = 10.0f;
	[SerializeField] [Range(0.0f, 1.0f)] float m_HForceMultiplier = 1.0f;
	[SerializeField] [Range(0.0f, 1.0f)] float m_VForceMultiplier = 1.0f;
	[SerializeField] bool m_GradualRolloff = false;
	[Header("References")]
	[SerializeField] SphereCollider m_SphereTrigger = null;
	[SerializeField] Material m_RedMaterial = null;
	[SerializeField] Material m_BlueMaterial = null;

	// -- Cached Components --
	private MeshRenderer m_Renderer = null;
	#endregion
	#endregion

	#region Unity Functions
	/// <summary>
	/// Called on Awake.
	/// Caches components.
	/// </summary>
	void Awake()
	{
		// Cache components
		m_Renderer = GetComponent<MeshRenderer>();
	}

	/// <summary>
	/// Called when another collider enters the trigger
	/// </summary>
	/// <param name="other">The other collider</param>
	void OnTriggerEnter(Collider other)
	{
		// Potentially activate a line showing they're connected
	}

	void OnTriggerStay(Collider other)
	{
		// Early-exit
		GameObject obj = other.gameObject;
		if (obj.tag != "Player")
			return;

		// Calculate force
		Vector3 distance = obj.transform.position - transform.position;
		Vector3 force = m_Force * distance.normalized * (10.0f * Time.deltaTime);
		if (m_GradualRolloff)
			force *= Mathf.Lerp(1.0f, 0.25f, Mathf.Min(distance.magnitude, m_SphereTrigger.radius) / m_SphereTrigger.radius);
		if (IsRed != obj.GetComponent<Player>().IsRed)
			force = -force;

		// Apply force
		if (!TouchingPlayer || IsRed)
			obj.GetComponent<Player>().AddForce(force);
	}
	#endregion

	#region Public Functions
	/// <summary>
	/// Sets the block to be red or blue.
	/// </summary>
	/// <param name="value">Block should be red (true) or blue (false)</param>
	public void SetPolarity(bool value)
	{
		Debug.Log(m_RedMaterial.name);
		Debug.Log(m_BlueMaterial.name);
		IsRed = value;
		m_Renderer.material = IsRed ? m_RedMaterial : m_BlueMaterial;
	}

	/// <summary>
	/// Toggles the block between red and blue polarity.
	/// </summary>
	public void TogglePolarity()
	{
		SetPolarity(!IsRed);
	}
	#endregion

	#region Private Functions
	#endregion
}

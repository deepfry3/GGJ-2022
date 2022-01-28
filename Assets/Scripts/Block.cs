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
	[SerializeField] BoxCollider m_BoxTrigger = null;
	[SerializeField] Material m_RedMaterial = null;
	[SerializeField] Material m_BlueMaterial = null;

	// -- Cached Components --
	private MeshRenderer m_Renderer = null;
	private float m_BoxTriggerExtent = 0.0f;
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
		m_BoxTriggerExtent = m_BoxTrigger.size.y / 2.0f;
	}

	/// <summary>
	/// Animate in
	/// </summary>
	void Update()
	{
		if (transform.position.y != 0.0f)
		{
			Vector3 target = new Vector3(transform.position.x, 0.0f, transform.position.z);
			transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 3.5f);
		}
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
			force *= Mathf.Lerp(1.0f, 0.25f, Mathf.Min(distance.magnitude, m_BoxTriggerExtent) / m_BoxTriggerExtent);
		if (IsRed != obj.GetComponent<Player>().IsRed)
			force = -force * 15.0f;

		// Apply force
		if (!TouchingPlayer || IsRed == GameManager.Instance.PlayerObject.GetComponent<Player>().IsRed)
			obj.GetComponent<Player>().AddForce(force);
	}
	#endregion

	#region Public Functions
	/// <summary>
	/// Sets the length of the block.
	/// </summary>
	/// <param name="value">Length to set</param>
	public void SetLength(float value)
	{
		transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, value);
	}

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

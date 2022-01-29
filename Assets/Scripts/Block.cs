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
	[SerializeField] AudioSource m_AudioSource = null;
	[SerializeField] GameObject m_Chain = null;
	[SerializeField] MeshRenderer[] m_Renderers = null;
	[Header("Audio")]
	[SerializeField] AudioClip m_MagnetAttractSound = null;
	[SerializeField] AudioClip m_MagnetRepelSound = null;

	// -- Cached Components --
	private MeshRenderer m_Renderer = null;
	private LineRenderer m_LineRenderer = null;
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
		m_LineRenderer = GetComponent<LineRenderer>();
	}

	/// <summary>
	/// Enable/disable chain
	/// </summary>
	void Start()
	{
		if (transform.position.y > 0.0f)
			m_Chain.SetActive(true);
		else
			m_Chain.SetActive(false);
	}

	/// <summary>
	/// Animate in
	/// </summary>
	void Update()
	{
		if (transform.position.y < 0.0f)
		{
			Vector3 target = new Vector3(transform.position.x, 0.0f, transform.position.z);
			transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 3.5f);
		}
		else if (transform.position.y > 18.0f)
		{
			Vector3 target = new Vector3(transform.position.x, 18.0f, transform.position.z);
			transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 3.5f);
		}
	}

	/// <summary>
	/// Called when another collider enters the trigger
	/// </summary>
	/// <param name="other">The other collider</param>
	void OnTriggerEnter(Collider other)
	{
		// Early-exit
		GameObject obj = other.gameObject;
		if (obj.tag != "Player")
			return;

		m_LineRenderer.enabled = true;
		m_AudioSource.PlayOneShot(m_MagnetAttractSound);
	}

	void OnTriggerStay(Collider other)
	{
		// Early-exit
		GameObject obj = other.gameObject;
		if (obj.tag != "Player")
			return;

		// Store reference to player
		Player player = obj.GetComponent<Player>();

		// Calculate force
		Vector3 distance = obj.transform.position - transform.position;
		Vector3 force = m_Force * distance.normalized * (10.0f * Time.deltaTime);
		force *= (1.0f + (Mathf.Min(player.Speed / 30.0f, 3.0f) - 0.25f));
		if (m_GradualRolloff)
			force *= Mathf.Lerp(1.0f, 0.25f, Mathf.Min(distance.magnitude, m_BoxTriggerExtent) / m_BoxTriggerExtent);
		if (IsRed != player.IsRed)
			force = -force * 15.0f;

		// Massage force as required
		Vector3 playerForce = player.ForceVelocity;
		float massagedY = force.y, massagedZ = force.z;
		if (playerForce.y <= 0.1f && force.y > 0.0f)         // If falling and being pulled up, get pulled up stronger
			massagedY *= 10.0f;
		else if (playerForce.y > 0.1f && force.y > 0.0f)    // If flinging up and being pulled up, get pulled up weaker
			massagedY *= 0.55f;
		if (force.z > 0.0f)									// If being pulled forward, get pulled stronger
			massagedZ *= 4.0f;

		// Stronger repel
		if (player.transform.position.y > transform.position.y && force.y > 0.0f)
			massagedY *= 2.0f;

		// Apply force
		force = new Vector3(force.x, massagedY, massagedZ);
		if (!TouchingPlayer || IsRed == GameManager.Instance.PlayerObject.GetComponent<Player>().IsRed)
			obj.GetComponent<Player>().AddForce(force);

		// Render line
		m_LineRenderer.SetPositions(new Vector3[] {
				transform.position, GameManager.Instance.PlayerObject.transform.position
			});
	}

	void OnTriggerExit(Collider other)
	{
		// Early-exit
		GameObject obj = other.gameObject;
		if (obj.tag != "Player")
			return;

		m_LineRenderer.enabled = false;
		m_AudioSource.PlayOneShot(m_MagnetRepelSound);
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
		IsRed = value;
		for (int i = 0; i < m_Renderers.Length; i++)
			m_Renderers[i].material = IsRed ? m_RedMaterial : m_BlueMaterial;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
	#region Variables / Properties
	#region Public
	// -- Properties --
	public bool IsGrounded { get; private set; }
	public bool LastGrounded { get; private set; }
	public bool IsRed { get; private set; } = true;
	public List<GameObject> BlocksTouching = new List<GameObject>();
	public bool AttachedToBlock { get => m_AttachedToBox; }
	public Vector3 ForceVelocity { get => m_ForceMoveVelocity; }
	public float Speed { get => m_Speed; }
	#endregion

	#region Private
	// -- Editable in Inspector --
	[Header("Movement")]
	[SerializeField] float m_Speed = 10.0f;
	[SerializeField] float m_Drag = 1.0f;
	[SerializeField] float m_Gravity = -9.81f;
	[Header("References")]
	[SerializeField] Material m_RedMaterial = null;
	[SerializeField] Material m_BlueMaterial = null;
	[SerializeField] MeshRenderer[] m_Renderers = null;
	[Header("Audio")]
	[SerializeField] AudioClip m_DeathSound = null;

	// -- Cached Components
	private PlayerInput m_Input = null;
	private CharacterController m_CharController = null;
	private AudioSource m_AudioSource = null;
	private MeshRenderer m_Renderer = null;

	// -- Input --
	private Vector2 m_InputMove = Vector2.zero;
	private bool m_InputChange = false;

	// -- Misc. --
	private Vector3 m_PlayerMoveVelocity = Vector3.zero;
	private float m_StartSpeed = 0.0f;
	private float m_DeathTimer = -1.0f;
	private bool m_AttachedToBox = false;
	[SerializeField] private Vector3 m_ForceMoveVelocity = Vector3.zero;
	#endregion
	#endregion

	#region Unity Functions
	/// <summary>
	/// Called on Awake.
	/// Caches components and initialize variables.
	/// </summary>
	void Awake()
	{
		// Cache components
		m_Input = GetComponent<PlayerInput>();
		m_CharController = GetComponent<CharacterController>();
		m_AudioSource = GetComponent<AudioSource>();
		m_Renderer = GetComponent<MeshRenderer>();

		// Initialize variables
		m_StartSpeed = m_Speed;
	}

	void Update()
	{
		#region States
		// Update if grounded
		LastGrounded = IsGrounded;
		float castDistance = (m_CharController.height * 0.5f) - m_CharController.radius + 0.01f;
		RaycastHit groundHit;
		IsGrounded = Physics.SphereCast(transform.position, m_CharController.radius, Vector3.down, out groundHit, castDistance, int.MaxValue, QueryTriggerInteraction.Ignore);
		if (IsGrounded)
		{
			Transform hitTransform = groundHit.transform;
			if (groundHit.transform.tag == "Block")
			{
			}
		}

		// Update if touching box
		Vector3 boxExtents = GetComponent<BoxCollider>().size / 2.0f;
		boxExtents.x += 0.1f; boxExtents.y += 0.1f; boxExtents.z += 0.1f;
		Collider[] colliders = Physics.OverlapBox(transform.position, boxExtents, Quaternion.identity, int.MaxValue, QueryTriggerInteraction.Ignore);
		BlocksTouching.Clear();
		m_AttachedToBox = false;
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject.tag == "Block")
			{
				BlocksTouching.Add(colliders[i].gameObject);
				if (colliders[i].gameObject.GetComponent<Block>().IsRed != IsRed)
					m_AttachedToBox = true;
			}
		}

		// Update speed
		m_Speed += Time.deltaTime / 3.0f;
		#endregion

		#region Movement
		#region Momentum
		// Gravity
		if (IsGrounded && m_ForceMoveVelocity.y < 0.0f)
			m_ForceMoveVelocity.y = 0.0f;
		else if (!IsGrounded)
			m_ForceMoveVelocity.y += m_Gravity * Time.deltaTime;

		// Lateral momentum
		if (m_ForceMoveVelocity.x != 0.0f || m_ForceMoveVelocity.z != 0.0f)
		{
			Vector3 prev = m_ForceMoveVelocity;
			m_ForceMoveVelocity.x -= Sign(m_ForceMoveVelocity.x) * m_Drag * Time.deltaTime;
			m_ForceMoveVelocity.z -= Sign(m_ForceMoveVelocity.z) * m_Drag * Time.deltaTime;
			if (Sign(m_ForceMoveVelocity.x) != Sign(prev.x))
				m_ForceMoveVelocity.x = 0.0f;
			if (Sign(m_ForceMoveVelocity.z) != Sign(prev.z))
				m_ForceMoveVelocity.z = 0.0f;
		}

		#endregion


		#endregion

		#region Perform movement
		// Get movement vector based on player input
		Vector3 moveVec = new Vector3(0.0f, 0.0f, m_Speed);
		if (m_AttachedToBox)
			moveVec = Vector3.zero;

		// Add velocity and slight downwards force so Move always does something (fixes RB collisions)
		moveVec += m_ForceMoveVelocity;
		moveVec = new Vector3(moveVec.x, moveVec.y - Time.deltaTime, moveVec.z);
		if (moveVec != Vector3.zero)
		{
			m_CharController.Move(moveVec * Time.deltaTime);
		}
		#endregion

		#region Death
		if (transform.position.y < -10.0f && m_ForceMoveVelocity.y < -10.0f && m_DeathTimer == -1.0f)
		{
			m_DeathTimer = 2.0f;
			m_AudioSource.PlayOneShot(m_DeathSound);
		}
		if (m_DeathTimer > 0.0f)
		{
			m_DeathTimer -= Time.deltaTime;
			if (m_DeathTimer <= 0.0f)
			{
				GameManager.Instance.Restart();
			}
		}
		#endregion
	}

	private void OnCollisionEnter(Collision collision)
	{
		Debug.Log("COLLISION");
	}
	#endregion

	#region Input Actions
	/// <summary>
	/// Called on Player invoking the 'Move' PlayerAction.
	/// Stores the movement vector in a private variable.
	/// </summary>
	/// <param name="value">Information returned on that action by the Input System</param>
	public void OnMoveInput(InputAction.CallbackContext value)
	{
		m_InputMove = Vector2.ClampMagnitude(value.ReadValue<Vector2>(), 1.0f);
	}

	/// <summary>
	/// Called on Player invoking the 'Left' PlayerAction.
	/// Player moves left one lane.
	/// </summary>
	/// <param name="value">Information returned on that action by the Input System</param>
	public void OnLeftInput(InputAction.CallbackContext value)
	{
		if (value.started)
			ChangeLane(false);
	}

	/// <summary>
	/// Called on Player invoking the 'Left' PlayerAction.
	/// Player moves right one lane.
	/// </summary>
	/// <param name="value">Information returned on that action by the Input System</param>
	public void OnRightInput(InputAction.CallbackContext value)
	{
		if (value.started)
			ChangeLane(true);
	}

	/// <summary>
	/// Called on Player invoking the 'Change' PlayerAction.
	/// </summary>
	/// <param name="value">Information returned on that action by the Input System</param>
	public void OnChangeInput(InputAction.CallbackContext value)
	{
		if (value.started)
			TogglePolarity();
	}
	#endregion

	#region Public Functions
	/// <summary>
	/// Adds the specified amount of force to the player's velocity.
	/// </summary>
	/// <param name="force">Vector3 of forces to apply</param>
	public void AddForce(Vector3 force)
	{
		m_ForceMoveVelocity += force;
	}

	/// <summary>
	/// Resets and sets the specified amount of force to apply to the player's velocity.
	/// </summary>
	/// <param name="force">Vector3 of forces to apply</param>
	public void SetForce(Vector3 force)
	{
		m_ForceMoveVelocity = Vector3.zero;
		AddForce(force);
	}

	/// <summary>
	/// Resets the player to the original speed and position
	/// </summary>
	public void Reset()
	{
		m_CharController.enabled = false;
		transform.position = new Vector3(0.0f, 1.5f, 0.0f);

		m_DeathTimer = -1.0f;
		m_Speed = m_StartSpeed;
		m_ForceMoveVelocity = Vector3.zero;
		m_CharController.enabled = true;
	}
	#endregion

	#region Private Functions
	/// <summary>
	/// Returns the sign of the specified float (-1, 0, or 1).
	/// </summary>
	/// <param name="value">Float to return the sign of</param>
	/// <returns>The sign of the specified float (-1, 0, or 1)</returns>
	private float Sign(float value)
	{
		return value < 0.0f ? -1.0f : value > 0.0f ? 1.0f : 0.0f;
	}

	/// <summary>
	/// Toggles the polarity of the player between red and blue.
	/// </summary>
	private void TogglePolarity()
	{
		m_Renderer.material = IsRed ? m_BlueMaterial : m_RedMaterial;
		for (int i = 0; i < m_Renderers.Length; i++)
			m_Renderers[i].material = IsRed ? m_BlueMaterial : m_RedMaterial;
		IsRed = !IsRed;
	}

	/// <summary>
	/// Moves the player left or right one lane, if applicable.
	/// </summary>
	/// <param name="value">Move left (false) or right (true)</param>
	private void ChangeLane(bool value)
	{
		if (value)
			m_CharController.Move(new Vector3(3.0f, 0.0f, 0.0f));
		else
			m_CharController.Move(new Vector3(-3.0f, 0.0f, 0.0f));
	}
	#endregion
}

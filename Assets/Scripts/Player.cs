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
	#endregion

	#region Private
	// -- Editable in Inspector --
	[Header("Movement")]
	[SerializeField] float m_Speed = 10.0f;
	[SerializeField] float m_Drag = 1.0f;
	[SerializeField] float m_Gravity = -9.81f;

	// -- Cached Components
	private PlayerInput m_Input = null;
	private CharacterController m_CharController = null;
	private AudioSource m_AudioSource = null;

	// -- Input --
	private Vector2 m_InputMove = Vector2.zero;
	private bool m_InputJump = false;
	private bool m_InputChange = false;

	// -- Misc. --
	private Vector3 m_PlayerMoveVelocity = Vector3.zero;
	private Vector3 m_ForceMoveVelocity = Vector3.zero;
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
		m_Input = GetComponent<PlayerInput>();
		m_CharController = GetComponent<CharacterController>();
		m_AudioSource = GetComponent<AudioSource>();
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
				// Do something when touch block
			}
		}
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
		Vector3 moveVec = new Vector3(m_InputMove.x, 0.0f, m_InputMove.y) * m_Speed;

		// Add velocity and slight downwards force so Move always does something (fixes RB collisions)
		moveVec += m_ForceMoveVelocity;
		moveVec = new Vector3(moveVec.x, moveVec.y - Time.deltaTime, moveVec.z);
		if (moveVec != Vector3.zero)
		{
			m_CharController.Move(moveVec * Time.deltaTime);
		}
		#endregion
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
	/// Called on Player invoking the 'Jump' PlayerAction.
	/// Stores button held state in a private variable, and queues a jump as required.
	/// </summary>
	/// <param name="value">Information returned on that action by the Input System</param>
	public void OnJumpInput(InputAction.CallbackContext value)
	{
		// Store button held state
		m_InputJump = (value.started ? true : value.canceled ? false : m_InputJump);
	}

	/// <summary>
	/// Called on Player invoking the 'Change' PlayerAction.
	/// </summary>
	/// <param name="value">Information returned on that action by the Input System</param>
	public void OnChangeInput(InputAction.CallbackContext value)
	{
		// Store button held state
		m_InputJump = (value.started ? true : value.canceled ? false : m_InputJump);
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
	#endregion
}

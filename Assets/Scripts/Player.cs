using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
	#region Variables / Properties
	#region Public
	// -- Properties --
	#endregion

	#region Private
	// -- Editable in Inspector --
	[Header("Movement")]
	[SerializeField] float m_Speed = 10.0f;

	// -- Cached Components
	private PlayerInput m_Input = null;
	private CharacterController m_CharController = null;
	private AudioSource m_AudioSource = null;

	// -- Input --
	private Vector2 m_InputMove = Vector2.zero;
	private bool m_InputJump = false;
	private bool m_InputChange = false;

	// -- Misc. --
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
		Vector3 moveVec = new Vector3(m_InputMove.x, 0.0f, m_InputMove.y) * m_Speed;

		if (moveVec != Vector3.zero)
		{
			m_CharController.Move(moveVec * Time.deltaTime);
		}
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
}

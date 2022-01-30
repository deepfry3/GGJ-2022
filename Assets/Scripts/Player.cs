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
	public float JumpTimer { get => m_CanJumpTimer; set => m_CanJumpTimer = value; }
	public float DistanceTravelled { get => m_DistanceTravelled; }
	public float AnimationSpeed { get => m_Animator.speed; set => m_Animator.speed = value; }
	public bool InitialWalkComplete { get; set; } = false;
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
	[SerializeField] SkinnedMeshRenderer m_SkinnedRenderer = null;
	[SerializeField] MeshRenderer[] m_Renderers = null;
	[SerializeField] GameObject m_Model = null;
	[SerializeField] Spinner m_CogSpinner = null;
	[SerializeField] Texture m_RedTex = null;
	[SerializeField] Texture m_BlueTex = null;
	[Header("Audio")]
	[SerializeField] AudioClip[] m_DeathSounds = null;
	[SerializeField] AudioClip m_TrickSound = null;

	// -- Cached Components
	private PlayerInput m_Input = null;
	private CharacterController m_CharController = null;
	private AudioSource m_AudioSource = null;
	private MeshRenderer m_Renderer = null;
	private Animator m_Animator = null;

	// -- Input --
	private Vector2 m_InputMove = Vector2.zero;
	private bool m_InputChange = false;

	// -- Misc. --
	private Vector3 m_PlayerMoveVelocity = Vector3.zero;
	private float m_StartSpeed = 0.0f;
	private float m_DeathTimer = -1.0f;
	private bool m_AttachedToBox = false;
	private float m_CanJumpTimer = 2.5f;
	private float m_DistanceTravelled = 0.0f;
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
		m_Animator = GetComponentInChildren<Animator>();

		// Initialize variables
		m_StartSpeed = m_Speed;
	}

	void Update()
	{
		#region States
		// Ensure x velocity is never not 0
		if (m_ForceMoveVelocity.x != 0.0f)
			m_ForceMoveVelocity = new Vector3(0.0f, m_ForceMoveVelocity.y, m_ForceMoveVelocity.z);
		if (transform.position.x != -3.0f && transform.position.x != 0.0f && transform.position.x != 3.0f)
		{
			// Find closest
			float laneDiff1 = Mathf.Abs(transform.position.x - -3.0f);
			float laneDiff2 = Mathf.Abs(transform.position.x);
			float laneDiff3 = Mathf.Abs(transform.position.x - 3.0f);
			float min = Mathf.Min(laneDiff1, laneDiff2, laneDiff3);
			if (min == laneDiff1)
				m_CharController.Move(new Vector3(-laneDiff1, 0.0f, 0.0f));
			else if (min == laneDiff2)
				m_CharController.Move(new Vector3(transform.position.x > 0.0f ? -laneDiff2 : laneDiff2, 0.0f, 0.0f));
			else if (min == laneDiff3)
				m_CharController.Move(new Vector3(laneDiff3, 0.0f, 0.0f));
		}

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
				m_Animator.speed = 0.0f;
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

		// Update speed and jump timer
		m_Speed += Time.deltaTime / 3.0f;
		if (m_CanJumpTimer > 0.0f)
		{
			m_CanJumpTimer -= Time.deltaTime;
			if (m_CanJumpTimer < 0.0f)
				m_CanJumpTimer = 0.0f;
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

		#region Trick
		//if (m_ForceMoveVelocity.y > 20.0f && m_Model.transform.localEulerAngles.y == 0.0f)
		//{
		//	m_AudioSource.PlayOneShot(m_TrickSound);
		//	m_Model.transform.localEulerAngles = new Vector3(m_Model.transform.localEulerAngles.x, 0.01f, m_Model.transform.localEulerAngles.z);
		//}

		if (m_Model.transform.localEulerAngles.y != 0.0f)
		{
			float y = Mathf.Lerp(m_Model.transform.localEulerAngles.y, 360.0f, Time.deltaTime * 4.0f);
			if (y > 359.5f)
				y = 0.0f;
			m_Model.transform.localEulerAngles = new Vector3(m_Model.transform.localEulerAngles.x, y, m_Model.transform.localEulerAngles.z);
		}

		#region Travelled
		if (m_DeathTimer < 0.0f)
			m_DistanceTravelled = transform.position.z;
		#endregion

		#region Animation
		if (AnimationSpeed > 0.0f && InitialWalkComplete)
			AnimationSpeed = Mathf.Lerp(AnimationSpeed, 0.15f, Time.deltaTime * 1.15f);
		#endregion

		#endregion

		#region Death
		if (transform.position.y < -10.0f && m_ForceMoveVelocity.y < -10.0f && m_DeathTimer == -1.0f)
		{
			m_DeathTimer = 1.75f;
			m_AudioSource.PlayOneShot(m_DeathSounds[Random.Range(0, m_DeathSounds.Length)]);
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
		// Early-exit
		if (Time.timeScale == 0.0f)
			return;

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
		// Early-exit
		if (Time.timeScale == 0.0f)
			return;

		if (value.started)
			ChangeLane(true);
	}

	/// <summary>
	/// Called on Player invoking the 'Change' PlayerAction.
	/// </summary>
	/// <param name="value">Information returned on that action by the Input System</param>
	public void OnChangeInput(InputAction.CallbackContext value)
	{
		if (value.started && Time.timeScale == 0.0f)
			GameManager.Instance.SkipTutorial();
		else if (value.started)
			TogglePolarity();
	}

	/// <summary>
	/// Called on Player invoking the 'Jump' PlayerAction.
	/// </summary>
	/// <param name="value">Information returned on that action by the Input System</param>
	public void OnJumpInput(InputAction.CallbackContext value)
	{
		// Early-exit
		if (Time.timeScale == 0.0f)
			return;

		if (value.started && !IsGrounded && m_CanJumpTimer == 0.0f)
		{
			// Force values
			float y = 6.5f, z = 0.75f;

			// Apply stronger/weaker jump if falling/rising fast
			if (m_ForceMoveVelocity.y < -8.0f)
				y -= (m_ForceMoveVelocity.y + 8.0f) * 1.25f;
			else if (m_ForceMoveVelocity.y > 8.0f)
				y -= (m_ForceMoveVelocity.y - 8.0f) * 1.25f;

			// Apply force and reset timer
			m_ForceMoveVelocity += new Vector3(0.0f, y, z);
			m_CanJumpTimer = -1.0f;

			// Start trick
			m_AudioSource.PlayOneShot(m_TrickSound);
			m_Model.transform.localEulerAngles = new Vector3(m_Model.transform.localEulerAngles.x, 0.01f, m_Model.transform.localEulerAngles.z);
		}
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
		m_DistanceTravelled = 0.0f;
		AnimationSpeed = 1.0f;
		InitialWalkComplete = false;
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
		Debug.Log("Pressed Polarity");

		// Change polarity
		//m_Renderer.material = IsRed ? m_BlueMaterial : m_RedMaterial;
		//m_SkinnedRenderer.materials[1] = IsRed ? m_BlueMaterial : m_RedMaterial;
		m_SkinnedRenderer.materials[1].mainTexture = IsRed ? m_BlueTex : m_RedTex;
		m_CogSpinner.transform.GetComponent<Renderer>().material.mainTexture = IsRed ? m_BlueTex : m_RedTex;

		//for (int i = 0; i < m_Renderers.Length; i++)
		//	m_Renderers[i].material = IsRed ? m_BlueMaterial : m_RedMaterial;
		IsRed = !IsRed;

		// Update cog-spin direction
		m_CogSpinner.SpinSpeed = new Vector3(0.0f, 0.0f, IsRed ? 250.0f : -250.0f);
	}

	/// <summary>
	/// Moves the player left or right one lane, if applicable.
	/// </summary>
	/// <param name="value">Move left (false) or right (true)</param>
	private void ChangeLane(bool value)
	{
		if (value && transform.position.x < 3.0f)
			m_CharController.Move(new Vector3(3.0f, 0.0f, 0.0f));
		else if (!value && transform.position.x > -3.0f)
			m_CharController.Move(new Vector3(-3.0f, 0.0f, 0.0f));
	}
	#endregion
}

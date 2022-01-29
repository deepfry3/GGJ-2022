using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
	#region Variables / Properties
	#region Properties
	public Vector3 SpinSpeed { get => m_SpinSpeed; set => m_SpinSpeed = value; }
	#endregion

	#region Private
	// -- Editable in Inspector --
	[Header("Movement")]
	[SerializeField] Vector3 m_SpinSpeed = Vector3.zero;
	[SerializeField] Vector3 m_MaxSpinSpeed = Vector3.zero;
	[SerializeField] Vector3 m_MinSpinSpeed = Vector3.zero;
	#endregion
	#endregion

	// Update is called once per frame
	void Update()
    {
		// Contain to max/min
		float x, y, z;
		x = m_SpinSpeed.x > m_MaxSpinSpeed.x ? Mathf.Lerp(m_SpinSpeed.x, m_MaxSpinSpeed.x, Time.deltaTime * 1.5f) : m_SpinSpeed.x < m_MinSpinSpeed.x ? Mathf.Lerp(m_SpinSpeed.x, m_MinSpinSpeed.x, Time.deltaTime * 1.5f) : m_SpinSpeed.x;
		y = m_SpinSpeed.y > m_MaxSpinSpeed.y ? Mathf.Lerp(m_SpinSpeed.y, m_MaxSpinSpeed.y, Time.deltaTime * 1.5f) : m_SpinSpeed.y < m_MinSpinSpeed.y ? Mathf.Lerp(m_SpinSpeed.y, m_MinSpinSpeed.y, Time.deltaTime * 1.5f) : m_SpinSpeed.y;
		z = m_SpinSpeed.z > m_MaxSpinSpeed.z ? Mathf.Lerp(m_SpinSpeed.z, m_MaxSpinSpeed.z, Time.deltaTime * 1.5f) : m_SpinSpeed.z < m_MinSpinSpeed.z ? Mathf.Lerp(m_SpinSpeed.z, m_MinSpinSpeed.z, Time.deltaTime * 1.5f) : m_SpinSpeed.z;
		m_SpinSpeed = new Vector3(x, y, z);

		// Perform rotation
		transform.Rotate(m_SpinSpeed * Time.deltaTime);
    }
}

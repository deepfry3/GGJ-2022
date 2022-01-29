using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
	#region Unity Functions
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

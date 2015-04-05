/**
 * Camera orbit controls.
 */

using UnityEngine;
using System.Collections;

public class CameraControls : MonoBehaviour
{
	const string INPUT_MOUSE_X = "Mouse X";
	const string INPUT_MOUSE_Y = "Mouse Y";

	public float orbitSpeed = 10f;
	float distance = 0f;

	void Start()
	{
		distance = Vector3.Distance(transform.position, Vector3.zero);
	}

	void LateUpdate()
	{
		if( Input.GetMouseButton(0) )
		{
			float rot_x = Input.GetAxis(INPUT_MOUSE_X);
			float rot_y = -Input.GetAxis(INPUT_MOUSE_Y);

			Vector3 eulerRotation = transform.localRotation.eulerAngles;

			eulerRotation.x += rot_y * orbitSpeed;
			eulerRotation.y += rot_x * orbitSpeed;

			eulerRotation.z = 0f;

			transform.localRotation = Quaternion.Euler( eulerRotation );
			transform.position = transform.localRotation * (Vector3.forward * -distance);
		}
	}
}

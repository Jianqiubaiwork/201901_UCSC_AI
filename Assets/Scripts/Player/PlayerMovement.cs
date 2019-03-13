using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
	public Rigidbody playerRigidbody;
	public bool isInAction = false;
	public bool isHit = false;
	public bool isTerminating = false;

	Animator anim;
	Vector3 direction;
	const int PHASE_CAP = 10;
	int phase = 0;
	int floorMask;
	int mazeMask;
	float camRayLength = 100f;

	void Awake()
	{
		floorMask = LayerMask.GetMask ("Floor");
		mazeMask = LayerMask.GetMask ("Maze");
		anim = GetComponent <Animator> ();
		playerRigidbody = GetComponent<Rigidbody> ();
		Vector3 offset = new Vector3(0.5f, 0f, 0.5f);
		playerRigidbody.MovePosition (transform.position + offset);
	}

	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag=="Hellephant")
		{
			isTerminating = true;
		}
		else
		{
			isTerminating = false;
		}
	}

	public void Move (int h, int v, int movementStepSize)
	{
		direction.Set (h, 0, v);
		Ray ray = new Ray(transform.position, direction);
		RaycastHit hit;

		if (direction != Vector3.zero) 
		{
			playerRigidbody.MoveRotation (Quaternion.LookRotation(direction));
		}
			
		if (h == 0 && v == 0)
		{
			isHit = true;
			isInAction = false;
			playerRigidbody.MovePosition(transform.position);
		}
		else if (Physics.Raycast(ray, out hit, movementStepSize, mazeMask) && phase==0)
		{
			isHit = true;
			isInAction = false;
			playerRigidbody.MovePosition(transform.position);
		}
		else
		{
			isHit = false;
			isInAction = true;
			playerRigidbody.MovePosition (transform.position + direction*movementStepSize/PHASE_CAP);
			phase += 1;
			if (phase == PHASE_CAP)
			{
				isInAction = false;
				phase = 0;
			}
		}
	}

	void Turning ()
	{
		Ray camRay = Camera.main.ScreenPointToRay (Input.mousePosition);

		RaycastHit floorHit;

		if (Physics.Raycast (camRay, out floorHit, camRayLength, floorMask))
		{
			Vector3 playerToMouse = floorHit.point - transform.position;
			playerToMouse.y = 0f;

			Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
			playerRigidbody.MoveRotation (newRotation);
		}
	}

	public void Animating(float h, float v)
	{
		bool walking = h != 0f || v!= 0f;
		anim.SetBool ("IsWalking", walking);
	}
}

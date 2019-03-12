using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public Rigidbody playerRigidbody;

	Vector3 direction;
	Vector3 phase = new Vector3(0f, 0f, 0f);
	Animator anim;
	int floorMask;
	float camRayLength = 100f;
	float h = -1;
	float v = -1;
	bool isChangingAction = true;
	bool isCollison = false;

	void Awake()
	{
		floorMask = LayerMask.GetMask ("Floor");
		anim = GetComponent <Animator> ();
		playerRigidbody = GetComponent<Rigidbody> ();
		Vector3 offset = new Vector3(0.5f, 0f, 0.5f);
		playerRigidbody.MovePosition (transform.position + offset);
	}

	void OnCollisonEnter(Collision col)
	{
		Debug.Log(col.gameObject.tag);
	}

	void FixedUpdate()
	{
		if (MenuManager.Instance.GAME_MODE == 1)
		{
			if (isChangingAction)
			{
				//Debug.Log("In PlayerMovement, isChangingAction: " + isChangingAction);
				h = Input.GetAxisRaw("Horizontal");
				v = Input.GetAxisRaw("Vertical");
			}
			Move(h, v, out isCollison, out isChangingAction, 5.0f);
		}
	}

	public void Move (float h, float v, out bool isCollison, out bool isChangingAction, float stepSize)
	{
		isCollison = false;
		isChangingAction = true;
		direction.Set (h, 0f, v);
		direction = direction.normalized*stepSize;
		Ray ray = new Ray(transform.position, direction);
		RaycastHit hit;
		if (phase.magnitude != 0f || !Physics.Raycast(ray, out hit, direction.magnitude))
		{
			isChangingAction = false;
			playerRigidbody.MovePosition (transform.position + direction*0.1f);
			phase += direction*0.1f;
			if (phase.magnitude == stepSize)
			{
				isChangingAction = true;
				//Debug.Log("Unit movement made! Action changing!");
				phase.Set (0f, 0f, 0f);
			}
		}
		else
		{
			//Debug.Log("Will hit in " + (direction.magnitude - phase.magnitude) + " ! Action changing!");
			isCollison = true;
			isChangingAction = true;
			playerRigidbody.MovePosition(transform.position);

		}
		if (direction != Vector3.zero) 
		{
			playerRigidbody.MoveRotation (Quaternion.LookRotation(direction));
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoverboard : MonoBehaviour {
	[Header("Flying")]
	[SerializeField] float maxRaycastLen = 0.3f;
	[SerializeField] float flyingHeight = 0.2f;
	[SerializeField] float flyForce = 11.0f;

	[Header("Rotation")]
	[SerializeField] float rotationStr = 1;

	[Header("Moving")]
	[SerializeField] float moveAcl = 1;

	[Header("Refs")]
	[SerializeField] Rigidbody rb;
	[SerializeField] ParticleSystem[] smogs;
	[SerializeField] Transform[] raycastPos;
	[SerializeField] Animator animatorHoverboard;
	[SerializeField] Vector3 centreOfMass;

	bool isTunnOff = true;
	float currForce;
	float currTurn;

	RaycastHit[] raycastHit;
	bool[] isRaycastHit;
	bool pressW;

	Vector3 velocity = Vector3.zero;

	void Awake() {
		raycastHit = new RaycastHit[raycastPos.Length];
		isRaycastHit = new bool[raycastPos.Length];
		rb.centerOfMass = centreOfMass;
	}

	void Update() {
		bool isSpacePressed = Input.GetKey(KeyCode.Space);
		if (isTunnOff != isSpacePressed) {
			isTunnOff = isSpacePressed;
			if (isTunnOff)
				EnableRagdoll();
			else
				DisableRagdoll();
		}

		if (isTunnOff)
			return;

		float vert = Input.GetAxisRaw("Vertical");
		currForce = vert != 0 ? vert * moveAcl : 0;

		float hor = Input.GetAxisRaw("Horizontal");
		currTurn = hor != 0 ? hor * rotationStr : 0;
	}

	void FixedUpdate() {
		if (isTunnOff)
			return;
		byte hittedRaycast = 0;

		for (byte i = 0; i < raycastPos.Length; ++i) {
			isRaycastHit[i] = Physics.Raycast(raycastPos[i].position, -transform.up, out raycastHit[i], maxRaycastLen, LayerMask.GetMask("Environment"));

			if (!isRaycastHit[i]) {
				if (Physics.Raycast(raycastPos[i].position, Vector3.down, out raycastHit[i], maxRaycastLen, LayerMask.GetMask("Environment")))
					rb.AddForceAtPosition(Vector3.up * flyForce * (1.0f - (raycastHit[i].distance / flyingHeight)), raycastPos[i].position);
			}
			else {
				if (isRaycastHit[i]) {
					++hittedRaycast;

					rb.AddForceAtPosition(transform.up * flyForce * (1.0f - (raycastHit[i].distance / flyingHeight)), raycastPos[i].position);
				}
			}

			ProcessSmog(i);

		}

		if(hittedRaycast != 0) {
			//rb.AddForce(Vector3.up * flyForce * (1.0f - (avgLen / flyingHeight)));

			if (currForce != 0)
				rb.AddForce(transform.right * currForce);
		}

		if (currTurn != 0)
			rb.AddRelativeTorque(Vector3.up * currTurn);
	}

	void ProcessSmog(int id) {
		if((isTunnOff || !isRaycastHit[id]) && smogs[0].isPlaying) {
			foreach (var smog in smogs)
					smog.Stop();
		}
		else {
			foreach (var smog in smogs) {
				smog.transform.position = raycastHit[id].point;
				if(smog.isStopped)
					smog.Play();
			}
		}
	}

	void EnableRagdoll() {
		animatorHoverboard.enabled = false;
	}

	void DisableRagdoll() {
		animatorHoverboard.enabled = true;
	}
}

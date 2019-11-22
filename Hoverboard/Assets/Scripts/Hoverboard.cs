using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoverboard : MonoBehaviour {
	[SerializeField] float flyingHeightMax = 0.2f;

	[SerializeField] ParticleSystem[] smogs;

	[SerializeField] Rigidbody rb;

	bool isTunnOff = true;

	void Awake() {

	}

	void Update() {
		bool isSpacePressed = Input.GetKey(KeyCode.Space);
		if(isTunnOff != isSpacePressed) {
			isTunnOff = isSpacePressed;
			if (isTunnOff)
				EnableRagdoll();
			else
				DisableRagdoll();
		}
	}

	void FixedUpdate() {
		if (isTunnOff)
			return;

		RaycastHit hit;
		if (Physics.Raycast(transform.position, -transform.up, out hit, flyingHeightMax, LayerMask.GetMask("Environment"))) {
			foreach (var smog in smogs) {
				smog.transform.position = new Vector3(smog.transform.position.x, hit.point.y, smog.transform.position.z);
				if(smog.isStopped)
					smog.Play();
			}
		}
		else {
			foreach (var smog in smogs)
				smog.Stop();
		}
	}

	void EnableRagdoll() {
		rb.isKinematic = false;
		rb.detectCollisions = true;
	}
	void DisableRagdoll() {
		rb.isKinematic = true;
		rb.detectCollisions = false;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoverboard : MonoBehaviour {
	[Header("Flying")]
	[SerializeField] float maxRaycastLen = 0.3f;
	[SerializeField] float flyingHeightMax = 0.2f;
	[SerializeField] float flyForce = 11.0f;
	[SerializeField] float gravityForce = 9.8f;

	[Header("Rotation")]
	[SerializeField] float rotationSpeed = 1;

	[Header("Moving")]
	[SerializeField] float moveForce = 1;
	[SerializeField] float moveForceMax = 1;
	[SerializeField] float moveDrag = 1;

	[Header("Refs")]
	[SerializeField] Rigidbody rb;
	[SerializeField] ParticleSystem[] smogs;
	[SerializeField] Transform[] raycastPos;
	[SerializeField] Animator animatorHoverboard;

	bool isTunnOff = true;
	float rotationY = 0;

	RaycastHit[] raycastHit;
	bool[] isRaycastHit;
	bool pressW;

	Vector3 velocity = Vector3.zero;

	void Awake() {
		raycastHit = new RaycastHit[raycastPos.Length];
		isRaycastHit = new bool[raycastPos.Length];
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

		if (Input.GetKey(KeyCode.A)) {
			rotationY -= rotationSpeed * Time.deltaTime;
		}
		else if (Input.GetKey(KeyCode.D)) {
			rotationY += rotationSpeed * Time.deltaTime;
		}

		pressW = Input.GetKey(KeyCode.W);
	}

	void FixedUpdate() {
		Vector3 avgNormal = Vector3.zero;
		float avgLen = 0;
		byte hittedRaycast = 0;

		for (byte i = 0; i < raycastPos.Length; ++i) {
			isRaycastHit[i] = Physics.Raycast(raycastPos[i].position, Vector3.down, out raycastHit[i], maxRaycastLen, LayerMask.GetMask("Environment"));

			ProcessSmog(i);

			if (isRaycastHit[i]) {
				avgNormal += raycastHit[i].normal;
				avgLen += raycastHit[i].distance;
				++hittedRaycast;
			}
		}
		avgNormal /= hittedRaycast;
		avgLen /= hittedRaycast;

		Vector3 euler = Quaternion.LookRotation(Vector3.forward, avgNormal).eulerAngles;
		euler.y = rotationY;
		transform.rotation = Quaternion.Euler(euler);

		if (pressW) {
			velocity += moveForce * Time.fixedDeltaTime * Vector3.right;
			if (velocity.x > moveForceMax)
				velocity.x = moveForceMax;
		}

		if (avgLen < flyingHeightMax) {
			velocity += flyForce * Time.fixedDeltaTime * Vector3.up;
		}

		transform.Translate(velocity);

		if (velocity.x != 0) {
			velocity -= moveDrag * Time.fixedDeltaTime * Vector3.right;
			if (velocity.x <= 0)
				velocity.x = 0;
		}

		velocity -= gravityForce * Time.fixedDeltaTime * Vector3.up;
	}

	void ProcessSmog(int id) {
		if((isTunnOff || !isRaycastHit[id]) && smogs[0].isPlaying) {
			foreach (var smog in smogs)
					smog.Stop();
		}
		else {
			foreach (var smog in smogs) {
				smog.transform.position = new Vector3(smog.transform.position.x, raycastHit[id].point.y, smog.transform.position.z);
				if(smog.isStopped)
					smog.Play();
			}
		}
	}

	void EnableRagdoll() {
		rb.isKinematic = false;
		rb.detectCollisions = true;
		animatorHoverboard.enabled = false;
	}

	void DisableRagdoll() {
		rb.isKinematic = true;
		rb.detectCollisions = false;
		animatorHoverboard.enabled = true;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {
	public float speed;
	public Vector3 direction = Vector3.zero;// Use this for initialization
	
	// Update is called once per frame
	void Update () 
	{
		transform.Rotate(direction * Time.deltaTime * speed);
	}
}

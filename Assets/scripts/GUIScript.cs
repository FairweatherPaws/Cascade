using UnityEngine;
using System.Collections;

public class GUIScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        GetComponent<Renderer>().material.mainTextureOffset += new Vector2(Time.deltaTime*0.01f, Time.deltaTime*0.01f);
	}
}

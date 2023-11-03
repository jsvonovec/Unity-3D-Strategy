﻿using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TextureProjector : MonoBehaviour {
  Camera projectorCam;
	void Start () {
    projectorCam = GetComponent<Camera>();
    Shader.SetGlobalMatrix("_projectorMatrix", projectorCam.projectionMatrix * projectorCam.worldToCameraMatrix);
  }
  private void OnValidate() { Start(); }
  private void OnDrawGizmosSelected() { LateUpdate(); }
  void LateUpdate() {
    Shader.SetGlobalMatrix("_projectorMatrix", projectorCam.projectionMatrix * projectorCam.worldToCameraMatrix);
  }
}
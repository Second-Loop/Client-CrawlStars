using System;
using UnityEngine;
using Utility;

namespace CameraControl {
    public class CameraController : MonoBehaviour {
        public Transform TargetPlayer { private get; set; }

        private void LateUpdate() {
            if (TargetPlayer == null) return;
            
            transform.position = TargetPlayer.position + Vector3.back * 10f;
        }
    }
}
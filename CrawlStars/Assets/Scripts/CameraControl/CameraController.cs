using UnityEngine;

namespace CameraControl {
    public class CameraController : MonoBehaviour {
        public Transform TargetPlayer { get; set; }

        private void Update() {
            if (!GameManager.Instance.AmIDead) return;

            if (Input.GetMouseButtonDown(0)) {
                SpectateManager.Instance.SwitchSpectatingPlayer();
            }
        }

        private void LateUpdate() {
            if (TargetPlayer == null) return;
            
            transform.position = TargetPlayer.position + Vector3.back * 10f;
        }
    }
}
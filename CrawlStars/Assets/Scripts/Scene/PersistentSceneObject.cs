using UnityEngine;

namespace Managing {
    [DisallowMultipleComponent]
    public sealed class PersistentSceneObject : MonoBehaviour {
        private void Awake() {
            DontDestroyOnLoad(gameObject);
        }
    }
}

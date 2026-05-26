using System.Collections.Generic;
using UnityEngine;

namespace Utility {
    public class ObjectPooling : MonoBehaviour {
        private static ObjectPooling instance;
        public static ObjectPooling Instance => instance;

        private static readonly Dictionary<string, Queue<GameObject>> objectPool =
            new Dictionary<string, Queue<GameObject>>();

        private void Awake() {
            if (instance != null) {
                Debug.LogError($"{nameof(ObjectPooling)} Instance가 {name}에서 중복 초기화 시도되었습니다.");
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public GameObject Get(string objName, Transform parent = null) {
            if (!objectPool.TryGetValue(objName, out var pool)) {
                pool = new Queue<GameObject>();
                objectPool.Add(objName, pool);
            }

            GameObject go;
            if (pool.Count == 0) {
                go = LoadPrefab(objName);
            } else {
                go = pool.Dequeue();
            }

            if (parent != null && go != null) {
                go.transform.SetParent(parent);
            }

            go.gameObject.SetActive(true);
            return go;
        }

        public T Get<T>(string objName, Transform parent = null) where T : class {
            var obj = Get(objName, parent);
            if (obj == null) return null;

            var component = obj.GetComponent<T>();
            if (component == null) {
                Debug.LogError($"ObjectPooling.Get<T>::해당 컴포넌트가 {objName}에 없습니다.");
            }

            return component;
        }

        public bool TryAbandon(string objName, GameObject go) {
            if (!objectPool.TryGetValue(objName, out var pool)) {
                Debug.LogError($"ObjectPooling.TryAbandon::{objName}이 Pool에 등록되어있지 않습니다. 오브젝트를 Destroy 합니다.");
                Destroy(go);
                return false;
            }

            go.transform.SetParent(transform);
            go.gameObject.SetActive(false);
            pool.Enqueue(go);
            return true;
        }

        public void ClearAll() {
            foreach (var pool in objectPool) {
                while (pool.Value.Count > 0) {
                    var obj = pool.Value.Dequeue();
                    Destroy(obj);
                }
            }

            objectPool.Clear();
        }

        public void ClearPool(string objName) {
            if (!objectPool.TryGetValue(objName, out var pool)) {
                Debug.LogError($"ObjectPooling.ClearPool::{objName}이 Pool에 등록되어있지 않습니다.");
                return;
            }

            while (pool.Count > 0) {
                var obj = pool.Dequeue();
                Destroy(obj);
            }

            objectPool.Remove(objName);
        }

        public void WarmUp(string objName, int amount) {
            if (amount == 0) {
                Debug.LogError($"ObjectPooling.WarmUp::amount가 0 이하입니다. {amount}");
                return;
            }

            var obj = LoadPrefab(objName);
            if (obj == null) return;

            if (!objectPool.TryGetValue(objName, out var pool)) {
                pool = new Queue<GameObject>();
                objectPool.Add(objName, pool);
            }

            pool.Enqueue(obj);

            for (int i = 0; i < amount - 1; ++i) {
                pool.Enqueue(LoadPrefab(objName));
            }
        }

        private GameObject LoadPrefab(string objName) {
            var obj = Resources.Load<GameObject>(objName);
            if (obj == null) {
                Debug.LogError($"ObjectPooling.LoadPrefab::{objName}이 Resources 경로에 없습니다.");
                return null;
            }

            return Instantiate(obj, transform, false);
        }
    }
}
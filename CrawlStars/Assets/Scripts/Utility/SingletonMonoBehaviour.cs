using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {
    private static T instance;
    public static T Instance => instance;

    protected virtual void Awake() {
        if (instance != null) {
            Debug.LogError($"{typeof(T)} Instance가 {name}에서 중복 초기화 시도되었습니다.");
            return;
        }

        instance = this as T;
        if (Application.isPlaying) {
            DontDestroyOnLoad(gameObject);
        }
    }
}

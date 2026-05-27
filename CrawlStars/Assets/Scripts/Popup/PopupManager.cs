using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PopupManager {
    private static PopupManager instance;
    public static PopupManager Instance => instance ??= new PopupManager();
    
    private Stack<PopupHandler> popupStack = new Stack<PopupHandler>();

    public void Show(string name) {
        var resource = Resources.Load<PopupHandler>(name);
        if (resource == null) {
            Debug.LogError($"PopupManager.Show::{name} 프리팹을 찾을 수 없습니다.");
            return;
        }

        var obj = Object.Instantiate(resource);
        popupStack.Push(obj);
        obj.gameObject.SetActive(true);
    }

    public void Close() {
        if (popupStack.Count == 0) {
            Debug.LogError($"PopupManager.Close::닫을 팝업이 없습니다.");
            return;
        }

        var popup = popupStack.Pop();
        Object.Destroy(popup.gameObject);
    }
}
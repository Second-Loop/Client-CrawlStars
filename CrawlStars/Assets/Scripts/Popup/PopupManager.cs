using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Popup {
    public class PopupManager {
        private static PopupManager instance;
        public static PopupManager Instance => instance ??= new PopupManager();

        private readonly List<KeyValuePair<string, PopupHandler>> popupList =
            new List<KeyValuePair<string, PopupHandler>>();
        
        public bool HasOpenPopup => popupList.Count > 0;

        public async UniTask<PopupHandler.Result> ShowAsync(string name, PopupHandler.Param param = null) {
            var resource = Resources.Load<PopupHandler>(name);
            if (resource == null) {
                Debug.LogError($"PopupManager.Show::{name} 프리팹을 찾을 수 없습니다.");
                return null;
            }

            var obj = Object.Instantiate(resource);
            popupList.Add(new KeyValuePair<string, PopupHandler>(name, obj));
            obj.SetData(param, popupList.Count * 1000);
            obj.gameObject.SetActive(true);
            return await obj.WaitForCloseAsync();
        }

        public void CloseTop() {
            if (popupList.Count == 0) {
                Debug.LogError($"PopupManager.Close::닫을 팝업이 없습니다.");
                return;
            }

            RemovePopup(popupList[^1].Value, popupList.Count - 1);
        }

        public void CloseByName(string name) {
            // 보통 Top 부터 닫는 경우가 많아서 역으로 순회
            for (int i = popupList.Count - 1; i >= 0; --i) {
                var popup = popupList[i];
                if (popup.Key == name) {
                    RemovePopup(popup.Value, i);
                    return;
                }
            }

            Debug.LogError($"PopupManager.CloseByName::{name}이 열려있지 않습니다.");
        }

        public void Close(PopupHandler target, PopupHandler.Result result) {
            // 보통 Top 부터 닫는 경우가 많아서 역으로 순회
            for (int i = popupList.Count - 1; i >= 0; --i) {
                var popup = popupList[i];
                if (popup.Value == target) {
                    RemovePopup(popup.Value, i, result);
                    return;
                }
            }

            Debug.LogError($"PopupManager.Close::{target.name}이 열려있지 않습니다.");
        }

        public void CloseAll() {
            for (int i = popupList.Count - 1; i >= 0; --i) {
                RemovePopup(popupList[i].Value, i);
            }
        }

        private void RemovePopup(PopupHandler target, int listIdx, PopupHandler.Result result = null) {
            target.Dispose(result);
            Object.Destroy(target.gameObject);
            popupList.RemoveAt(listIdx);
        }
    }
}
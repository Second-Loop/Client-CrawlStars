using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using Popup;
using UnityEngine;
using Utility;

public class ModePopup : PopupHandler {
    [SerializeField] private Transform itemRoot;
    [SerializeField] private GameObject spacer;

    private List<ModeItem> modeItems = new List<ModeItem>();

    public override void SetData(Param param, int sortingOrder) {
        base.SetData(param, sortingOrder);
        
        SetDataAsync().Forget();
    }

    public override void Dispose(Result result = null) {
        foreach (var modeItem in modeItems) {
            modeItem.Release();
            ObjectPooling.Instance.TryAbandon(nameof(ModeItem), modeItem.gameObject);
        }
        modeItems.Clear();
        base.Dispose(result);
    }

    private async UniTaskVoid SetDataAsync() {
        var info = await ModeManager.Instance.GetModeInfoAsync();
        if (info == null) {
            RequestPopupClosing();
            return;
        }

        foreach (var item in info.items) {
            var modeItem = ObjectPooling.Instance.Get<ModeItem>(nameof(ModeItem), itemRoot);
            modeItem.SetData(item, () => RequestPopupClosing());
            modeItems.Add(modeItem);
        }

        spacer.SetActive(info.items.Length < 3);
    }
}

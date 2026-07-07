using System.Collections.Generic;
using Core;
using Popup;
using UnityEngine;
using Utility;

public class ModePopup : PopupHandler {
    [SerializeField] private Transform itemRoot;

    private List<ModeItem> modeItems;

    public override void SetData(Param param, int sortingOrder) {
        base.SetData(param, sortingOrder);
        
        var items = ModeManager.Instance.ModeInfo.items;

        foreach (var item in items) {
            var modeItem = ObjectPooling.Instance.Get<ModeItem>(nameof(ModeItem), itemRoot);
            modeItem.SetData(item);
            modeItems.Add(modeItem);
        }
    }

    public override void Dispose(Result result = null) {
        foreach (var modeItem in modeItems) {
            ObjectPooling.Instance.TryAbandon(nameof(ModeItem), modeItem.gameObject);
        }
        modeItems.Clear();
    }
}

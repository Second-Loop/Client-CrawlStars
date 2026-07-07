using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using Popup;
using UnityEngine;
using Utility;

public class CharacterPopup : PopupHandler {
    [SerializeField] private Transform itemRoot;
    [SerializeField] private GameObject spacer;

    private readonly List<CharacterItem> characterItems = new List<CharacterItem>();

    public override void SetData(Param param, int sortingOrder) {
        base.SetData(param, sortingOrder);

        SetDataAsync().Forget();
    }

    public override void Dispose(Result result = null) {
        foreach (var characterItem in characterItems) {
            characterItem.Release();
            ObjectPooling.Instance.TryAbandon(nameof(CharacterItem), characterItem.gameObject);
        }

        characterItems.Clear();
        base.Dispose(result);
    }

    private async UniTaskVoid SetDataAsync() {
        var info = await CharacterManager.Instance.GetCharacterInfoAsync();
        if (info == null) {
            RequestPopupClosing();
            return;
        }

        foreach (var item in info.items) {
            var characterItem = ObjectPooling.Instance.Get<CharacterItem>(nameof(CharacterItem), itemRoot);
            characterItem.SetData(item, () => RequestPopupClosing());
            characterItems.Add(characterItem);
        }

        spacer.SetActive(info.items.Length < 3);
    }
}

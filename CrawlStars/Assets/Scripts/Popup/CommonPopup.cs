using TMPro;
using UnityEngine;

namespace Popup {
    public class CommonPopup : PopupHandler {
        [SerializeField] protected TextMeshProUGUI descriptionText;

        public class Param : PopupHandler.Param {
            public string title;
            public string description;
            public Param(string title, string description) {
                this.title = title;
                this.description = description;
            }
        }

        public override void SetData(PopupHandler.Param param, int sortingOrder) {
            base.SetData(param, sortingOrder);

            var myParam = param as Param;
            if (myParam == null) {
                Debug.LogError("CommonTwoButtonPopup.SetData::param is not CommonTwoButtonPopup.Param");
                return;
            }

            titleText.text = myParam.title;
            descriptionText.text = myParam.description;
        }
    }
}
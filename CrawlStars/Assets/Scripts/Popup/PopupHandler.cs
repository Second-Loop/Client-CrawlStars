using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public abstract class PopupHandler : MonoBehaviour {
    [SerializeField] protected TextMeshProUGUI titleText;
    [SerializeField] protected Button quitButton;

    protected void Start() {
        quitButton.onClick.AddListener(OnClickQuit);
    }

    protected void OnDestroy() {
        quitButton.onClick.RemoveListener(OnClickQuit);
    }

    public virtual void SetData() {
        
    }

    protected virtual void OnClickQuit() {
        PopupManager.Instance.Close();
    }
}

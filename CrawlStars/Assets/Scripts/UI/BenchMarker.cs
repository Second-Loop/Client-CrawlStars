using System.Collections.Generic;
using System.Linq;
using Core.Player;
using DG.Tweening;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BenchMarker : MonoBehaviour {
    [SerializeField] private Image keyW;
    [SerializeField] private Image keyA;
    [SerializeField] private Image keyS;
    [SerializeField] private Image keyD;
    [SerializeField] private Image mouse;
    [SerializeField] private TextMeshProUGUI latencyText;
    [SerializeField] private TextMeshProUGUI lossText;
    [SerializeField] private Image effect;
    
    private readonly Queue<double> inputQue = new Queue<double>();
    private int inputCount = 0;
    private int lostCount = 0;
    
    private const float ColorDuration = 0.3f;
    private const float LossThreshold = 3000f;

    public void OnPressKey(Vector2 moveDir, Vector2 attackDir) {
        if (moveDir == Vector2.zero && attackDir == Vector2.zero) return;

        if (moveDir.x < 0) TurnRedToWhite(keyA);
        else if (moveDir.x > 0) TurnRedToWhite(keyD);

        if (moveDir.y < 0) TurnRedToWhite(keyS);
        else if (moveDir.y > 0) TurnRedToWhite(keyW);

        if (attackDir != Vector2.zero) TurnRedToWhite(mouse);

        inputQue.Enqueue(Time.realtimeSinceStartupAsDouble);
        ++inputCount;
    }

    public void OnReceiveSnapshot(SnapshotDto snapshot) {
        if (snapshot?.Players == null) {
            Debug.LogWarning("BenchMark.OnReceiveSnapshot::snapshot players is null");
            return;
        }

        var me = snapshot.Players.FirstOrDefault(data => data.Id == PlayerManager.Instance.MyId);
        if (me == null) {
            Debug.LogError("BenchMark.OnReceiveSnapshot::Can not find my data in snapshot");
            return;
        }

        double elapsedMs = -1.0;
        bool isLost = false;

        while (inputQue.Count > 0) {
            var inputTime = inputQue.Peek();
            elapsedMs = (Time.realtimeSinceStartupAsDouble - inputTime) * 1000.0;
            if (elapsedMs < LossThreshold) break;

            inputQue.Dequeue();
            ++lostCount;
            isLost = true;
        }

        if (isLost) {
            effect.DOKill();
            effect.color = Color.red;
            effect.DOFade(0f, ColorDuration);
        }

        UpdateLossText();

        if (inputQue.Count == 0 || elapsedMs < 0 || 
            me.MoveDir.ToVector2() == Vector2.zero && me.AttackDir.ToVector2() == Vector2.zero) return;

        inputQue.Dequeue();

        latencyText.text = $"{elapsedMs:F2} ms"; 
        UpdateLossText();
    }

    private void UpdateLossText() {
        float lossRate = inputCount == 0 ? 0f : lostCount / (float)inputCount * 100f;
        lossText.text = $"{lossRate:F2} %";
    }

    private static void TurnRedToWhite(Image target) {
        target.DOKill();
        target.color = Color.red;
        target.DOColor(Color.white, ColorDuration);
    }
}

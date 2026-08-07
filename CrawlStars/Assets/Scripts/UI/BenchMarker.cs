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
    [SerializeField] private TextMeshProUGUI timeoutRateText;
    [SerializeField] private Image effect;

    private readonly InputAckTracker inputAckTracker = new InputAckTracker();
    
    private const float ColorDuration = 0.3f;

    private void Awake() {
        latencyText.text = "-";
        UpdateTimeoutRateText();
    }

    private void Update() {
        var expiredCount = inputAckTracker.CheckExpiration(Time.realtimeSinceStartupAsDouble);
        if (expiredCount <= 0) return;

        PlayTimeoutEffect();
        UpdateTimeoutRateText();
    }

    public void OnInputSubmitted(InputMessageDto input) {
        if (input == null) return;

        var moveDir = input.MoveDir.ToVector2();
        var attackDir = input.AttackDir.ToVector2();

        if (moveDir.x < 0) TurnRedToWhite(keyA);
        else if (moveDir.x > 0) TurnRedToWhite(keyD);

        if (moveDir.y < 0) TurnRedToWhite(keyS);
        else if (moveDir.y > 0) TurnRedToWhite(keyW);

        if (attackDir != Vector2.zero) TurnRedToWhite(mouse);

        if (inputAckTracker.TryRecord(input.ClientTick, Time.realtimeSinceStartupAsDouble)) {
            UpdateTimeoutRateText();
        }
    }

    public void OnReceiveSnapshot(SnapshotDto snapshot) {
        if (snapshot?.Players == null) return;

        var me = snapshot.Players.FirstOrDefault(data => data.Id == PlayerManager.Instance.MyId);
        if (me == null) {
            Debug.LogError("BenchMark.OnReceiveSnapshot::Can not find my data in snapshot");
            return;
        }

        // 응답 처리 전에 타임아웃 요소들 먼저 처리, Update보다 먼저 호출될 수 있기 때문
        var curTime = Time.realtimeSinceStartupAsDouble;
        var expiredCount = inputAckTracker.CheckExpiration(curTime);
        if (expiredCount > 0) {
            PlayTimeoutEffect();
        }

        if (inputAckTracker.TryAcknowledge(me.LastProcessedClientTick, curTime, out var latencyMs)) {
            latencyText.text = $"{latencyMs:F2} ms";
        }
        UpdateTimeoutRateText();
    }

    private void UpdateTimeoutRateText() {
        timeoutRateText.text = $"{inputAckTracker.TimeoutRate * 100.0:F2} %";
    }

    private void PlayTimeoutEffect() {
        effect.DOKill();
        effect.color = Color.red;
        effect.DOFade(0f, ColorDuration);
    }

    private static void TurnRedToWhite(Image target) {
        target.DOKill();
        target.color = Color.red;
        target.DOColor(Color.white, ColorDuration);
    }
}

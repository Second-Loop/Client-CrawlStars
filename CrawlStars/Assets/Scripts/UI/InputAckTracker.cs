using System.Collections.Generic;

public sealed class InputAckTracker {
    private readonly struct PendingInput {
        public long ClientTick { get; }
        public double SubmittedAt { get; }

        public PendingInput(long clientTick, double submittedAt) {
            ClientTick = clientTick;
            SubmittedAt = submittedAt;
        }
    }

    private readonly Queue<PendingInput> pendingInputs = new Queue<PendingInput>();

    private long lastSubmittedTick;
    private long lastAcknowledgedTick;

    public long SubmittedCount { get; private set; }
    public long TimedOutCount { get; private set; }
    public int PendingCount => pendingInputs.Count;
    public double TimeoutRate => SubmittedCount == 0 ? 0.0 : TimedOutCount / (double)SubmittedCount;

    private const double AckTimeoutSeconds = 3.0;

    public bool TryRecord(long clientTick, double submittedAt) {
        if (clientTick <= 0 || clientTick <= lastSubmittedTick || clientTick <= lastAcknowledgedTick) return false;

        pendingInputs.Enqueue(new PendingInput(clientTick, submittedAt));
        lastSubmittedTick = clientTick;
        ++SubmittedCount;
        return true;
    }

    public bool TryAcknowledge(long clientTick, double acknowledgedAt, out double latencyMs) {
        latencyMs = 0.0;
        if (clientTick <= lastAcknowledgedTick) return false;

        lastAcknowledgedTick = clientTick;
        var foundExactTick = false;

        while (pendingInputs.Count > 0 && pendingInputs.Peek().ClientTick <= clientTick) {
            var pendingInput = pendingInputs.Dequeue();

            // 서버가 해당 입력을 처리했지만, 해당 스냅샷이 서버의 latest-only 정책으로 최신 스냅샷에 교체되어 클라이언트가 못 받았을 수 있기 때문에 정상 케이스임
            if (pendingInput.ClientTick != clientTick) continue;

            latencyMs = (acknowledgedAt - pendingInput.SubmittedAt) * 1000.0;
            foundExactTick = true;
        }

        return foundExactTick;
    }

    public int CheckExpiration(double currentTime) {
        var expiredCount = 0;

        while (pendingInputs.Count > 0 && currentTime - pendingInputs.Peek().SubmittedAt >= AckTimeoutSeconds) {
            pendingInputs.Dequeue();
            ++TimedOutCount;
            ++expiredCount;
        }

        return expiredCount;
    }
}

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Utility {
    public static class ErrorHandler {
        private const int Delay = 500;

        /// <summary>
        /// 단순 환경 문제로 실패할 수 있는 메서드를 retryCount 만큼 재시도 함으로써 성공을 기대함 (Addressable, Network 등)
        /// </summary>
        public static async UniTask<bool> DoWithReTryAsync(Func<UniTask<bool>> func, string label, int retryCount) {
            for (int i = 0; i < retryCount; ++i) {
                bool isSucceed;
                try {
                    isSucceed = await func.Invoke();
                } catch (Exception e) {
                    Debug.LogError(e.ToString());
                    continue;
                }

                if (isSucceed) return true;

                await UniTask.Delay(Delay);
            }
            Debug.LogError($"ErrorHandler.DoWithReTryAsync::all {label} retry failed");
            return false;
        }
    }
}
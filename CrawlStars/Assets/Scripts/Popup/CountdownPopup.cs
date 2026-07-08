using System;
using System.Collections.Generic;
using Core;
using Core.Player;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Network;
using TMPro;
using UnityEngine;

namespace Popup {
    public class CountdownPopup : PopupHandler {
        public class Param : PopupHandler.Param {
            public IReadOnlyList<ReadyPlayerDto> players;
            public int? countdownSeconds;
            public Param(IReadOnlyList<ReadyPlayerDto> players, int? countdownSeconds) {
                this.players = players;
                this.countdownSeconds = countdownSeconds;
            }
        }

        [SerializeField] private GameObject countdownGroup;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI startText;

        [SerializeField] private List<PlayerCard> playerCards;
        [SerializeField] private GameObject vsText;

        public override bool CanCloseWithEsc => false;
        
        private const int FallbackSeconds = 5;
        private const int DelayMilliSeconds = 1000;

        public override void SetData(PopupHandler.Param param, int sortingOrder) {
            base.SetData(param, sortingOrder);

            var validParam = param as Param;
            if (validParam == null) {
                Debug.LogError("CountdownPopup.SetData::invalid param");
                RequestPopupClosing();
                return;
            }

            countdownGroup.gameObject.SetActive(false);
            startText.transform.localScale = Vector3.zero;

            SetPlayerCards(validParam.players);

            int countdownSeconds = validParam.countdownSeconds ?? FallbackSeconds;
            StartCountdownAsync(countdownSeconds - 1).Forget();
        }

        private void SetPlayerCards(IReadOnlyList<ReadyPlayerDto> players) {
            if (players == null) {
                TurnOffPlayerCards();
                Debug.LogError("CountdownPopup.SetPlayerCards::players in param is null");
                return;
            }

            bool isSoloMode = ModeManager.Instance.CurGameMode == ModeManager.GameMode.Solo;
            int mySideIdx = 0;
            int otherSideStartIdx = isSoloMode ? 1 : 3;
            int otherSideIdx = otherSideStartIdx;

            vsText.SetActive(!isSoloMode);

            try {
                foreach (var player in players) {
                    bool isMySide = player.Team == PlayerManager.Instance.MyTeam;
                    int idx = isMySide ? mySideIdx++ : otherSideIdx++;
                    playerCards[idx].SetData(PlayerData.CharacterType.A, "Player", isMySide);
                    playerCards[idx].gameObject.SetActive(true);
                }

                // 혹시라도 남은 슬롯은 끄기
                for (int i = mySideIdx; i < otherSideStartIdx; ++i) {
                    playerCards[i].gameObject.SetActive(false);
                }

                for (int i = otherSideIdx; i < playerCards.Count; ++i) {
                    playerCards[i].gameObject.SetActive(false);
                }
            } catch (Exception e) {
                // 서버에서 team 정보 잘 못 줬을 경우 Index OutOfBounds 발생 에러 핸들링
                TurnOffPlayerCards();
                Debug.LogError(e.ToString());
            }
        }

        private void TurnOffPlayerCards() {
            if (playerCards == null) return;

            foreach (var playerCard in playerCards) {
                playerCard.gameObject.SetActive(false);
            }
        }

        private async UniTaskVoid StartCountdownAsync(int countdownSeconds) {
            SetCountDownText(countdownSeconds);
            countdownGroup.gameObject.SetActive(true);

            DateTime endTime = DateTime.UtcNow.AddSeconds(countdownSeconds);
            while (DateTime.UtcNow < endTime) {
                int remainingSeconds = Mathf.CeilToInt((float)(endTime - DateTime.UtcNow).TotalSeconds);
                SetCountDownText(remainingSeconds);
                await UniTask.Delay(DelayMilliSeconds);
            }

            countdownGroup.gameObject.SetActive(false);

            vsText.SetActive(false);
            startText.transform.DOScale(1f, 0.5f);
            await UniTask.Delay(DelayMilliSeconds);
            RequestPopupClosing();
        }

        private void SetCountDownText(int seconds) {
            countdownText.text = $"{seconds} seconds..";
        }
    }
}
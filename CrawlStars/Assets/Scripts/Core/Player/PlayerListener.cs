using Core.Character;
using Network;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Utility;

namespace Core.Player {
    public class PlayerListener : MonoBehaviour {
        [SerializeField] private Transform bodyRoot;
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private StatusBar hpBar;
        [SerializeField] private SpriteRenderer aura;
        [SerializeField] private SpriteRenderer meleeAttackEffect;
        [SerializeField] private float meleeEffectDuration = 0.15f;

        private bool isStatusInitialized;
        private CharacterManager.CharacterType characterType;
        private CancellationTokenSource meleeEffectCancellation;
        private PlayerSpriteAnimator spriteAnimator;

        private static readonly Color32 MyAuraColor = new Color32(23, 212, 29, 150);
        private static readonly Color32 MySideAuraColor = new Color32(0, 198, 255, 150);
        private static readonly Color32 OtherSideAuraColor = new Color32(255, 0, 0, 150);
        
        // 임시
        public float Hp => hpBar.Value;

        public void Initialize(ReadyPlayerDto playerData, bool isMe) {
            isStatusInitialized = false;

            characterType = (CharacterManager.CharacterType)playerData.CharacterType;
            var info = CharacterManager.Instance.GetCharacterInfo(characterType);
            if (info != null) {
                body.sprite = SpriteCacheHelper.Get(info.iconSpriteName);
            }

            spriteAnimator ??= GetComponent<PlayerSpriteAnimator>();
            spriteAnimator ??= gameObject.AddComponent<PlayerSpriteAnimator>();
            spriteAnimator.Initialize(body, characterType.ToString());

            CancelMeleeAttackEffect();

            bool isMySide = playerData.Team == PlayerManager.Instance.MyTeam;

            aura.color = isMe ? MyAuraColor : (isMySide ? MySideAuraColor : OtherSideAuraColor);
            hpBar.gameObject.SetActive(false);
            hpBar.SetColor(isMe, isMySide);

            MoveTo(playerData.SpawnPosition.ToVector2());
        }

        public void ApplyStatus(float hp) {
            int roundedHp = Mathf.RoundToInt(hp);
            if (!isStatusInitialized) {
                hpBar.Initialize(roundedHp);
                isStatusInitialized = true;
                return;
            }

            hpBar.MoveValue(roundedHp);
        }
        
        public void MoveTo(Vector3 position) {
            transform.position = position + Vector3.back;
        }

        public void SetMoving(bool isMoving) {
            spriteAnimator?.SetMoving(isMoving);
        }
        
        public void RotateTo(Vector2 direction) {
            if (direction == Vector2.zero) return;
            if (spriteAnimator != null && spriteAnimator.IsAttacking) return;

            ApplyRotation(direction);
        }

        public void RotateToAttack(Vector2 direction) {
            if (direction == Vector2.zero) return;

            ApplyRotation(direction);
        }

        private void ApplyRotation(Vector2 direction) {
            float angle = MathUtil.GetAngle(direction);
            bodyRoot.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // 공격 모션임
        public void Attack(Vector2 direction) {
            if (direction == Vector2.zero) return;

            spriteAnimator?.PlayAttack();
            if (characterType == CharacterManager.CharacterType.Lily) {
                PlayMeleeAttackEffect();
            }
        }

        private void PlayMeleeAttackEffect() {
            if (meleeAttackEffect == null) return;

            CancelMeleeAttackEffect();
            meleeAttackEffect.gameObject.SetActive(true);
            var destroyToken = gameObject.GetCancellationTokenOnDestroy();
            meleeEffectCancellation = CancellationTokenSource.CreateLinkedTokenSource(destroyToken);
            HideMeleeAttackEffectAsync(meleeEffectCancellation.Token).Forget();
        }

        private async UniTask HideMeleeAttackEffectAsync(CancellationToken cancellationToken) {
            var isCanceled = await UniTask.Delay(
                TimeSpan.FromSeconds(meleeEffectDuration),
                cancellationToken: cancellationToken
            ).SuppressCancellationThrow();
            if (isCanceled) return;

            meleeAttackEffect.gameObject.SetActive(false);
            meleeEffectCancellation.Dispose();
            meleeEffectCancellation = null;
        }

        private void CancelMeleeAttackEffect() {
            meleeEffectCancellation?.Cancel();
            meleeEffectCancellation?.Dispose();
            meleeEffectCancellation = null;

            if (meleeAttackEffect != null) {
                meleeAttackEffect.gameObject.SetActive(false);
            }
        }

        private void OnDisable() {
            CancelMeleeAttackEffect();
        }

        public void BeingHit(float hp) {
            ApplyStatus(hp);
        }
    }
}
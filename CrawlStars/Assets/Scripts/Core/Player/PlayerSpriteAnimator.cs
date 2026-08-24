using System;
using UnityEngine;

namespace Core.Player {
    public class PlayerSpriteAnimator : MonoBehaviour {
        [SerializeField] private float walkFramesPerSecond = 10f;
        [SerializeField] private float attackFramesPerSecond = 14f;

        private SpriteRenderer body;
        private Sprite idleSprite;
        private Sprite[] walkFrames = Array.Empty<Sprite>();
        private Sprite[] attackFrames = Array.Empty<Sprite>();
        private Vector3 idleScale;
        private bool hasIdleScale;
        private bool isMoving;
        private bool isAttacking;
        private int frameIndex;
        private float frameTimer;

        public void Initialize(SpriteRenderer targetBody, string characterName) {
            body = targetBody;
            idleSprite = targetBody != null ? targetBody.sprite : null;
            walkFrames = LoadFrames($"Animations/{characterName}/Walk");
            attackFrames = LoadFrames($"Animations/{characterName}/Attack");
            isMoving = false;
            isAttacking = false;
            frameIndex = 0;
            frameTimer = 0f;

            if (body != null && !hasIdleScale) {
                idleScale = body.transform.localScale;
                hasIdleScale = true;
            }

            ApplySprite(idleSprite);

            enabled = body != null && (walkFrames.Length > 0 || attackFrames.Length > 0);
        }

        public void SetMoving(bool moving) {
            if (isMoving == moving) return;

            isMoving = moving;
            if (!isAttacking) {
                ApplyMovementState();
            }
        }

        public void PlayAttack() {
            if (body == null || attackFrames.Length == 0) return;

            isAttacking = true;
            frameIndex = 0;
            frameTimer = 0f;
            ApplySprite(attackFrames[frameIndex]);
        }

        private void Update() {
            if (isAttacking) {
                AdvanceAttack();
            } else if (isMoving) {
                AdvanceWalk();
            }
        }

        private void AdvanceAttack() {
            float frameDuration = 1f / attackFramesPerSecond;
            frameTimer += Time.deltaTime;

            while (frameTimer >= frameDuration) {
                frameTimer -= frameDuration;
                frameIndex++;
                if (frameIndex >= attackFrames.Length) {
                    isAttacking = false;
                    ApplyMovementState();
                    return;
                }

                ApplySprite(attackFrames[frameIndex]);
            }
        }

        private void AdvanceWalk() {
            if (walkFrames.Length == 0) return;

            float frameDuration = 1f / walkFramesPerSecond;
            frameTimer += Time.deltaTime;

            while (frameTimer >= frameDuration) {
                frameTimer -= frameDuration;
                frameIndex = (frameIndex + 1) % walkFrames.Length;
                ApplySprite(walkFrames[frameIndex]);
            }
        }

        private void ApplyMovementState() {
            frameIndex = 0;
            frameTimer = 0f;

            if (isMoving && walkFrames.Length > 0) {
                ApplySprite(walkFrames[frameIndex]);
            } else {
                ApplySprite(idleSprite);
            }
        }

        private void ApplySprite(Sprite sprite) {
            if (body == null) return;

            body.sprite = sprite;
            body.transform.localScale = idleScale;
        }

        private static Sprite[] LoadFrames(string resourcePath) {
            var frames = Resources.LoadAll<Sprite>(resourcePath);
            Array.Sort(frames, (left, right) => string.CompareOrdinal(left.name, right.name));
            return frames;
        }
    }
}
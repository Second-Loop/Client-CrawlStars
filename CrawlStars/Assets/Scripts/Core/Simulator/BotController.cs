using Core.Player;
using Core.Projectile;
using Cysharp.Threading.Tasks;
using Network;
using UnityEngine;

namespace Core.Simulator {
    /*
     * Explore → 적, 아이템, 목표물을 찾으며 이동
     * Chase → 적 추격
     * Attack → 적 공격
     * Dodge → 공격 회피
     * Retreat -> 도망
     */

    public class BotController {
        private static BotController instance;
        public static BotController Instance => instance ??= new BotController();
        
        private enum State { None, Explore, Chase, Attack, Dodge, Retreat }

        private State state;

        public void Initialize() {
            state = State.None;
        }

        public async UniTask SendInputAsync() {
            (Vector2 moveDirection, Vector2 attackDirection) = Update();

            await NetworkManager.Instance.SendSocketJsonAsync(new InputMessageDto {
                MoveDir = new Vector2Dto(moveDirection),
                AttackDir = new Vector2Dto(attackDirection),
                PressedAttack = attackDirection != Vector2.zero
            });
        }

        private (Vector2 moveDirection, Vector2 attackDirection) Update() {
            var players = PlayerManager.Instance.playerListeners;
            var projectiles = ProjectileManager.Instance.projectileListeners;
            var me = PlayerManager.Instance.myListener;

            switch (state) {
                case State.None:
                    state = State.Explore;
                    break;
                case State.Explore:
                    break;
                case State.Chase:
                    break;
                case State.Retreat:
                    break;
                case State.Dodge:
                    break;
            }

            return (Vector2.zero, Vector2.zero);
        }
    }
}
using System;
using System.Collections.Generic;
using Core.Player;
using UnityEngine;

// 나중에 클라에서도 tick 을 계산해야 하기에 서버로 이주 후에는 이름을 TickManager로 바꾸기
public class Simulator : MonoBehaviour {
    [SerializeField] private InputProvider inputProvider;

    public int TickCount { get; private set; }
    private float accumulator;
    private bool isStarted;

    private const int TickRate = 30;
    private const float TickThreshold = 1f / TickRate;

    private void Update() {
        if (!isStarted) return;

        accumulator += Time.deltaTime;

        if (accumulator >= TickThreshold) {
            Tick();
            accumulator -= TickThreshold;
        }
    }

    public void Initialize() {
        accumulator = 0;
        TickCount = 0;
        isStarted = false;
    }

    public void StartTime() {
        isStarted = true;
    }
    
    // temp data
    private Vector2 playerPos = Vector2.zero;
    private Vector2 playerLookDir = Vector2.zero;
    private const float MoveSpeed = 0.2f;

    private void Tick() {
        // Get player's input
        Vector2 moveDirection = inputProvider.GetMoveDirection();
        Vector2 lookDirection = inputProvider.GetLookingSide();
        bool isPressedAttack = inputProvider.GetAttack();

        // Simulate
        playerPos += moveDirection * MoveSpeed;
        playerLookDir += lookDirection;
        
        // Return to clients
        PlayerManager.Instance.Move(playerPos);
        PlayerManager.Instance.Look(playerLookDir);
        if (isPressedAttack) {
            PlayerManager.Instance.Attack();
        }
    }
}

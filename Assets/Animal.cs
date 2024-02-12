using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AnimalState
{
    Idle,
    Moving,
    Chase,
}

[RequireComponent(typeof(NavMeshAgent))]
public class Animal : MonoBehaviour
{
    [Header("Wander")]
    [SerializeField] private float wanderDistance = 50f; // How far the animal can move in one go.
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float maxWalkTime = 6f;

    [Header("Idle")]
    [SerializeField] private float idleTime = 5f; // How long the animal takes a break for.

    [Header("Chase")]
    [SerializeField] private float runSpeed = 8f;

    [Header("Attributes")]
    [SerializeField] private int health = 10;

    protected NavMeshAgent navMeshAgent;
    protected AnimalState currentState = AnimalState.Idle;

    private void Start()
    {
        InitialiseAnimal();
    }

    protected virtual void InitialiseAnimal()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = walkSpeed;

        currentState = AnimalState.Idle;
        UpdateState();
    }

    protected virtual void UpdateState()
    {
        switch (currentState)
        {
            case AnimalState.Idle:
                HandleIdleState();
                break;
            case AnimalState.Moving:
                HandleMovingState();
                break;
            case AnimalState.Chase:
                HandleChaseState();
                break;
        }
    }

    protected Vector3 GetRandomNavMeshPosition(Vector3 origin, float distance)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * distance;
            randomDirection += origin;
            NavMeshHit navMeshHit;

            if (NavMesh.SamplePosition(randomDirection, out navMeshHit, distance, NavMesh.AllAreas))
            {
                return navMeshHit.position;
            }
        }

        return origin;
    }

    protected virtual void CheckChaseConditions()
    {

    }

    protected virtual void HandleChaseState()
    {
        StopAllCoroutines();
    }

    protected virtual void HandleIdleState()
    {
        StartCoroutine(WaitToMove());
    }

    private IEnumerator WaitToMove()
    {
        float waitTime = Random.Range(idleTime / 2, idleTime * 2);
        yield return new WaitForSeconds(waitTime);

        Vector3 randomDestination = GetRandomNavMeshPosition(transform.position, wanderDistance);

        navMeshAgent.SetDestination(randomDestination);
        SetState(AnimalState.Moving);
    }

    protected virtual void HandleMovingState()
    {
        StartCoroutine(WaitToReachDestination());
    }

    private IEnumerator WaitToReachDestination()
    {
        float startTime = Time.time;

        while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance && navMeshAgent.isActiveAndEnabled)
        {
            if (Time.time - startTime >= maxWalkTime)
            {
                navMeshAgent.ResetPath();
                SetState(AnimalState.Idle);
                yield break;
            }

            CheckChaseConditions();

            yield return null;
        }

        // Destination has been reached
        SetState(AnimalState.Idle);
    }

    protected void SetState(AnimalState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        OnStateChanged(newState);
    }

    protected virtual void OnStateChanged(AnimalState newState)
    {
        if (newState == AnimalState.Moving)
            navMeshAgent.speed = walkSpeed;

        if (newState == AnimalState.Chase)
            navMeshAgent.speed = runSpeed;

        UpdateState();
    }

    public virtual void RecieveDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
            Die();
    }

    protected virtual void Die()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
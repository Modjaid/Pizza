using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Computer-controlled enemy unit.
/// </summary>
public class Enemy : BasicUnit
{
    private UnityEngine.AI.NavMeshAgent agent;
    private IEnumerator alarmEffectRoutine;

    public float timeToCoolDown = 5f;
    public float chasingSpeed;
    public float catchDistance = 1f;
    public Renderer closeSightMaterial;
    public Renderer sightMaterial;
    public GameObject alarmEffect;
    public Color chasingColor;
    public Color alarmedColor;
    public Color usualColor;
    public GameObject fovObject;

    public FieldOfView fieldOfView;
    public Transform pathHolder;

    [SerializeField]
    private State state;
    private PlayerAxisController enemy;
    private List<Vector3> patroolingPath = new List<Vector3>();
    private IEnumerator followPathRoutine;
    private IEnumerator chaseRoutine;
    private IEnumerator alarmedRoutine;
    private Vector3 lastTriggerPosition;
    private float lastTimeSeenPlayer;
    private float regularSpeed;
    private float lastTriggerTime;
    private bool patrooling = false;

    private enum State
    {
        // Just standing chillin' state.
        Idle,
        // Patrooling from node to node in looped way.
        Patrooling,
        // This state is when the enemy is aware there is player somewhene, but
        // He didn't had direct sight of it.
        Alarmed,
        // The enemy had seen player in direct, and he is coming towards him.
        // When the distance to player is small, catch him.
        Chasing,
    }

    protected override void Awake()
    {
        base.Awake();

        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    protected override void Start()
    {
        regularSpeed = agent.speed;

        CreatePathArray();
        ChangeSightColor(usualColor);

        GameManager.Instance.OnGameStopped += StopBrain;

        base.Start();
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameStopped -= StopBrain;
    }

    private void Update()
    {
        if(GameManager.Instance.GameStatus == GameStatus.Paused) return;

        if (GameManager.Instance.GameStatus == GameStatus.Started)
        {
            Animate(agent.velocity.magnitude / agent.speed);
            Think();
        }
    }

    /// <summary>
    /// This function controls majority of Enemy's AI.
    /// </summary>
    private void Think()
    {
        // Look for enemy.
        if (fieldOfView.visibleTargets.Count > 0)
        {
            if (enemy != null && !enemy.IsHidden)
            {
                lastTimeSeenPlayer = Time.time;
            }

            if (enemy == null)
            {
                PlayerAxisController newEnemy = fieldOfView.visibleTargets[0].GetComponent<PlayerAxisController>();

                if (!newEnemy.IsHidden)
                {
                    enemy = newEnemy;
                    lastTimeSeenPlayer = Time.time;

                    ChangeState(State.Chasing);

                    StartChasing(enemy);
                }
            }
        }

        // If enemy is hidden, forget it.
        if (enemy != null && enemy.IsHidden)
        {
            enemy = null;
        }

        // Thonk.
        switch (state)
        {
            // IDLE
            case State.Idle:
                if (patroolingPath.Count > 0)
                {
                    StopMovement();
                    StartPatrooling();
                }
                break;


            // ALARMED
            case State.Alarmed:
                if (Time.time > lastTriggerTime + 4f)
                {
                    ChangeState(State.Patrooling);
                }
                break;


            // CHASING
            case State.Chasing:
                if (Time.time >= lastTimeSeenPlayer + timeToCoolDown)
                {
                    StopChasing();
                    ChangeState(State.Patrooling);
                    StartPatrooling();
                }
                break;


            // PATROOLING
            case State.Patrooling:
                if (!patrooling) StartPatrooling();
                break;
        }
    }

    private IEnumerator PatroolRoutine()
    {
        int currentNodeIndex = 0;
        Move(patroolingPath[currentNodeIndex]);

        while (true)
        {
            Vector3 node = patroolingPath[currentNodeIndex];
            node.y = transform.position.y;
            float distanceToNode = Vector3.Distance(transform.position, node);
            if (distanceToNode < 0.2f)
            {
                currentNodeIndex++;
                currentNodeIndex = currentNodeIndex % patroolingPath.Count;
                Move(patroolingPath[currentNodeIndex]);
            }

            yield return null;
        }
    }

    private void StopBrain()
    {
        StopPatrooling();
        StopChasing();
        StopMovement();

        // todo: stop enemy immediately

        animator.SetFloat("Speed", 0);
    }

    private IEnumerator ChaseRoutine()
    {
        while (state == State.Chasing)
        {
            if (enemy == null) yield break;

            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy <= catchDistance)
            {
                enemy.Catched(this);

                yield break;
            }

            Move(enemy.transform.position);

            yield return new WaitForSeconds(0.1f);
        }
    }

    // Stand still to "process"
    private IEnumerator AlarmRoutine()
    {
        float timeToEndStandStill = Time.time + 1f;
        StopMovement();

        state = State.Alarmed;

        while (state == State.Alarmed)
        {
            if (Time.time >= timeToEndStandStill)
            {
                Move(lastTriggerPosition);

                break;
            }

            yield return null;
        }
    }

    private void ChangeState(State newState)
    {
        state = newState;

        switch (newState)
        {
            case State.Alarmed:
                ShowAlarm();
                ChangeSightColor(alarmedColor);
                break;

            case State.Chasing:
                ShowAlarm();
                ChangeSightColor(chasingColor);
                break;

            case State.Idle:
            case State.Patrooling:
                ChangeSightColor(usualColor);
                break;
        }
    }

    private void ChangeSightColor(Color newColor)
    {
        closeSightMaterial.material.SetColor("_Color", newColor);
        sightMaterial.material.SetColor("_Color", newColor);
    }

    private void StartAlarmedRoutine()
    {
        if (alarmedRoutine != null)
            StopCoroutine(alarmedRoutine);
        alarmedRoutine = AlarmRoutine();
        StartCoroutine(alarmedRoutine);
    }

    /// <summary>
    /// Aware enemy of Player's presence.
    /// This means, tridder is not direct (like the enemy saw the player directly),
    /// But he heard it. Or found his steps or else etc.
    /// </summary>
    /// <param name="triggerPosition">Global coordinate of trigger</param>
    /// <param name="triggerPower">Strength of trigger ranged from 0 to 1</param>
    public void SendTrigger(Vector3 triggerPosition, float triggerPower)
    {
        lastTriggerTime = Time.time;
        lastTriggerPosition = triggerPosition;

        // If enemy is just chilling.
        if (state != State.Chasing && state != State.Alarmed)
        {
            StopPatrooling();

            ChangeState(State.Alarmed);

            StartAlarmedRoutine();
        }
    }

    public override void HitByACar()
    {
        animator.SetTrigger("HitByCar");
        Destroy(fieldOfView);
        Destroy(fovObject);
        Destroy(alarmEffect);
        GetComponent<Collider>().enabled = false;
        agent.enabled = false;
        transform.position = new Vector3(transform.position.x, 0.4f, transform.position.z);
        Destroy(this);
    }

    /// <summary>
    /// Find path and move to position.
    /// </summary>
    public void Move(Vector3 destination)
    {
        agent.isStopped = false;

        agent.SetDestination(destination);
    }

    public override float GetNormalizedSpeed()
    {
        return agent.velocity.magnitude / agent.speed;
    }

    public override bool IsMoving()
    {
        return GetNormalizedSpeed() > 0.1f;
    }

    public override void StopMovement()
    {
        agent.isStopped = true;
    }

    private void StartChasing(PlayerAxisController unit)
    {
        StopPatrooling();

        enemy = unit;
        ChangeState(State.Chasing);

        if (chaseRoutine != null)
            StopCoroutine(chaseRoutine);
        chaseRoutine = ChaseRoutine();
        StartCoroutine(ChaseRoutine());

        agent.speed = chasingSpeed;
    }

    private void StopChasing()
    {
        if (chaseRoutine != null)
            StopCoroutine(chaseRoutine);

        ChangeState(State.Idle);

        agent.speed = regularSpeed;
        enemy = null;
    }

    private void CreatePathArray()
    {
        if (pathHolder == null) return;

        foreach (Transform child in pathHolder.transform)
        {
            Vector3 pathNode = child.position;

            patroolingPath.Add(pathNode);
        }
    }

    private void StartPatrooling()
    {
        ChangeState(State.Patrooling);
        patrooling = true;

        if (pathHolder == null) return;

        if (followPathRoutine != null)
            StopCoroutine(followPathRoutine);
        followPathRoutine = PatroolRoutine();
        StartCoroutine(followPathRoutine);
    }

    private void StopPatrooling()
    {
        patrooling = false;
        if (followPathRoutine != null)
            StopCoroutine(followPathRoutine);

        ChangeState(State.Idle);
        StopMovement();
    }

    private void ShowAlarm()
    {
        if (alarmEffectRoutine != null)
            StopCoroutine(alarmEffectRoutine);
        alarmEffectRoutine = AlarmEffectRoutine();
        StartCoroutine(alarmEffectRoutine);
    }

    private IEnumerator AlarmEffectRoutine()
    {
        alarmEffect.SetActive(true);
        yield return new WaitForSeconds(3f);
        alarmEffect.SetActive(false);

    }
}
using System;
using UnityEngine;
public class EnemyCharacter : PooledMonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private GameObject visualObject;
    [SerializeField] private SoundData deathSound;
    [SerializeField] private State remainState;
    [SerializeField] private State startState;

    public float CurrentMoveSpeed { get; private set; }
    public float RelativeSpeedMultiplier { get; private set; }
    public int CurrentWaypointIndex { get; private set; }
    public Vector3 PreviousPosition { get; internal set; }
    public IPathFinder PathFinder { get; private set; }
    public EnemyData EnemyData => enemyData;
    public GameObject GemBeingCarried { get; private set; }
    public Health HealthComponent { get; private set; }
    public EnemyUnitState CurrentEnemyState { get; private set; }

    public static event Action<EnemyData, Vector3> EnemyDied;
    public static event Action<EnemyCharacter> EnemySnatchedGem;
    public static event Action<EnemyCharacter> EnemyArrivedToBase;
    public static event Action<EnemyCharacter> EnemyArrivedHomeEvent;

    private HealthBar healthBar;
    private Collider enemyCollider;
    private AudioSource audioSource;
    private StateMachine stateMachine;

    void OnEnable()
    {
        if (HealthComponent == null)
            HealthComponent = GetComponent<Health>();

        if (enemyCollider == null)
            enemyCollider = GetComponent<Collider>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (PathFinder == null)
            PathFinder = Services.Get<IPathFinder>();

        if (stateMachine == null)
        {
            stateMachine = new StateMachine
            {
                Owner = this,
                RemainState = remainState
            };
        }

        enemyCollider.enabled = true;
        visualObject.SetActive(true);

        HealthComponent.SetStartingHealth(enemyData.HitPoints);
        HealthComponent.HealthReachedZero += Death;
        HealthComponent.HealthReduced += HealthReduced;
        HealthComponent.EffectApplied += ApplyEffect;
        HealthComponent.EffectRemoved += RemoveEffect;

        OnReturnToPool += RemoveEvents;

        CurrentMoveSpeed = EnemyData.MoveSpeed;

        SetEnemyState(EnemyUnitState.AssaultingBase);

        CurrentWaypointIndex = 0;
    }

    private void Update()
    {
        stateMachine.Update();
    }

    /// <summary>
    /// Apply effect from damage
    /// </summary>
    /// <param name="status"></param>
    private void ApplyEffect(IStatusEffect status)
    {
        if (status is FrozenEffect)
        {
            CurrentMoveSpeed *= status.EffectData.speedReductionPercentage;
        }
        else if (status is StunnedEffect)
        {
            CurrentMoveSpeed = 0;
        }
    }

    /// <summary>
    /// Remove effect that was applied from damage
    /// </summary>
    /// <param name="status"></param>
    private void RemoveEffect(IStatusEffect status)
    {
        if (status is FrozenEffect)
        {
            CurrentMoveSpeed = enemyData.MoveSpeed;
        }
        else if (status is StunnedEffect)
        {
            CurrentMoveSpeed = enemyData.MoveSpeed;
        }
    }

    /// <summary>
    /// Remove events when returned to pool
    /// </summary>
    /// <param name="obj"></param>
    private void RemoveEvents(PooledMonoBehaviour obj)
    {
        HealthComponent.HealthReachedZero -= Death;
        HealthComponent.HealthReduced -= HealthReduced;
        OnReturnToPool -= RemoveEvents;
    }

    /// <summary>
    /// Add healthbar to enemy
    /// </summary>
    /// <param name="healthBar"></param>
    public void AssignHealthBar(HealthBar healthBar)
    {
        this.healthBar = healthBar;
    }

    /// <summary>
    /// Update healthbar
    /// </summary>
    /// <param name="obj"></param>
    private void HealthReduced(int obj)
    {
        healthBar.UpdateHealthBar(obj, enemyData.HitPoints);
    }

    /// <summary>
    /// Update speed based on condition
    /// </summary>
    private void FixedUpdate()
    {
        RelativeSpeedMultiplier = CurrentMoveSpeed / EnemyData.MoveSpeed;
    }

    /// <summary>
    /// Spawn enemy
    /// </summary>
    /// <param name="waveManager"></param>
    public void SpawnEnemy()
    {
        stateMachine.Init(startState);
    }

    /// <summary>
    /// When enemy picks up the gem
    /// </summary>
    /// <param name="gemObject"></param>
    public void PickupGem(GameObject gemObject)
    {
        GemBeingCarried = gemObject;
        SetEnemyState(EnemyUnitState.GoingHome);
        EnemySnatchedGem?.Invoke(this);
    }

    /// <summary>
    /// Drop the gem on death if carried
    /// </summary>
    void DropGem()
    {
        if (GemBeingCarried == null)
            return;

        GemBeingCarried.transform.position = transform.position + Vector3.up;
        GemBeingCarried.SetActive(true);
        GemBeingCarried.transform.SetParent(null);
        GemBeingCarried = null;
    }

    /// <summary>
    /// When enemy reaches players base
    /// </summary>
    public void EnemyReachedBase()
    {
        SetEnemyState(EnemyUnitState.GoingHome);
        EnemyArrivedToBase?.Invoke(this);
    }

    /// <summary>
    /// Update current state of enemy
    /// </summary>
    /// <param name="enemyState"></param>
    public void SetEnemyState(EnemyUnitState enemyState)
    {
        CurrentEnemyState = enemyState;
    }

    /// <summary>
    /// When enemy arrives back home
    /// </summary>
    public void EnemyArrivedHome()
    {
        EnemyArrivedHomeEvent?.Invoke(this);
        stateMachine.Stop();
        ReturnToPool();
    }

    public void Death()
    {
        DropGem();

        EnemyDied?.Invoke(enemyData, transform.position);

        healthBar.CallReturnToPool();
        enemyCollider.enabled = false;
        visualObject.SetActive(false);

        if (deathSound != null)
            Services.Get<ISoundManager>().PlaySound(audioSource, deathSound);

        HealthComponent.HealthReachedZero -= Death;
        HealthComponent.HealthReduced -= HealthReduced;
        HealthComponent.EffectApplied -= ApplyEffect;
        HealthComponent.EffectRemoved -= RemoveEffect;

        stateMachine.Stop();
        ReturnToPool(1);
    }

    public void IncrementWaypointIndex()
    {
        CurrentWaypointIndex++;
    }

    public void DecreaseWaypointIndex()
    {
        CurrentWaypointIndex--;
    }
}

public enum EnemyUnitState
{
    None,
    AssaultingBase,
    GoingHome,
    Dead
}
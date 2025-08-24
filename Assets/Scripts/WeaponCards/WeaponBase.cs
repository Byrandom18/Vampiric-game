// WeaponBase.cs
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [Header("Base Stats - DO NOT MODIFY IN RUNTIME")]
    public float baseShootInterval = 1f;
    public float baseSpeed = 10f;
    public float baseLifetime = 3f;
    public float baseDamage = 10f;
    public int basePenetrate = 0;
    public float baseSize = 1f;
    public int baseCount = 1;
    public float baseDefShred = 0f;

    [Header("Current Stats (Read Only)")]
    [SerializeField] private float _currentShootInterval;
    [SerializeField] private float _currentSpeed;
    [SerializeField] private float _currentLifetime;
    [SerializeField] private float _currentDamage;
    [SerializeField] private int _currentPenetrate;
    [SerializeField] private float _currentSize;
    [SerializeField] private int _currentCount;
    [SerializeField] private float _currentDefShred;

    public float currentShootInterval => _currentShootInterval;
    public float currentSpeed => _currentSpeed;
    public float currentLifetime => _currentLifetime;
    public float currentDamage => _currentDamage;
    public int currentPenetrate => _currentPenetrate;
    public float currentSize => _currentSize;
    public int currentCount => _currentCount;
    public float currentDefShred => _currentDefShred;

    protected virtual void Start()
    {
        UpdateStats();
    }

    public virtual void UpdateStats()
    {
        _currentShootInterval = baseShootInterval;
        _currentSpeed = baseSpeed;
        _currentLifetime = baseLifetime;
        _currentDamage = baseDamage;
        _currentPenetrate = basePenetrate;
        _currentSize = baseSize;
        _currentCount = baseCount;
        _currentDefShred = baseDefShred;
    }

    public virtual void ApplyTemporaryMultipliers(float damageMultiplier, float speedMultiplier,
        float lifetimeMultiplier, float sizeMultiplier, float intervalMultiplier,
        int penetrateAdd, int countAdd, float defShredAdd)
    {
        _currentShootInterval = baseShootInterval * intervalMultiplier;
        _currentSpeed = baseSpeed * speedMultiplier;
        _currentLifetime = baseLifetime * lifetimeMultiplier;
        _currentDamage = baseDamage * damageMultiplier;
        _currentPenetrate = basePenetrate + penetrateAdd;
        _currentSize = baseSize * sizeMultiplier;
        _currentCount = baseCount + countAdd;
        _currentDefShred = baseDefShred + defShredAdd;
    }

    public virtual void ResetTemporaryMultipliers()
    {
        UpdateStats();
    }
}
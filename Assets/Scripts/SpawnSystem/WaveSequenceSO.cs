using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWaveSequence", menuName = "Wave System/Wave Sequence")]
public class WaveSequenceSO : ScriptableObject
{
    [Serializable]
    public class WaveStage
    {
        public WaveConfigSO waveConfig;
        public bool waitForAllEnemiesDead = true;
        public float timeBeforeNextWave = 5f;
        public float waveDuration = 60f;
    }

    [Header("Последовательность волн")]
    public WaveStage[] waves;

    [Header("Настройки сложности")]
    public float difficultyScalePerWave = 1.15f;
    public int maxActiveEnemies = 50;
    public int startingWaveNumber = 1;

    [Header("Настройки респавна")]
    public bool infiniteWaves = false;
    public int wavesBeforeBoss = 5;
    public WaveConfigSO bossWaveTemplate;

    [Header("Награды")]
    public int expRewardPerWave = 50;
    public int currencyRewardPerWave = 10;

    // Метод для получения конфига волны по индексу (с учетом бесконечных волн)
    public WaveConfigSO GetWaveConfig(int waveIndex)
    {
        if (waves == null || waves.Length == 0)
            return null;

        if (infiniteWaves && waveIndex >= waves.Length)
        {
            // Создаем динамическую волну для бесконечного режима
            return CreateDynamicWave(waveIndex);
        }

        return waveIndex < waves.Length ? waves[waveIndex].waveConfig : null;
    }

    // Метод для создания динамической волны
    public WaveConfigSO CreateDynamicWave(int waveIndex)
    {
        WaveConfigSO dynamicWave = ScriptableObject.CreateInstance<WaveConfigSO>();

        // Берем за основу последнюю волну из последовательности
        WaveConfigSO baseWave = waves[waves.Length - 1].waveConfig;

        // Копируем основные параметры
        dynamicWave.waveName = $"Wave {waveIndex + 1}";
        dynamicWave.waveNumber = waveIndex + 1;
        dynamicWave.enemyPrefab = baseWave.enemyPrefab;
        dynamicWave.eliteEnemyPrefab = baseWave.eliteEnemyPrefab;

        // Масштабируем сложность
        float scaleFactor = Mathf.Pow(difficultyScalePerWave, waveIndex - waves.Length + 1);

        dynamicWave.minEnemies = Mathf.RoundToInt(baseWave.minEnemies * scaleFactor);
        dynamicWave.maxEnemies = Mathf.RoundToInt(baseWave.maxEnemies * scaleFactor);
        dynamicWave.healthMultiplier = baseWave.healthMultiplier * scaleFactor;
        dynamicWave.damageMultiplier = baseWave.damageMultiplier * scaleFactor;
        dynamicWave.speedMultiplier = baseWave.speedMultiplier * (1 + (scaleFactor - 1) * 0.5f);
        dynamicWave.eliteChance = Mathf.Min(0.5f, baseWave.eliteChance * scaleFactor);

        return dynamicWave;
    }

    // Метод для проверки, является ли волна босс-волной
    public bool IsBossWave(int waveIndex)
    {
        return !infiniteWaves && waveIndex == waves.Length - 1 ||
               infiniteWaves && (waveIndex + 1) % wavesBeforeBoss == 0;
    }
}
﻿public enum Orientation
{
    north,
    east,
    south,
    west,
    none
}

public enum AimDirection
{
    Up,
    UpRight,
    UpLeft,
    Right,
    Left,
    Down
}

public enum ChestSpawnEvent
{
    onRoomEntry,
    onEnemiesDefeated
}

public enum ChestSpawnPosition
{
    atSpawnerPosition,
    atPlayerPosition
}

public enum ChestState
{
    closed,
    healthItem,
    ammoItem,
    weaponItem,
    empty
}

public enum GameState
{
    gameStarted,
    playingLevel,
    engagingEnemies,
    bossStage,
    engagingBoss,
    levelCompleted,
    gameWon,
    gameLost,
    gamePaused,
    dungeonOverviewMap,
    restartGame
}

public enum ItemParameterType
{
    None,
    ClipAmmoRemaining,
    HealthPointsRecovery,
    CurrentWeaponAmmoRecovery,
    AmmoCapacity
}

public enum AmmoType
{
    None,
    LightType,
    HeavyType,
    EnergyType
}
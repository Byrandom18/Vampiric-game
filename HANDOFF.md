# Handoff — Vampiric-game (hybrid DOTS)

**Дата:** 2026-08-23  
**Проект:** `C:\Unity\Vampiric-game`  
**Unity:** 6000.0.79f1, 2D URP, Input System  
**Git:** нет репозитория  
**План рефактора:** не править `c:\Users\byran\.cursor\plans\dots_hybrid_refactor_6d9d6c94.plan.md`  
**Предыдущий чат:** DOTS hybrid refactor (`6051db6f-3418-4b0f-8eff-6a5ee2435f4e`)

Язык с пользователем — русский. Код и идентификаторы — английские.

---

## Что это

Survivors-like (Vampire Survivors). Сделан гибридный рефактор:

- **ECS + Burst** — враги, снаряды, пикапы, хиты, spatial hash
- **GameObject** — игрок, UI, хаб, спрайты-компаньоны

**Не ставить** Entities Graphics и DOTS Physics.

---

## Состояние кода

Гибридный рантайм написан и компилируется. Проверено через Unity `csc`:

- `Vampiric.Simulation` — exit 0
- `Assembly-CSharp` — exit 0

Исправлены:

- `CS0103` `EnemyDamage` — `EnemyPoolManager` удалён, `HandleDeath()` только `ReturnToPool()`
- `CS0162` `PlayerStats` — мёртвый хвост `UpdateStats` убран
- `CS0518` `EnemyPrefabCache.CachedEnemy` — вместо `{ get; init; }` обычные поля
- `SGSG0002` `HitResolveSystem.TryHitPlayer` — добавлен `ref SystemState state`

**В редакторе сцены ещё не донастроены.** Play с Hub уже поднимает симуляцию, но часть UI/префабов не назначена (см. «Что сделать в Unity»).

---

## Конфиг проекта

| Параметр | Значение | Комментарий |
|---|---|---|
| Entities | `1.4.8` | Burst 1.8.29, Collections 2.6.8, Mathematics 1.3.2 |
| API | .NET Standard 2.1 (`apiCompatibilityLevel: 6`) | нет `IsExternalInit` — не использовать `init` |
| Unsafe | выкл. в проекте, вкл. в `Vampiric.Simulation.asmdef` | так и должно быть |
| Input | Both (`activeInputHandler: 2`) | action asset уже в Build Settings |
| Build | Hub → SampleScene | Play только с Hub |
| Visual Scripting | в манифесте, графов нет | можно снять позже |
| Pixel Perfect | ошибки в PackageCache | не наш код, не используется |

Игровой код **остаётся в Assembly-CSharp**, чтобы был виден `InputSystem_Actions`. Симуляция — отдельная asmdef.

---

## Архитектура

```
WaveManager / WeaponFireDirector / Pickup
        ↓
SimulationDriver  →  SimEntityFactory  →  ECS world
        ↓                                      ↓
SimulationBridge (NativeQueue / maps)     ISystem (Burst)
        ↓                                      ↓
PresentationRegistry (GO pool, sync TF)   SpatialHash
        ↓
PlayerStats / UI / Hub / Save
```

### Сборки и неймспейсы

- `Assets/Scripts/Simulation/` + `Vampiric.Simulation.asmdef`  
  `Vampiric.Simulation`, `Vampiric.Combat`, `Vampiric.Stats` (enum `StatId`)
- Остальное — Assembly-CSharp  
  `Vampiric.Game`, `Vampiric.Weapons`, `Vampiric.Items`, `Vampiric.Meta`, `Vampiric.Presentation`, `Vampiric.UI`

Конвенции: `_privateFields`, `PascalCase` свойства, неймспейсы `Vampiric.*`.

### Автосоздание после загрузки сцены

| Сцена | Кто | Что создаёт |
|---|---|---|
| не Hub | `GameContentBootstrap` | PresentationRegistry, SimulationDriver, WeaponFireDirector (на объект с тегом `Player`), ArtifactRunInventory, дефолтные каталоги оружия/артефактов/фьюжна |
| Hub | `HubController` | runtime uGUI: начать забег, фьюжн, ремонт |

Не обязательно вешать эти компоненты руками.

### Симуляция (`Vampiric.Simulation`)

Компоненты: `Health`, `Defense`, `Velocity2D`, `ColliderRadius`, `SimFaction`, `PlayerTag`, `PlayerRuntimeStats`, `EnemyTag`, `EnemyMoveData`, `ContactDamage`, `EnemyRangedAttack`, `ExplodeOnDeath`, `SplitOnDeath`, `ProjectileTag`, `DamagePayload`, `Pierce`, `HomingData`, `BounceData`, `HitRecord`, `PickupTag`/`PickupData`, `Lifetime`, `CompanionLink`, `PendingDestroy`, `LootValue`.

Системы: `SpatialHashSystem`, `EnemyMoveSystem`, `ProjectileMoveSystem`, `HomingSystem`, `HitResolveSystem`, `LifetimeSystem`, `ContactDamageSystem`, `PickupSystem`, `EnemyRangedSystem`.

Бой: `DamageRequest` + Builder, `WeaponCategory`, `FactionId`, `DamageMath`, `SimEvents`.  
Мост: `SimulationBridge` — статические NativeQueue / hash maps.  
Пулы: `SimArchetypes`, `PrefabCatalog` (неймспейс `Vampiric.Game`, лежит в Simulation).

### Презентация

`PresentationRegistry` пулит companion GO и **выключает** старые MB, чтобы не было двойной симуляции:

`EnemyClass`, `EnemyDamage`, `Drone`, `Elemental`, `ExplosiveEnemy`, `SlimeDeath`, `Projectile`, `Bouncing`, `SeekingMissile`, `Cyclone`, `CollectibleItem` + `Rigidbody2D.simulated = false`.

Поэтому сундуки открываются триггером игрока, не снарядами.

`EnemyPrefabCache` читает статы с легаси-компонентов префаба. На вражеских префабах **нужны** `EnemyClass` + `EnemyDamage` (и спец-скрипты, если есть).

### Игрок и статы

`PlayerStats` — фасад над `StatSheet`. Смерть: `ProfileState.CommitFinishedRun` → LoadScene `"Hub"`.

Важно:

- `RefreshDerivedStats` копирует тоталы листа в публичные поля.
- `UpgradeStat` пишет карточные моды **в лист**, не в поля (кроме `baseHealth` / `baseAtk`).
- **Не вызывать** `PushBasesToSheet` после артефактов/карт без сброса баз — задвоит статы.

Тег `Player` стоит на **дочернем** `Player Sprite`, родитель `Player` — Untagged. Так и должно быть: камера и `WeaponFireDirector` ищут тег.

### Оружие

`WeaponDefinition` / `WeaponLoadout` / `WeaponFireDirector` / `WeaponEvolutionCatalog` / `WeaponId` / `FirePattern`.  
`WeaponManager` только гоняет loadout и **выключает** легаси-скрипты оружия.  
`WeaponScript.Update` — no-op.

`GameContentBootstrap.RegisterWeapons` берёт `projectilePrefab` рефлексией с компонентов на сцене (`ConeScript` и т.д.). Скрипты оружия на Player Sprite **не удалять**.

| Id | Оружие | Эволюция |
|---|---|---|
| 1 Cone | Веер шторма | + Bouncing → RicochetCone |
| 2 Spread | Круговой залп | + Homing → HomingFan |
| 3 ArmorBreak | Пробой брони | |
| 4 Minigun | Миниган | + Grenade → ExplosiveMinigun |
| 5 Homing | Самонаведение | |
| 6 Bouncing | Рикошет | |
| 7 Grenade | Граната, `maxLevel = 1` | сразу можно эволюционировать |
| 8–10 | эволюции | |

Карты: `CardSelectionSystem.InjectGeneratedCards` добавляет гранату и эволюции. Разлок гранаты: `isWeaponUnlock` + `resultWeaponId = Grenade`. Старые `CardData` в сцене уже назначены.

Эволюции кросс-категорийные (как VS).

### Артефакты и мета

Забег: **6 экип** (статы сразу) + **6 рюкзак** (без статов). Склад хаба без лимита.  
Переполнение 12 слотов → `ReplaceArtifactPanel` (replace или отказ).  
Фьюжн **только в хабе**.

Срок жизни (считаются **завершённые** забеги; забег дропа не считается; склад не стареет):

| Редкость | Забеги | Ремонт (золото) | Аффиксы |
|---|---|---|---|
| Rare | 3 | 50 | 1–2 |
| Epic | 5 | 150 | 2–3 |
| Legendary | 8 | 400 | 3–4 |

Common из дизайна убран (`ItemRarity`). У старого `EquipmentItem` enum Common ещё есть — только для легаси-ассетов.

Дефолтные артефакты (`ArtifactCatalog`):

| Id | Имя | Редкость |
|---|---|---|
| 1 | Патрон | Rare |
| 2 | Краеугольная линза | Rare |
| 3 | Мушка винтовки | Rare |
| 4 | Серебряный слиток | Rare |
| 5 | Прицел сокола | Epic |
| 6 | Серебряная пуля | Epic |
| 7 | Снаряд Богов | Legendary |

Фьюжн: 1+3→5, 2+4→6, 3+4→6, 5+6→7.

Сейв: `Application.persistentDataPath/vampiric_profile.json` (`SaveService`).

Дроп артефакта с врага ~8% (`SimulationDriver`).

### Волны

`WaveManager` спавнит через `SimulationDriver`, считает `_simulatedActive`, слушает `SimulationDriver.EnemyKilled`.  
`currentWaveSequence` в сцене уже назначен.  
Сплит-слаймы **не увеличивают** `enemiesRemaining`.

---

## Что сделано в сценах / что нет

### Уже есть в SampleScene

- Wave Manager + sequence
- CardSelectionSystem + пул карт + панель
- ItemPoolManager: exp/gold/magnet/heart/bomb
- Health Bar на Canvas
- Player Sprite: PlayerStats, движение, все 6 легаси-оружий с префабами снарядов, WeaponManager
- EventSystem, Input
- Камера с URP Additional Camera Data + CameraFollowing

### Не назначено (нужно в Unity)

**Player Sprite → PlayerStats**

- `expBar` = None — компонента `ExpBar` в сцене нет
- `levelText` / `goldText` / `gemsText` = None

**Manager → ItemPoolManager:** `gemPrefab` = None  

**Цифры урона:** в сцене нет `DamageTextManager`. Префаб есть: `Assets/Prefabs/UI/DamageText.prefab`.  
Либо добавить `DamageTextManager`, либо положить `SimulationDriver` и назначить `_damageTextPrefab`.

**Hub.unity:** только голая камера, без Universal Additional Camera Data. Overlay UI хаба рисуется, фон может быть чёрным. Добавить URP camera data (+ Light 2D по желанию). Имена сцен не менять: `"Hub"`, `"SampleScene"`.

**Мусор**

- Missing Script после удаления `EnemyPoolManager`, `LootSystem`, `ItemGenerator`, `ItemCombiner` (`.cs` нет, `.meta` остались)
- Объект `Enemy` — старые тестовые инстансы, лучше выключить
- `NewMonoBehaviourScript.cs` — пустышка
- На `CardSelectionSystem` висит лишний скрипт с `lifeTime`/`floatSpeed` (похоже на DamageText)

---

## Как проверять

1. Дождаться импорта Entities, Console без ваших CS-ошибок.
2. Play **с Hub** (не SampleScene).
3. «Начать забег» → SampleScene: ходьба, волны, спрайты врагов едут за entity.
4. Левелап → карты, в том числе граната и эволюции.
5. Смерть → Hub, золото/гемы в статусе.

Если враги без спрайтов — у префаба волны нет `EnemyClass`/`EnemyDamage`.  
Если оружие молчит — на Player Sprite нет weapon-скриптов с `projectilePrefab`.

---

## Известные ямы

- Нет SubScene / baking — мир поднимает `SimulationDriver.EnsureWorld()` через `DefaultWorldInitialization`.
- Companion physics выключена → сундуки только игроком.
- Сплит не инкрементит remaining волны.
- Hub без URP-камеры выглядит плоско/чёрно.
- `com.unity.2d.pixel-perfect` сыплет CS0234 в лог редактора — игнор или снять пакет.
- Visual Scripting не используется.
- `EquipmentItem` / старые SO оставлены ради GUID сцен и префабов.
- Артефакт-пикап сейчас спавнится с `_expPrefab` (тот же спрайт опыта).
- Каталоги оружия/артефактов/фьюжна создаются в рантайме, ассетов SO нет.

---

## Что делать дальше (приоритет)

1. **Редактор:** ExpBar + тексты золота/уровня/гемов; DamageText; URP на камере Hub; снять Missing Script; выключить объект Enemy.
2. Прогнать полный цикл Hub → забег → карты → смерть → Hub и проверить JSON сейва.
3. Отдельный визуал для artifact pickup, не спрайт опыта.
4. Вынести `WeaponDefinition` / `ArtifactDefinition` / каталоги в ассеты вместо `CreateInstance`.
5. Почистить `.meta` удалённых скриптов и `NewMonoBehaviourScript`.
6. По желанию снять Visual Scripting и Pixel Perfect.

Не ставить Entities Graphics / DOTS Physics. Не переносить игровой код в asmdef без решения по `InputSystem_Actions`. Не использовать `init` accessors.

---

## Ключевые файлы

| Путь | Роль |
|---|---|
| `Assets/Scripts/Game/GameContentBootstrap.cs` | автостарт рантайма, регистрация оружия |
| `Assets/Scripts/Game/SimulationDriver.cs` | мир, спавн, drain событий |
| `Assets/Scripts/Game/SimEntityFactory.cs` | создание entity |
| `Assets/Scripts/Game/EnemyPrefabCache.cs` | статы с легаси-префабов |
| `Assets/Scripts/Presentation/PresentationRegistry.cs` | GO-компаньоны |
| `Assets/Scripts/Simulation/**` | ECS |
| `Assets/Scripts/Weapons/**` | data-driven оружие |
| `Assets/Scripts/Items/**` | артефакты |
| `Assets/Scripts/Meta/**` | сейв, хаб |
| `Assets/Scripts/Player/PlayerStats.cs` | фасад статов, смерть → Hub |
| `Assets/Scripts/SpawnSystem/WaveManager.cs` | волны → SimulationDriver |
| `Assets/Scripts/Player/CardSelectionSystem.cs` | карты + инжект эволюций |
| `Assets/Scripts/Enemy/EnemyDamage.cs` | легаси, только презентация/кэш |
| `Packages/manifest.json` | Entities 1.4.8 |
| `ProjectSettings/EditorBuildSettings.asset` | Hub first |

# План сборки в Unity, контент, карты, хаб и персонажи

Документ для сборки текущего гибридного проекта и добавления контента.  
Код уже компилируется. Сцены и ассеты ещё не доведены.  
Персонажей в коде **нет** — это отдельный этап в конце.

Связанный статус: `HANDOFF.md`.

---

## 0. Как сейчас устроено включение оружия

Это главный контракт. Его нельзя ломать при добавлении карт и персонажей.

### Цепочка

```
CardData (ассет)
    → CardSelectionSystem.SelectCard
        → WeaponManager.ApplyCardEffect
            → WeaponLoadout.Unlock / ApplyUpgrade / Remove
                → WeaponFireDirector стреляет только ActiveSlots
```

### Регистрация (ещё не стрельба)

При старте `SampleScene` `GameContentBootstrap` создаёт `WeaponFireDirector` на объекте с тегом `Player` (сейчас это **Player Sprite**) и вызывает `RegisterDefinition` для каждого `WeaponId`.

`Register` кладёт слот в loadout:

- `IsUnlocked = false`
- `Level = 0`
- definition есть, префаб снаряда регистрируется в `PresentationRegistry`

**Сейчас ни одно оружие не включается само.** Игрок начинает без оружия, пока не выберет карту unlock.

`Configure()` у runtime-definition выставляет только id, имя, категорию, паттерн, префаб, aim, флаг эволюции и maxLevel.  
Числа (`BaseInterval`, `BaseDamage` и т.д.) остаются дефолтами SO (`1` сек, `10` урона…). Значения со старых `ConeScript` / `MinigunScript` **не копируются**. Поэтому следующий шаг контента — вынести `WeaponDefinition` в ассеты с реальными цифрами.

### Что считает «включённым»

`WeaponLoadout.Unlock(id)`:

- слот должен уже быть зарегистрирован
- если уже unlocked — ничего
- иначе `IsUnlocked = true`, `Level = 1`

`WeaponFireDirector.Update` идёт только по `Loadout.ActiveSlots` (`IsUnlocked == true`) и спавнит ECS-снаряды.

Старые `ConeScript` и остальные на Player Sprite **выключены** в `WeaponManager.Awake`. Они нужны только как источник `projectilePrefab` (рефлексия в `FindPrefab<T>`). Не удалять их, пока оружие не переедет на ассеты.

### Три пути карты → оружие

`WeaponManager.ApplyCardEffect`:

1. **Эволюция** (`card.isEvolution`)  
   снимает `firstWeaponId` и `secondWeaponId`, включает `resultWeaponId` на 1 уровне.

2. **Разлок по новому id** (`isWeaponUnlock && resultWeaponId != 0`)  
   `Unlock((WeaponId)resultWeaponId)`. Так сейчас работает граната (id `7`).

3. **Разлок по старому enum** (`isWeaponUnlock`)  
   `Unlock(FromLegacy(weaponType))`.  
   `WeaponType`: Cone=0, Spread=1, ArmorBreak=2, Minigun=3, Homing=4, Bouncing=5.  
   Гранаты и эволюций в этом enum **нет**.

Иначе это **апгрейд**: `ApplyUpgrade` — если оружие выключено, сначала Unlock, потом `Level++` и множители с карты.

### Когда карта вообще показывается

`CardSelectionSystem.IsCardAvailable`:

| Условие | Результат |
|---|---|
| `PlayerStats.lvl < requiredLevel` | скрыта |
| `isEvolution` | оба исходных оружия на maxLevel **и** результат ещё не открыт |
| `resultWeaponId == Grenade` | скрыта, если граната уже есть |
| обычный unlock | скрыта, если `WeaponManager.IsWeaponUnlocked(weaponType)` |
| `requiredCards` | все указанные карты уже выбраны (`PlayerProgress`) |
| `GetCardLevel >= maxLevel` | скрыта |

Гранату и 3 эволюции `InjectGeneratedCards()` **добавляет в рантайме**. В `allCards` на сцене их нет.

`GetDescription()` для любой unlock-карты **игнорирует** текст ассета и пишет `Unlock {weaponType} Weapon`. У гранаты из-за этого описание врёт (`ArmorBreak`). Это надо починить при разборе карт.

### Старт персонажа (когда появится)

Стартовое оружие — это **не карта**, а прямой вызов после регистрации:

```
director.Loadout.Unlock(character.StartingWeapon);
```

Карту unlock этого оружия из пула убрать, иначе игрок «откроет уже открытое».

---

## 1. Сборка проекта в Unity (сцена забега)

Делать **до** большого контента. Play — только со сцены **Hub** (в Build Settings она первая). Для отладки волн можно открыть `SampleScene`, но хаб/сейв/персонаж тогда пропускаются.

### 1.1 Импорт и Console

1. Открыть проект в **6000.0.79f1**.
2. Дождаться импорта Entities.
3. Не ставить Entities Graphics и DOTS Physics.
4. Свои ошибки (`CS0103`, `CS0162`) уже исправлены. `com.unity.2d.pixel-perfect` в логе можно игнорировать.

### 1.2 Теги и иерархия игрока

- Родитель `Player` — Untagged, только Transform.
- Ребёнок `Player Sprite` — тег **Player**. Здесь `PlayerStats`, `PlayerMovement`, оружейные скрипты, `WeaponManager`, `PlayerProgress`.
- Тег не переносить на родителя: камера и `WeaponFireDirector` ищут `Player`.

### 1.3 HUD на Canvas (`SampleScene`)

Сейчас назначен только `healthBar`.

1. Под `Bars` (или рядом) создать **Exp Bar**:
   - Slider
   - компонент `ExpBar`
   - в `ExpBar.expSlider` — этот Slider
   - по желанию `levelText` / `expText` на самом ExpBar
2. `PlayerStats.expBar` ← этот объект.
3. Три UI Text (uGUI Text или завести TMP и поменять поля — сейчас тип `UnityEngine.UI.Text`):
   - уровень → `levelText`
   - золото → `goldText`
   - гемы → `gemsText`

### 1.4 Цифры урона и пикапы

1. Пустой объект `DamageTextManager`, префаб `Assets/Prefabs/UI/DamageText.prefab`.
   Либо объект `SimulationDriver` и то же в `_damageTextPrefab`.
2. `Manager` → `ItemPoolManager`: назначить `gemPrefab`, если гемы должны падать.
3. Снять Missing Script (удалённые `EnemyPoolManager`, `LootSystem`, `ItemGenerator`, `ItemCombiner`).
4. Выключить корневой объект `Enemy` (тестовые инстансы). Волны спавнят ECS сами.

### 1.5 Карты на сцене

`CardSelectionSystem` уже заполнен. Проверить:

- `cardSelectionPanel`, `cardsContainer`, `cardPrefab` не None
- в `allCards` есть unlock + апгрейды 6 старых оружий и стат-карты
- `useTimer` сейчас выключен — нормально

После появления ассетов гранаты/эволюций — добавить их сюда и **удалить** `InjectGeneratedCards()`.

### 1.6 Камера и ввод

- Main Camera: URP Additional Camera Data уже есть, `CameraFollowing` ищет тег Player.
- Input System: Both, action asset в Build Settings есть.
- EventSystem в сцене есть.

### 1.7 Проверка забега

1. Play на Hub (после этапа 3) или временно на SampleScene.
2. Без стартового оружия снарядов не будет, пока не выпадет unlock — это текущее поведение.
3. Для проверки сразу: в временном скрипте или кнопке вызвать `WeaponFireDirector.Instance.Loadout.Unlock(WeaponId.Cone)`.
4. Волны → спрайты врагов едут за entity.
5. Левелап → 3 карты, выбор работает, `Time.timeScale` возвращается в 1.
6. Смерть → сцена `Hub`.

---

## 2. Карточки: что делать сейчас

Карты — единственный способ открыть и качнуть оружие (пока нет персонажа). Три вида одного `CardData`.

### 2.1 Типы

**A. Unlock оружия**

- `isWeaponUnlock = true`
- `statValue = 0`
- `maxLevel = 1`
- `weaponType` = нужный `WeaponType` (для 6 старых)
- для гранаты/новых id ещё `resultWeaponId` = число из `WeaponId` (Grenade = 7)
- множители оставить 1

Существующие ассеты: `Assets/Resources/Cards/Weapons/*/… Unlock.asset`.

**B. Апгрейд оружия**

- `isWeaponUnlock = false`
- `statValue = 0`
- `weaponType` того же оружия
- `requiredCards` = карточка unlock этого оружия (как `Cone2` требует `Cone unlock`)
- множители: `1.2` = +20% (`{dmg}` в описании)
- `penetrateAdd` / `countAdd` / `defShredAdd` — плюсом
- `maxLevel` — сколько раз эта **конкретная** карта может выпасть (`PlayerProgress` считает по ссылке на ассет)

Цепочка VS-стиля: Unlock → Cone2 → Cone3 → …  
Либо одна карта апгрейда с `maxLevel = 5`.

**C. Стат игрока**

- `isWeaponUnlock = false`
- `statValue != 0` (иначе карта уйдёт в апгрейд оружия!)
- `statType` из enum (`AtkMod = 4`, `HealthMod = 1`, …)
- оружейные множители = 1

Пример: `AtlMod.asset` — `statType=4`, `statValue=10`, `maxLevel=3`, `requiredLevel=3`.

**D. Эволюция**

- `isEvolution = true`
- `isWeaponUnlock = true`
- `firstWeaponId` / `secondWeaponId` / `resultWeaponId` — int из `WeaponId`
- `maxLevel = 1`
- показывается только когда оба исходных на капе

Сейчас D создаётся в коде. Нужны ассеты.

### 2.2 Как создать карту в редакторе

1. RMB → **Create → Cards → Card Data**.
2. Положить в `Assets/Resources/Cards/…` (или `Assets/Content/Cards/`).
3. Заполнить поля по типу A/B/C/D.
4. Описание: плейсхолдеры `{dmg}` `{spd}` `{dur}` `{size}` `{cd}` `{pen}` `{count}` `{shred}` `{stat}`.
5. Перетащить ассет в `CardSelectionSystem.allCards` на сцене.  
   Если не перетащить — карта не выпадет.

### 2.3 Что починить в картах (код, маленький этап)

Сделать до массового наполнения, иначе описания и граната врут.

1. `GetDescription`: если `isWeaponUnlock` — брать `description` ассета, не хардкод.
2. `IsCardAvailable` для unlock: проверять `resultWeaponId`, если он не 0, иначе `weaponType`. Стартовое оружие персонажа тоже фильтровать.
3. Убрать `InjectGeneratedCards`. Сделать 4 ассета: Grenade Unlock + 3 эволюции, добавить в `allCards`.
4. Расширить `WeaponType` **или** везде перейти на `WeaponId` / `resultWeaponId`. Второй путь правильнее: `WeaponType` не знает Grenade и эволюции.
5. `ApplyCardEffect`: эволюции уже идут через `isEvolution` раньше unlock — порядок ок.

### 2.4 Правила баланса карт

- Unlock: один на оружие, `maxLevel = 1`.
- Апгрейды одного оружия в сумме не больше `WeaponDefinition.MaxLevel - 1` (уровень 1 даёт Unlock). Сейчас maxLevel оружия = 6, значит 5 апгрейдов.
- Эволюции: `WeaponDefinition.maxLevel = 1` у результата (уже так у гранаты и эволюций).
- Стат-карты не должны иметь `isWeaponUnlock`.
- Не ставить `statValue != 0` на оружейную карту — сломает ветку в `ApplyCardEffect`.

---

## 3. Как добавлять новый контент

### 3.1 Новое оружие (полный цикл)

**Код (один раз на новое поведение):**

1. Добавить значение в `WeaponId`.
2. Если паттерн новый — значение в `FirePattern` и ветка в `WeaponFireDirector.Fire`.
3. Если хватает Cone / Spread / Homing / Bounce / Minigun / Grenade / ArmorBreak — новый id + definition, код паттерна не нужен.

**Ассеты:**

1. Префаб снаряда (спрайт + по желанию старый `Projectile` для вида; симуляцию с него снимет `PresentationRegistry`).
2. **Create → Vampiric → Weapon Definition**:
   - Id, имя, Category, Pattern, Max Level
   - Projectile Prefab
   - интервал, урон, скорость, жизнь, размер, pierce, count, shred
   - углы / homing / bounce
   - Aim
3. Карта Unlock + 1–5 апгрейдов (п. 2).
4. Положить definition в каталог, который bootstrap **читает с диска**, а не `CreateInstance`.

**Сборка bootstrap (обязательный рефактор контента):**

Сейчас `RegisterWeapons()` захардкожен. Заменить на:

- поле `List<WeaponDefinition> _weapons` или папка `Resources/Weapons`
- `foreach` → `director.RegisterDefinition(def)`
- префаб брать из SO, не из `FindPrefab<ConeScript>()`

Пока этого нет, новое оружие всё равно придётся дописывать в `RegisterWeapons`.

**Эволюция:** строка в `WeaponEvolutionCatalog` (ассет, не `CreateDefault`) + карта D + definition результата с `Is Evolution`.

### 3.2 Новый враг

1. Префаб с спрайтом, коллайдером (для вида).
2. Обязательно `EnemyClass` (скорость, урон) и `EnemyDamage` (хп, опыт).  
   `EnemyPrefabCache` читает их рефлексией.
3. По типу: `Drone`, `Elemental`, `ExplosiveEnemy`, `SlimeDeath`.
4. На забеге эти MB **выключатся** — двигает ECS. Компоненты нужны как таблица статов.
5. `Create → Wave System → Wave Config`: `enemyPrefab`, числа, радиус, множители.
6. Добавить stage в `WaveSequenceSO` (`NewWaveSequence` уже на Wave Manager).

Не класть врагов руками на сцену.

### 3.3 Новый артефакт

1. **Create → Vampiric → Artifact Definition**: уникальный int Id, имя, иконка, Rare/Epic/Legendary, фиксированные статы.
2. Зарегистрировать в `ArtifactCatalog` (сейчас `EnsureDefaults` в коде — вынести в список SO).
3. Фьюжн: рецепт в `ArtifactFusionCatalog` (два id → результат). Только хаб.
4. Визуал пикапа: сейчас спавнится спрайт опыта. Позже отдельный префаб.

### 3.4 Новая карта статов / иконки

Как 2.1 C. Иконки можно брать из того же атласа, что у существующих карт.

---

## 4. Полный план UI хаба (не из кода)

Сейчас `HubController.BootIfHubScene` создаёт Canvas, кнопки и список склада через `new GameObject`. Это временный каркас. **Целевой хаб — сцена `Hub.unity`, свёрстанная в редакторе.** Код только биндит ссылки и вызывает логику.

`ReplaceArtifactPanel` тоже собирается из кода — это UI **забега**, не хаба. Делать префабом отдельно.

### 4.1 Выключить кодогенерацию

1. Удалить `[RuntimeInitializeOnLoadMethod]` у `HubController`.
2. Удалить `BuildUi` / `CreateButton` / `CreateLabel`.
3. Повесить `HubController` на объект в сцене.
4. Все виджеты — `[SerializeField]` ссылки.
5. EventSystem и Canvas лежат в сцене, не создаются в `Start`.

Пока пункт не сделан, любой объект, нарисованный в Hub, **дублируется** вторым runtime-canvas.

### 4.2 Камера и пайплайн

Сцена сейчас: одна Camera без URP.

1. Main Camera → Add Component **Universal Additional Camera Data**.
2. Orthographic, фон тёмный.
3. Light 2D (по желанию), чтобы не было чёрного кадра.
4. Не добавлять игрока и волны на Hub.

### 4.3 Иерархия Canvas (собрать руками)

Рекомендуемый корень: `HubCanvas` (Screen Space Overlay, Canvas Scaler Scale With Screen Size 1920×1080, GraphicRaycaster) + `EventSystem`.

```
HubCanvas
├── Background
├── Header
│   ├── Title
│   ├── GoldText
│   └── GemsText
├── Screen_Main              // главная
│   ├── Button_Play
│   ├── Button_Characters
│   ├── Button_Stash
│   └── SelectedCharacterPreview
├── Screen_Characters        // выбор персонажа
│   ├── CharacterList (Scroll / Horizontal)
│   ├── CharacterCardPrefab
│   ├── PassiveText
│   ├── StartingWeaponText
│   └── Button_ConfirmCharacter
├── Screen_Stash             // склад + фьюжн + ремонт
│   ├── StashGrid
│   ├── SlotPrefab
│   ├── EquippedSlots (6)
│   ├── BackpackSlots (6)
│   ├── DetailPanel (имя, редкость, жизнь, аффиксы)
│   ├── FuseSlotA / FuseSlotB
│   ├── Button_Fuse
│   ├── Button_Repair
│   └── Button_Back
└── Screen_Confirm           // опционально
    └── Button_StartRun
```

Одна сцена, экраны включать/выключать `SetActive`. Не отдельные scene load на каждый экран.

### 4.4 Префабы слотов

**CharacterCard**

- иконка, имя, стартовое оружие, короткое описание пассива
- Button → `HubController.SelectCharacter(id)`
- визуал «выбран»

**ArtifactSlot**

- иконка, редкость (цвет), `RemainingRuns`
- клик: выбрать для деталки / фьюжна / экипа
- второй клик или кнопки: в Equipped (до 6) или Backpack (до 6)
- склад без лимита — ScrollRect + Content Size Fitter

Не спавнить слоты склада из кода «на весь экран кнопками». Спавн **инстансов префаба в готовый Grid** — нормально: вёрстка префаба в редакторе, код только `Instantiate` в контейнер.

### 4.5 Что биндит HubController

```
[SerializeField] Text goldText, gemsText, statusText;
[SerializeField] GameObject screenMain, screenCharacters, screenStash;
[SerializeField] Button playButton, charactersButton, stashButton;
[SerializeField] Button fuseButton, repairButton, confirmCharacterButton;
[SerializeField] Transform stashGrid, equippedGrid, backpackGrid;
[SerializeField] GameObject artifactSlotPrefab;
[SerializeField] Transform characterList;
[SerializeField] GameObject characterCardPrefab;
[SerializeField] Image characterPreview;
[SerializeField] Text passiveText, startingWeaponText;
```

Логика, которая уже есть и остаётся:

- `ToggleLoadout` → 6+6
- `TryFuse` / `TryRepair` (`ProfileState`)
- `SetRunLoadout` + `SceneManager.LoadScene("SampleScene")`

Новое:

- выбранный персонаж в `GameSaveData.SelectedCharacterId`
- Play не стартует, если персонаж не выбран (или выбран дефолт)

### 4.6 Поток игрока в хабе

1. Заход → `Screen_Main`, золото/гемы из сейва.
2. Персонажи → выбрать карточку → Confirm → id в сейв → назад на Main, превью обновлён.
3. Склад → клик предмета → деталка; «В экип» / «В рюкзак»; два предмета + Fuse; один + Repair (50 / 150 / 400).
4. Play → записать экип/рюкзак/персонажа → `SampleScene`.
5. Смерть в забеге → обратно Hub, склад пополнен, предметы состарились (кроме полученных в этом забеге).

### 4.7 Стиль и шрифты

- Не `LegacyRuntime.ttf` из кода. TMP + один шрифт проекта.
- Кнопки — Image + Sprite, не плоский `Image.color` 0.18.
- Экраны якорями, не `anchoredPosition` из кода.

### 4.8 Порядок работ по хабу

1. Собрать пустые экраны и навигацию `SetActive` без логики.
2. Привязать золото/гемы и кнопку Play (грузка SampleScene).
3. Сетка склада + префаб слота + деталка.
4. Fuse / Repair на тех же слотах.
5. Экран персонажей (после этапа 5, когда появятся SO).
6. Удалить runtime-создание UI. Проверить, что Canvas один.

---

## 5. Персонажи: пассив и стартовое оружие

Сейчас персонажа нет. Один визуал Player Sprite, оружия с нуля.

### 5.1 Данные (ассеты, не хардкод)

Новый SO: **Create → Vampiric → Character Definition**

```
id: int
displayName: string
icon / portrait: Sprite
runVisual: Sprite          // что поставить на Player Sprite
startingWeapon: WeaponId   // уровень 1 после Unlock
passive:
  trigger: OnStart | OnLevelUp
  stat: StatId             // например AttackMod
  amount: float            // +1 = +1% за уровень, если AttackMod в процентах
description: string        // для UI хаба
```

Примеры:

| Персонаж | Старт | Пассив |
|---|---|---|
| Охотник | Cone | OnLevelUp AttackMod +1 |
| Танк | Spread | OnStart BaseHealth +40; OnLevelUp HealthMod +1 |
| Берсерк | Minigun | OnLevelUp DamageMod +1 |

Пассив «+1% атаки каждый уровень»:

- `trigger = OnLevelUp`
- `stat = AttackMod`
- `amount = 1`
- на уровне 1 сразу один тик (итого +1% на старте) **или** только со 2 уровня — зафиксировать одно правило. Рекомендация: тик на каждом `LevelUp`, включая переход 1→2; на старте забега отдельный тик «уровень 1», чтобы бонус был сразу.

Несколько пассивов на персонажа — список, не один слот.

### 5.2 Сейв

В `GameSaveData`:

- `SelectedCharacterId`
- позже: список открытых персонажей, если будет анлок

Характер не стареет. Сейв пишет `HubController` при Confirm.

### 5.3 Старт забега

После `RegisterWeapons()` в `GameContentBootstrap` (или новый `RunStartController`):

1. Прочитать `ProfileState.Current.Data.SelectedCharacterId`.
2. Найти `CharacterDefinition`.
3. Спрайт Player Sprite ← `runVisual` (не менять тег и компоненты).
4. `WeaponFireDirector.Instance.Loadout.Unlock(startingWeapon)` — это и есть «стартовое оружие 1 уровня».
5. Повесить `CharacterPassiveRunner` на игрока: применить OnStart; подписаться на `PlayerStats.OnLevelUp`.
6. Карту unlock стартового оружия не класть в доступные (`IsCardAvailable`).

`Unlock` уже ставит Level = 1. Карту апгрейда этого оружия оставлять — качается как обычно.

### 5.4 Пассив в статах

Не писать в публичные поля `PlayerStats.atk` напрямую.

```
Sheet.AddModifier(StatSheet.CharacterSource, new StatModifier(stat, amount));
RefreshDerivedStats();
```

Нужен отдельный source (`CharacterSource`), чтобы при рестарте/смене не задвоить.  
`OnLevelUp`: ещё один модификатор или один стак с растущим value. Проще один модификатор: `amount * (lvl)` и перезапись каждый уровень.

`PlayerStats.LevelUp` уже шлёт `OnLevelUp` до карт — пассив успеет примениться до выбора.

### 5.5 Как добавить нового персонажа (чеклист)

1. Ассет Character Definition.
2. Спрайт / портрет.
3. Стартовый `WeaponId` уже зарегистрирован и имеет карты апгрейда.
4. Пассив заполнен.
5. Положить SO в каталог персонажей (Resources или список на HubController).
6. Карточка в хабе появится из каталога (инстанс `CharacterCard` в список).
7. Play → в забеге сразу стреляет стартовое оружие, пассив виден на статах после уровня.

Код `WeaponId` / паттернов для нового персонажа не нужен, если оружие уже есть.

### 5.6 Чего не делать

- Не плодить отдельные Player-префабы на каждый персонаж, пока отличаются только спрайт, пассив и старт. Один Player Sprite + данные.
- Не включать стартовое оружие картой на 0.1 сек — только `Unlock`.
- Не класть пассив в `CardData`.
- Не хранить пассив только в UI-тексте.

---

## 6. Порядок внедрения (сводка)

| # | Этап | Где | Результат |
|---|---|---|---|
| 1 | Сборка SampleScene (HUD, damage text, missing scripts) | Unity | забег читаемый |
| 2 | Починить описания карт + ассеты гранаты/эволюций, убрать Inject | код + ассеты | карты честные |
| 3 | `WeaponDefinition` ассеты, bootstrap читает список | код + ассеты | цифры оружия из инспектора |
| 4 | Hub сцена: Canvas, экраны, выключить BuildUi | Unity + тонкий код | хаб не из кода |
| 5 | Склад/фьюжн/ремонт на префабах слотов | Unity + HubController bind | мета работает визуально |
| 6 | CharacterDefinition + пассив + Unlock старта | код + ассеты | персонажи |
| 7 | Экран выбора персонажа в Hub | Unity | Play со стартовым оружием |
| 8 | Новый контент по чеклистам §3 и §5.5 | ассеты | без правок ядра |

Не смешивать этап 4 с текущим `RuntimeInitializeOnLoadMethod`: сначала выключить авто-UI, потом рисовать сцену.

---

## 7. Быстрые ответы

**Почему ничего не стреляет на старте?**  
Слоты зарегистрированы, но `IsUnlocked == false`. Нужна карта unlock или `Unlock` от персонажа.

**Как открыть оружие картой?**  
`isWeaponUnlock = true`, правильный `weaponType` или `resultWeaponId`, ассет в `allCards`.

**Как качнуть оружие?**  
Отдельная карта, `isWeaponUnlock = false`, `statValue = 0`, `requiredCards` = unlock, множители ≠ 1.

**Как добавить карту в пул?**  
Создать SO → перетащить в `CardSelectionSystem.allCards`.

**Где эволюции?**  
Сейчас в коде. Перенести в Card Data + каталог рецептов.

**Хаб из кода?**  
Временно да. Целевой — сцена. Автосоздание UI снести до вёрстки.

**Новый персонаж?**  
SO с пассивом и `startingWeapon` → выбрать в хабе → на старте забега `Unlock` + модификатор на `OnLevelUp`.

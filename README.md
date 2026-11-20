
# Little Castaway

**Little Castaway** is a low-poly 3D first-person survival game developed in Unity. After a shipwreck caused by a violent storm, Arthur Sterling washes ashore on a mysterious island. The player must explore, gather resources, discover crafting recipes, manage their survival needs, and deal with the island's wildlife to survive for five days until rescue arrives.

The game combines survival mechanics, exploration, environmental storytelling, crafting, and a stylised low-poly art direction.

## Game Overview

- **Genre:** Survival
- **Perspective:** First-person
- **Art style:** Low-poly 3D
- **Engine:** Unity
- **Target audience:** Players aged 12 and above
- **Main objective:** Survive five complete day/night cycles until rescue

The island holds the remains of an ancient civilisation destroyed centuries ago by a catastrophic storm. According to the island's legends, a curse has affected the surrounding waters, causing ships to disappear. As Arthur explores the island, he discovers ruins, abandoned shelters, and notes left by previous castaways. These notes reveal parts of the island's history and teach the player how to craft tools and useful items.

## Gameplay Features

### Survival System
The player must manage several vital needs:
- Health
- Hunger
- Thirst
- Energy and stamina
- Fatigue and sleep

Survival also involves environmental dangers. Staying underwater for too long causes drowning damage, while extreme fatigue can make the character pass out. Sleeping in a tent helps the player manage fatigue.

### Exploration and Resource Gathering
- Explore the island from a first-person perspective.
- Collect resources and search for food and water.
- Harvest fruit and gather wood.
- Interact with objects, animals, trees, and structures using a camera-based raycast system.
- Discover environmental clues, abandoned shelters, and ancient ruins.

### Crafting and Progression
Crafting is based on recipes discovered throughout the island:
- Find notes that explain how to create tools, weapons, and survival items.
- Unlock recipes by collecting the corresponding notes.
- Gather and combine the required ingredients.
- Use a hotbar to select and equip items.
- Cook meat using the campfire system.

### Wildlife and Animal AI
Animals have different behaviours, implemented using Unity's `NavMeshAgent`:
- **Predators** hunt the player and other creatures.
- **Prey** attempt to flee when they detect danger.
- **Domesticable animals** can become companions.

The player can attempt to tame animals by feeding them apples. The documented success chance is 50%. Tamed animals can be named, assist the player, and support them in combat.

### Dynamic World
- **Day/night cycle:** Each in-game day lasts approximately eight minutes in real time.
- **Dynamic weather:** The island can transition between sunny, rainy, foggy, and stormy conditions.
- **Minimap:** Helps the player navigate the island.
- **Campfire and cooking:** Allows the player to prepare food.
- **Audio and visual effects:** Contextual sound effects and particle systems reinforce actions and environmental atmosphere.

## Technical Overview

| Area | Technology / Approach |
|---|---|
| Game engine | Unity |
| Programming language | C# |
| AI navigation | Unity NavMeshAgent |
| Interaction | Camera-based raycasting |
| Data-driven items and recipes | ScriptableObjects |
| UI | Unity UI |
| Scene transitions | Unity Scene Management and fade transitions |
| Persistence | Save/load system; `PlayerPrefs` is used for scene continuation |
| Version control | Git and GitHub |

### Tools Used
- **Unity** — gameplay programming, scene construction, and asset integration.
- **Adobe Photoshop** — notes, interface elements, and image editing.
- **Adobe After Effects** — cutscene video editing.
- **Canva** — early planning of narrative notes and UI layouts.
- **Notion** — task organisation, documentation, and progress tracking.
- **Epidemic Sound** — music and sound effects.

## Project Structure

The source archive includes the following main areas:

```text
LittleCastaway/
├── Assets_jogo/
│   ├── Animals_prefabs_c_script/
│   ├── Notas/
│   ├── Sons_Game/
│   └── ...                 # UI, prefabs, textures, audio and other assets
├── Receitas/               # Crafting recipe assets
├── Scenes/
│   ├── CutsceneIntro.unity
│   ├── Game/
│   ├── Game.unity
│   ├── LoseScene.unity
│   ├── MainMenu.unity
│   └── WinScene.unity
└── Scripts/
    ├── AnimalAI.cs
    ├── SurvivalManager.cs
    ├── InventoryManager.cs
    ├── MesaCraftingManager.cs
    ├── PlayerInteraction.cs
    └── ...                 # Gameplay, UI, audio and scene-management scripts
```

### Core Systems and Scripts

| Script / System | Responsibility |
|---|---|
| `SurvivalManager` | Coordinates the player's survival needs, fatigue, in-game time, sleeping, drowning, and win/lose conditions. |
| `AnimalAI` | Controls animal behaviour, including predator, prey, and domestication logic. |
| `AnimalSpawner` | Spawns and replenishes wildlife in the island environment. |
| `PlayerInteraction` | Uses camera raycasts to detect and interact with objects, animals, trees, and structures; also handles combat and tool actions. |
| `InventoryManager` | Manages item collection, quantities, and removal from the player's inventory. |
| `InventoryUI` | Displays and controls the inventory interface. |
| `HotbarManager` | Handles quick item selection and equipped items. |
| `MesaCraftingManager` | Checks ingredients and unlocked recipes, then creates crafted items. |
| `RecipeData` | Stores recipe requirements, output items, quantities, and unlock conditions. |
| `NoteManager` | Manages collected notes and journal pages, including recipe unlocks. |
| `ControladorDeClima` | Controls dynamic weather changes. |
| `AudioManager` | Centralises music and sound-effect playback. |
| `SceneFader` | Handles fade transitions between scenes. |

## Development Process

The project was developed by an initially three-person group, but the report records that the work was ultimately carried out mainly by the two active team members. Development followed a flexible, iterative workflow: tasks and priorities were adjusted as the project evolved, with frequent communication and practical testing.

### Team Contributions

**Anthony Frade**
- Programming
- Game design
- UI/UX design
- Narrative design
- Environment design
- Sound

**Valezka Naia**
- Programming
- Game design
- UI/UX design
- Level design
- Environment design

Both team members contributed across different areas when needed. The report identifies Anthony as primarily responsible for much of the game's visual and aesthetic work, while Valezka focused mainly on programming.

### Development and Iteration
The initial scope focused on the core playable experience: the main island, first-person controller, interaction and resource gathering, day/night cycle, player status, interface, inventory, narrative notes, and crafting.

Additional features were introduced during development, including dynamic weather, animal domestication, a minimap, drowning, fatigue and passing out, melee combat, campfire cooking, and introduction, victory, and defeat cutscenes.

The original win condition required ten day/night cycles. It was reduced to five to better match the amount of available content and avoid an overly long playthrough.

## Testing and Improvements

The team conducted blind playtesting sessions with external players to evaluate whether the tutorial, interface, and environmental clues were understandable without prior instructions.

Testing identified issues involving:
- Player movement speed and exploration pace.
- Predator difficulty and animal navigation.
- Inventory stacking and consumable items.
- Wood availability in the starting area.
- Hotbar usability.
- Pause menu functionality.
- Inconsistent sound effects.

The team addressed these issues by adjusting movement speed, rebalancing predator behaviour, improving item stacking, increasing available wood, implementing a complete pause menu, and fixing audio and NavMesh issues.

The report describes the final result as an advanced, functional prototype approaching a **vertical slice**, with the core survival loop and major gameplay systems integrated.

## Development Logs

- [Development Log 1](https://youtu.be/dAq97XHEiEE)
- [Development Log 2](https://youtu.be/2GwLT4Pc7Cw)

## Repository

The project repository is available on GitHub:

[ValezkaNaia / LittleCastaway](https://github.com/ValezkaNaia/LittleCastaway)

## Future Improvements

The project report identifies several possible extensions:
- Expand crafting with more complex buildable structures.
- Develop weather systems with a stronger impact on gameplay.
- Add progression and levels for domesticated animals.
- Expand inventory mechanics and strategic depth.
- Explore the possibility of cooperative multiplayer.

## Credits

**Little Castaway** was developed by **Anthony Frade** and **Valezka Naia** for the Bachelor's Degree in Engineering and Digital Game Development at **IPCA**, in the *Desenvolvimento de Jogos Aplicados* course (2025/2026).

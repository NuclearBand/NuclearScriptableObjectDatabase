### Introduction

Each game has data that game-designers work with. In RPG there is a database of items, in match-3 - the cost in the crystals of tools from the store, in action - hit points, for which medical kit heals. 

There are many ways to store such data - someone stores it in tables, in XML or JSON files that edit with their own tools. Unity provides its own way - Scriptable Objects (SO), which I like because you don't have to write your own editor to visualize them, it's easy to make links to the game's assets and to each other, and with Addressables this data can be easily and conveniently stored off-game and updated separately.

In this article I would like to talk about my SODatabase library, with which you can conveniently create, edit and use in the game (edit and serialize) scriptable objects.
<cut />
### Create and edit SO

I create and edit SOs in a separate window similar to the project windows with an inspector - on the left there is a folder tree (the folder where all SOs are located - the group in addressables), and on the right there is a selected SO inspector. 

![Interface](https://habrastorage.org/webt/g-/at/3_/g-at3_ewbaje3clfmpwy2fdv9fw.png).

To draw such a WindowEditor, I use the library [Odin Inspector](https://odininspector.com/). In addition, I use serialization for SO from this library - it extends the standard Unity serialization, allowing to store polymorphic classes, deep nesting, references to classes.

![Create SO](https://habrastorage.org/webt/kw/8z/6k/kw8z6kpamvb8eq2k5mkissvo4m8.png)

Creation of new SO is done by pressing the button in this window - there you need to select the type of the desired model, and it is created in the folder. In order for the SO type to appear in this window as an option, SO must be inherited from DataNode, which has only one additional field to ScriptableObject.
```csharp
public string FullPath { get; }
```
This is the path to a given SO, by which it can be accessed at runtime

### Access to SO in the game

Usually, you need to either get some specific model, for example, SO with a list of settings of some window, or a set of models from a folder - for example, a list of items, where the model of each item represents a separate SO.
For this purpose, SODatabase has two main methods that return either the entire list of models from the desired folder or a specific model from a folder with a certain name.

```csharp
public static T GetModel<T>(string path) where T : DataNode   

public static List<T> GetModels<T>(string path, bool includeSubFolders = false) where T : DataNode
```

Once at the beginning of the game, before requesting the SODatabase models, you need to initialize to update and download data from Addressables.

### Load and save

One of the disadvantages of ScriptableObject in comparison with storing data with serialization in its own format is that it is not possible to change data in SO and save it at runtime. That is, in fact, ScriptableObject is designed to store static data. But game state needs to be loaded and saved, and I implement this through the same SO from the database.

Perhaps this is not an idiomatic way to combine the database of static models of the game with the loading and saving of dynamic data, but in my experience there has never been a case when it would create some inconvenience, but there are some tangible advantages. For example, with the help of the same inspectors, you can watch the game data in the editor and change them. You can conveniently load player saves, look at their contents and edit them in the editor without using any external utilities or your own editors to render XML or other formats.

I achieve this by serializing dynamic fields in ScriptableObject with JSON.

The *DataNode class *- the parent class of all SO stored in *SODatabase* is marked as 
```csharp
[JsonObject(MemberSerialization.OptIn, IsReference = true)].
```
and all its *JsonProperty* are serialized into a save.txt file when you save the game. Accordingly, during the initialization of *SODatabase*, besides the request for addressables change data, *JsonConvert.PopulateObject* for each dynamic model from *SODatabase* is executed using data from this file.

For this to work smoothly, I serialize the SO references (which can be dynamic fields marked as JsonProperty) into a path line and then deserialize them back into the SO references at boot. There is a limitation - data on game assets cannot be dynamic. But it's not a fundamental constraint, I just haven't had a case when such dynamic data would be required yet, so I didn't implement a special serialization for such data.

### Examples

In a class-starter game initialization and data upload
```csharp
async void Awake()
{
    SODatabase.InitAsync(null, null);
    await SODatabase.LoadAsync();
}
```
and saving the state when you leave
```csharp
private void OnApplicationPause(bool pauseStatus)
{
    if (pauseStatus)
        SODatabase.Save();
}

private void OnApplicationQuit()
{
    SODatabase.Save();
}        
```

In RPG to store information about the player I directly create *PlayerSO*, in which only dynamic fields - the name, the number of explosions of the player, crystals, etc.. It's also a good practice in my opinion to create a static line with the path by which I store the model in SODatabase, so that I can access it at runtime. 
 ```csharp
public class PlayerSO : DataNode
{
    public static string Path => "PlayerInfo/Player";

    [JsonProperty]
    public string Title = string.empty;

    [JsonProperty]
    public int Experience;
}
```
 
Similarly, for the player inventory I create *PlayerInventorySO*, where I store a list of links to the player's items (each item is a link to a static SO from the SODatabase).
 
 ```csharp
 public class PlayerInventorySO : DataNode
 {
     public static string Path => "PlayerInfo/PlayerInventory";
 
     [JsonProperty]
     Public List<ItemSO> Items = new List<ItemSO>();
 }
 ```

There are half static, half dynamic data - for example, quests. This may not be the best approach, but I store dynamic information on progress in this quest right in *QuestSO* models with static information about quests (name, description, goals, etc.). Thus, a game-designer in one inspector sees all the information about the current state of the quest and its description.

```csharp
public class QuestNode : DataNode
{
    public static string Path = "QuestNodes";

    //Editor
    public virtual string Title { get; } = string.empty;

    public virtual string Description { get; } = string.Empty;

    public int TargetCount;
    
    //Runtime
    [JsonProperty]
    private bool finished;
    public bool Finished
    {
        get => finished;
        set => finished = value;
    }
}
```
In general, it's better to make the fields with JsonProperty private so that SO does not serialize them.
The access to this data looks like this
```csharp
var playerSO = SODatabase.GetModel<PlayerSO>(PlayerSO.Path);
var playerInventorySO = SODatabase.GetModel<PlayerInventorySO>(PlayerInventorySO.Path);
var questNodes = SODatabase.GetModels<QuestNode>(QuestNode.Path, true);
```

/*
  The Arena is a self-contained game mode.
  Actors score points by killing other actors
  over the course of a round's duration.
*/

using Godot;
using System;
using System.Collections.Generic;

public class Arena : Spatial, IGamemode {
  public Spatial terrain;
  public List<Vector3> enemySpawnPoints, playerSpawnPoints, itemSpawnPoints;
  public List<Vector3> usedEnemySpawnPoints;
  public float roundTimeRemaining, secondCounter;
  public bool roundTimerActive = false;
  public int playerWorldId = -1;
  public const int DefaultKillQuota = 5;
  public int killQuota;
  public Actor player;

  public int nextId = 1;
  public System.Collections.Generic.Dictionary<int, int> scores;
  public System.Collections.Generic.Dictionary<int, Actor> actors;
  

  public void Init(string[] argv){
    string terrainFile = argv[0];
    Sound.PlayRandomSong(Sound.GetPlaylist(Sound.Playlists.Arena));
    killQuota = DefaultKillQuota;

    actors = new System.Collections.Generic.Dictionary<int, Actor>();
    scores = new System.Collections.Generic.Dictionary<int, int>();

    InitTerrain(terrainFile);
    InitSpawnPoints();
  }

  public bool GameOver(){
    return false;
  }

  public bool Victory(){
    return false;
  }

  public void Update(float delta){
    if(roundTimerActive){
      Timer(delta);
    }
  }

  public void Timer(float delta){
    secondCounter += delta;

    if(secondCounter >= 1.0f){
      roundTimeRemaining -= secondCounter;
      secondCounter = 0f;
    }
  }
  
  public bool PlayerWon(){
    GD.Print("Score " + scores[playerWorldId] + ", needed " + killQuota);
    if(scores[playerWorldId] >= killQuota){
      return true;
    }
    
    return false;
  }

  public string GetObjectiveText(){
    string ret = "Arena\n";
    
    int remainingEnemies = killQuota - scores[playerWorldId];

    ret += "Defeat " + remainingEnemies + " enemies to advance.";
    
    return ret;
  }
  
  public string TimeFormat(int timeSeconds){
    int minutes = timeSeconds / 60;
    int seconds = timeSeconds % 60;
    string minutesText = "" + minutes;
    
    if(minutes < 1){
      minutesText = "00";
    }
    
    string secondsText = "" + seconds;
    
    if(seconds < 1){
      secondsText = "00";
    }
    
    return minutesText + ":" + secondsText;
  }

  public int NextId(){
    int ret = nextId;
    nextId++;
    return ret;
  }

  public string NextBotName(){
    string name = "Bot_" + nextId;
    nextId++;
    return name;
  }

  public string NextItemName(){
    string name = "Item_" + nextId;
    nextId++;
    return name;
  }

  public Actor GetPlayer(){
    return player;
  }


  public Actor InitializeActor(Actor actor){
    int id = NextId();
    if(actor.stats != null && actor.stats.HasStat("id")){
      actor.stats.SetStat("id", id);
      scores.Add(id, 0);
      actors.Add(id, actor);
    }
    else{
      GD.Print("Actor without proper stats was initialized");
    }

    return SpawnActor(actor);
  }

  public Actor SpawnActor(Actor actor){
    return actor;
  }

  // public Actor SpawnActor(ActorData dat){
  //   Actor.Brains brain = Actor.Brains.Player1;

  //   Vector3 pos;
  //   if(brain == Actor.Brains.Player1){
  //     pos = RandomSpawn(playerSpawnPoints);
  //   }
  //   else{
  //     pos = RandomSpawn(enemySpawnPoints, usedEnemySpawnPoints);
  //     usedEnemySpawnPoints.Add(pos);
  //   }


  //   Actor actor = null;// FIXME Actor.Factory(brain, dat);
  //   actor.NameHand(actor.Name + "(Hand)");  
    
  //   actors.Add(actor);
  //   AddChild(actor);

  //   return actor;
  // }

  public void LocalInit(){

    // player = InitActor(settings.player, NextId());
    // playerWorldId = player.id;
    //GD.Print("Session.session.Player set to " + Session.session.player.ToString());

    // foreach(ActorData enemy in settings.enemies){
    //   Actor enemyActor = InitActor(enemy, NextId());
    // }

    roundTimerActive = false;
  }

  // public Actor InitActor(ActorData dat, int id){
  //   scores.Add(id, 0);
  //   dat.id = id;
  //   Actor ret = SpawnActor(dat);

  //   return ret;
  // }

  public void InitTerrain(string terrainFile){
    PackedScene ps = (PackedScene)GD.Load(terrainFile);
    Node instance = ps.Instance();
    AddChild(instance);
    terrain = (Spatial)instance;

    // Everything below this line is a dirty hack to work around a bug.
    GridMap gm = Util.GetChildByName(terrain, "Map") as GridMap;
    
    MeshLibrary theme = gm.Theme;
    List<Vector3> colors = new List<Vector3>{
      new Vector3(0.537f, 0.101f, 0.101f),
      new Vector3(0.141f, 0.313f, 0.125f),
      new Vector3(0.137f, 0.215f, 0.521f),
      new Vector3(0.690f, 0.321f, 0.129f),
      new Vector3(0.490f, 0.168f, 0.490f),
      new Vector3(0.717f, 0.694f, 0.203f),
      new Vector3(1,1,1),
      new Vector3(0,0,0)
    };

    for(int i = 0; i < 8; i++){
      ArrayMesh arrMesh = theme.GetItemMesh(i) as ArrayMesh;
      SpatialMaterial material = GFX.GetColorSpatialMaterial(colors[i]) as SpatialMaterial;
      
      material.EmissionEnabled = true;
      material.Emission = GFX.Color(colors[i]);
      material.EmissionEnergy = 0.5f;
      
      material.TransmissionEnabled = true;
      material.Transmission = GFX.Color(colors[i]);

      material.RefractionEnabled = false;
      
      material.Metallic = 1f;
      
      material.Roughness = 0.5f;

      arrMesh.SurfaceSetMaterial(0, material);

    }
  }

  public void HandleEvent(SessionEvent sessionEvent){
    if(sessionEvent.type == SessionEvent.Types.ActorDied ){
      HandleActorDead(sessionEvent);
    }
    else if(sessionEvent.type == SessionEvent.Types.Pause){
      TogglePause();
    }
  }

  public Actor ActorFromPath(string actorPath){
    Node actorNode = GetNode(new NodePath(actorPath));
    return Actor.GetActorFromNode(actorNode);
  }

  public void HandleActorDead(SessionEvent sessionEvent){
    string[] actorPaths = sessionEvent.args;  
    
    if(actorPaths == null || actorPaths.Length == 0 || actorPaths[0] == ""){
      return;
    }

    Actor killed = ActorFromPath(actorPaths[0]);

    if(killed == null || killed.stats == null || killed.stats.GetStat("id") < 1){
      GD.Print("Inadequate info in a SessionEvent to handle killed actor death");
      return;
    }
    ClearActor(killed.stats.GetStat("id"));
    
    Actor killer = ActorFromPath(actorPaths[1]);
    if(killer == null || killer.stats == null || killer.stats.GetStat("id") < 1){
      GD.Print("Inadequate info in a SessionEvent to award killer");
      return;
    }
    
    AwardForKill(killer.stats.GetStat("id"));
    
    Career career = Career.GetActiveCareer();
    
    if(killed.stats.GetStat("id") == playerWorldId && career != null){
      career.FailEncounter();
    }

    if(PlayerWon() && career != null){
      career.CompleteEncounter();
    }
  }

  public void AwardForKill(int id){
    if(scores.ContainsKey(id)){
      scores[id]++;
    }
    else{
      GD.Print("Actor " + id + " doesn't exist.");
    }
  }

  public void ClearActor(int id){
    if(actors.ContainsKey(id)){
      Actor actor = actors[id];
      Node actorNode = actor.body as Node;
      if(actorNode != null){
        actorNode.QueueFree();
      }
      actors.Remove(id);

    }
    if(scores.ContainsKey(id)){
      scores.Remove(id);
    }
  }

  // public void ClearActor(Actor actor){
  //   if(actor == null || actors == null){
  //     GD.Print("ClearActor: actor or actors were null");
  //     return;
  //   }

  //   Node body = actor.body as Node;
  //   if(body != null){
  //     body.QueueFree();
  //   }
  //   actors.Remove(actor);
  // }

  public void SetPause(bool val){
  }
  
  public void TogglePause(){
  }
  
  public void InitSpawnPoints(){
    SceneTree st = GetTree();
    
    playerSpawnPoints = GetSpawnPoints("PlayerSpawnPoint");
    enemySpawnPoints = GetSpawnPoints("EnemySpawnPoint");
    itemSpawnPoints = GetSpawnPoints("ItemSpawnPoint");

    usedEnemySpawnPoints = new List<Vector3>();
  }
  

  List<Vector3> GetSpawnPoints(string name){
    List<Vector3> ret = new List<Vector3>();

    SceneTree st = GetTree();
    List<System.Object> objs = Util.ArrayToList(st.GetNodesInGroup(name));

    foreach(System.Object obj in objs){
      Spatial spat = obj as Spatial;
      if(spat != null){
        ret.Add(spat.GetGlobalTransform().origin);
      }
    }

    return ret;
  }

  public void SpawnItem(ItemFactory.Items type, int quantity = 1){
    Vector3 pos = RandomItemSpawn();
    Item item = ItemFactory.Factory(type) as Item;
    item.Translation = pos;
    AddChild(item);
  }
  
  public Vector3 RandomItemSpawn(){
    System.Random rand = Session.GetRandom();
    int randInt = rand.Next(itemSpawnPoints.Count);
    return itemSpawnPoints[randInt];
  }
  
  public Vector3 RandomSpawn(List<Vector3> spawnList){
    System.Random rand = Session.GetRandom();
    int randInt = rand.Next(spawnList.Count);
    
    return spawnList[randInt];
  }

  // Don't place spawnpoints on top of each other.
  public Vector3 RandomSpawn(List<Vector3> spawnList, List<Vector3> usedList){
    if(spawnList.Count == usedList.Count){
      usedList = new List<Vector3>();
    }
    if(spawnList.Count == 0){
      GD.Print("RandomSpawn: spawnList empty");
    }

    System.Random rand = Session.GetRandom();

    Vector3 ret = new Vector3();
    bool finished = false;
    const int MaxIterations = 500;
    int i = 0;

    while(!finished && i < MaxIterations){
      i++;
      int randInt = rand.Next(spawnList.Count);
      ret = spawnList[randInt];
      
      if(usedList.IndexOf(ret) == -1){
        finished = true;
      } 
    }

    return ret;
  }

  public void PlayerReady(){}

}
using Godot;
using System;
using System.Collections.Generic;

public class CareerMenu : Container, IMenu {
  public Career career;
  public Button mainMenuButton;
  public List<CareerNode> careerNodes;
  public System.Collections.Generic.Dictionary<int, Button> careerButtons;
  public TextEdit background;
  public int nodeYOffset;

  public void Init(){
    nodeYOffset = 0;

    career = Career.GetActiveCareer();
    careerNodes = career.careerNodes;
    Sound.PlayRandomSong(Sound.GetPlaylist(Sound.Playlists.Menu));
    int inProgress = 0; //Session.session.career.stats.GetBaseStat(StatsManager.Stats.NodeInProgress);
    if(inProgress == 1){
      GD.Print("Node in progress");
      int currentNode = 0; //Session.session.career.stats.GetBaseStat(StatsManager.Stats.CurrentNode);
      //Session.session.career.ExecuteNode(currentNode);
      Clear();
    }
    InitControls();
    ScaleControls();
    GetTree().GetRoot().Connect("size_changed", this, "ScaleControls");
  }

  public override void _Process(float delta){
    HandleScrolling();
  }

  public void HandleScrolling(){
    Vector2 mousePos = Util.GetMousePosition();
    Rect2 screen = this.GetViewportRect();
    float width = screen.Size.x;
    float height = screen.Size.y;
    float wu = width/10; // relative height and width units
    float hu = height/10;

    if(mousePos.x < 2*wu || mousePos.x > 6*wu){
      return; // Not in the center of the screen
    }

    float y = mousePos.y;
    int increment = 0;
    int pixelsPast;

    int srs = 3; // Scroll region size
    int brs = 10 - srs; // Bottom scroll region start

    if(y < srs*hu){
      pixelsPast = (int)((srs*hu) - y);
      increment = (int)(pixelsPast / (srs*hu) * 10); 
    }
    else if(y > brs*hu){
      pixelsPast = (int)(y - (brs*hu));
      increment =  (int)(pixelsPast / (srs*hu) * 10f);
      increment *= -1;
    }

    nodeYOffset += increment;
    ScaleControls();
  }

  public void Clear(){
    GD.Print("CareerMenu.Clear");
    this.QueueFree();
  }

  void InitControls(){
    background = Menu.BackgroundBox();
    AddChild(background);

    mainMenuButton = Menu.Button("Main Menu", () => { 
      ReturnToMainMenu(); 
    });
    AddChild(mainMenuButton);

    careerButtons = new System.Collections.Generic.Dictionary<int, Button>();

    foreach(CareerNode node in careerNodes){
      AddNodeButton(node);
    }
  }

  void AddNodeButton(CareerNode node){
    Button button = NodeButton(node.nodeId, node.encounter.GetDisplayName());
    careerButtons.Add(node.nodeId, button);
    AddChild(button);
  }

  Button NodeButton(int id, string type){
    return Menu.Button(type, () => {
      ExecuteNode(id);
    });
  }

  void ExecuteNode(int id){
    GD.Print("Executing node " + id + ".");
    //Session.session.career.ExecuteNode(id);
    return;


    CareerNode node = CareerNode.GetNode(id, career.careerNodes);
    int nodeLevel = CareerNode.GetLevel(node, career.careerNodes);
    int nextLevel = nodeLevel -1;



    if(nodeLevel == 0){
      GD.Print("Final boss time!");
      return;
    }
    else{
      GD.Print("Changing current level to " + nextLevel);
    }
    ScaleControls();
  }

  void ScaleControls(){
    Rect2 screen = this.GetViewportRect();
    float width = screen.Size.x;
    float height = screen.Size.y;
    float wu = width/10; // relative height and width units
    float hu = height/10;

    Menu.ScaleControl(background, width, height, 0, 0);
    Menu.ScaleControl(mainMenuButton, 2 * wu, hu, 0, height - hu);
    ScaleNodeButtons();
  }

  void ScaleNodeButtons(){
    System.Collections.Generic.Dictionary<int, CareerNode[]> levels = CareerNode.GetLevels(careerNodes);
    List<CareerNode> nodes = careerNodes;
    foreach(int key in levels.Keys){
      ScaleLevel(key, levels[key], nodes);
    }
  }

  void  ScaleLevel(int level, CareerNode[] levelNodes, List<CareerNode> nodes){
    int currentLevel = 0; //career.stats.GetStat(StatsManager.Stats.CurrentLevel);
    int lastNode = 0; //career.stats.GetStat(StatsManager.Stats.LastNode);

    for(int i = 0; i < levelNodes.Length; i++){
      bool active = CareerNode.NodeIsActive(levelNodes[i], nodes, currentLevel, lastNode);
      ScaleNodeButton(levelNodes[i].nodeId, level, i, levelNodes.Length, active);
    }
  }

  void ScaleNodeButton(int node, int level, int x, int xMax, bool active){
    Rect2 screen = this.GetViewportRect();
    float width = screen.Size.x;
    float height = screen.Size.y;
    float wu = width/10; // relative height and width units
    float hu = height/10;

    if(!careerButtons.ContainsKey(node)){
      GD.Print("Node " + node + " has no button.");
      return;
    }

    float xSpace = 6 * wu;
    float xSpaceUnits = xSpace / (xMax + 1);
    float xPos = wu + (x + 1) * xSpaceUnits;

    float yPos = hu * (1+level);

    float xSize = wu;
    float ySize = hu;

    if(active){
      float scaleOffset = 50;
      xPos -= (scaleOffset / 2.0f);
      yPos -= (scaleOffset / 2.0f);
      xSize += scaleOffset;
      ySize += scaleOffset;
    }

    yPos += nodeYOffset;

    Button nodeButton = careerButtons[node];
    Menu.ScaleControl(nodeButton, xSize, ySize, xPos, yPos);
    nodeButton.Disabled = !active;
  }

  public void ReturnToMainMenu(){
    Session.ChangeMenu("MainMenu");
  }
}
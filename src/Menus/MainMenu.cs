using Godot;
using System;

public class MainMenu : Container, IMenu {
    
  public Godot.Button newGameButton;
  public Godot.Button continueGameButton;
  public Godot.Button settingsButton;
  public Godot.Button creditsButton;
  public Godot.Button quitButton;
  public TextEdit background;
  public TexturedButton logo;

  public void Init(){
    InitControls();
    ScaleControls();
    GetTree().GetRoot().Connect("size_changed", this, "ScaleControls");
  }

  void InitControls(){
    background = Menu.BackgroundBox();
    AddChild(background);

    logo = Menu.TexturedButton("res://Assets/Textures/logo.jpg");
    AddChild(logo);

    newGameButton = Menu.Button(text : "New", onClick : NewGame);
    AddChild(newGameButton);

    if(CareerDb.SaveExists()){
      continueGameButton = Menu.Button(text : "Continue", onClick : ContinueGame);
      AddChild(continueGameButton);
    }

    quitButton = Menu.Button(text : "Quit", onClick : Quit);
    AddChild(quitButton);
    
    creditsButton = (Godot.Button)Menu.Button(text : "Credits", onClick : Credits);
    AddChild(creditsButton);

    settingsButton = (Godot.Button)Menu.Button(text : "Settings", onClick : Settings);
    AddChild(settingsButton);
    
    Sound.PlayRandomSong(Sound.GetPlaylist(Sound.Playlists.Menu));
  }

  void ScaleControls(){
    Rect2 screen = this.GetViewportRect();
    float width = screen.Size.x;
    float height = screen.Size.y;
    float wu = width/10; // relative height and width units
    float hu = height/10;
    
    Menu.ScaleControl(background, width, height, 0, 0);
    Menu.ScaleControl(logo, 8 * wu, 3 * hu, 2 * wu, 0);
    Menu.ScaleControl(newGameButton, 2 * wu, 2 * hu, 0, 2 * hu);
    if(continueGameButton != null){
      Menu.ScaleControl(continueGameButton, 2 * wu, 2 * hu, 0, 0);
    }
    Menu.ScaleControl(settingsButton, 2 * wu, 2 * hu, 0, 4 * hu);
    Menu.ScaleControl(creditsButton, 2 * wu, 2 * hu, 0, 6 * hu);
    Menu.ScaleControl(quitButton, 2 * wu, 2 * hu, 0, 8 * hu);
  }
  
  public void NewGame(){
    Session.ChangeMenu("NewGameMenu");
  }

  public void ContinueGame(){
    GD.Print("ContinueGame");
    Career.ContinueCareer();
  }

  public void Settings(){
    GD.Print("Settings menu");
    Session.ChangeMenu("SettingsMenu");
  }

  public void Credits(){
    GD.Print("Credits menu");
    Session.ChangeMenu("CreditsMenu");
  }
  
  public void Quit(){
    Session.session.Quit();
  }
    
}

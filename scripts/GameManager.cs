using Godot;
using System.Collections.Generic;
public partial class GameManager : Node
{
    public static GameManager Instance;

    public enum State
    {
        MainMenu,
        Paused,
        Starting,
        Racing,
        Ending,
        Finished,
        Shop
    }

    private State _gameState = State.MainMenu;
    public State GameState => _gameState;

    public int PlayerRacePosition;

    public Camera2D MainCamera;

    public List<PlayerUpgrade> PlayerUpgrades = new();

    public int RaceCount = 1;
    public int PlayerScore = 6000;
    public int PlayerPoints = 0;
    public override void _Ready()
    {
        Instance = this;
        GD.Print("GameManager Ready!");
        //GD.Seed("Protogen".Hash());
    }

    public void SetGameState(State state)
    {
        _gameState = state;
    }

    public void SwitchToRaceScene()
    {
        GD.Print("Starting Race (Switching Scene)");
        GetTree().ChangeSceneToFile("res://scenes/game.tscn");
    }
}

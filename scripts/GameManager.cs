using Godot;

// Notes
// 1. Collisions are fucked lol
// 2. Need markers on floor for when a corner is coming
// 3. Minimap?
// 4. Ramps and Obstacles?

public partial class GameManager : Node2D
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
    public override void _Ready()
    {
        Instance = this;
        GD.Print("GameManager Ready!");
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

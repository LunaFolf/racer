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

    public struct PlayerUpgradeValues
    {
        public float Speed;
        public float Traction;
        public float Turning;
    }

    private PlayerUpgradeValues _playerUpgradeValues;
    public PlayerUpgradeValues PlayerUpgradesMults => _playerUpgradeValues;

    public int RaceCount = 1;
    public int PlayerScore = 8192;
    public int PlayerPoints = 0;

    public string GameSeed = "";

    public AudioStreamPlayer UISFXPlayer;
    private AudioStream _uiAcceptSfx;
    private AudioStream _uiDenySfx;

    public AudioStreamPlayer MusicPlayer;
    private static double _bpm = 140;
    private double _beatInterval = 60.0 / _bpm;
    private double _bpmDelta;

    public float BeatBloom;
    public override void _Ready()
    {
        Instance = this;
        GD.Print("GameManager Ready!");

        MusicPlayer = new AudioStreamPlayer();
        AddChild(MusicPlayer);
        MusicPlayer.Stream = GD.Load<AudioStream>("res://assets/sounds/DST-RailJet-LongSeamlessLoop.mp3");
        MusicPlayer.SetBus("Music");
        MusicPlayer.VolumeDb = -32;
        MusicPlayer.Play();

        UISFXPlayer = new AudioStreamPlayer();
        UISFXPlayer.SetBus("SFX");
        AddChild(UISFXPlayer);
        _uiAcceptSfx = GD.Load<AudioStream>("res://assets/sounds/ui/on.ogg");
        _uiDenySfx = GD.Load<AudioStream>("res://assets/sounds/ui/off.ogg");
    }

    public void UiAccept()
    {
        UISFXPlayer.Stream = _uiAcceptSfx;
        UISFXPlayer.Play();
    }
    public void UiDeny()
    {
        UISFXPlayer.Stream = _uiDenySfx;
        UISFXPlayer.Play();
    }

    public void SetGameState(State state)
    {
        _gameState = state;
    }

    public override void _Process(double delta)
    {
        if (!MusicPlayer.IsPlaying()) return;
        _bpmDelta += delta;

        if (_bpmDelta >= _beatInterval)
        {
            _bpmDelta -= _beatInterval;
            BeatBloom = (float)_beatInterval;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (BeatBloom > 0f) BeatBloom -= (float)delta;
    }

    public void SwitchToRaceScene()
    {
        GD.Print("Starting Race (Switching Scene)");
        GetTree().ChangeSceneToFile("res://scenes/game.tscn");
    }

    public void AddPlayerUpgrade(PlayerUpgrade upgrade)
    {
        PlayerUpgrades.Add(upgrade);
        GD.Print("Added upgrade: " + upgrade.name);

        switch (upgrade.type)
        {
            case PlayerUpgrade.Type.SPEED:
                _playerUpgradeValues.Speed += upgrade.multiplier;
                break;
            case PlayerUpgrade.Type.TRACTION:
                _playerUpgradeValues.Traction += upgrade.multiplier;
                break;
            case PlayerUpgrade.Type.TURNING:
                _playerUpgradeValues.Turning += upgrade.multiplier;
                break;
        }
    }
}

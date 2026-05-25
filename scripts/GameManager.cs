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
        Tutorial,
        Racing,
        Ending,
        Shop,
        GameOver
    }

    private State _gameState = State.MainMenu;
    public State GameState => _gameState;

    public int PlayerRacePosition;

    public Camera2D MainCamera;

    public List<PlayerUpgrade> PlayerUpgrades = new();
    public List<PlayerUpgrade> BotUpgrades = new();

    public struct PlayerUpgradeValues
    {
        public float Speed;
        public float Traction;
        public float Turning;

        public PlayerUpgradeValues(float speed = 0f, float traction = 0f, float turning = 0f)
        {
            Speed = speed;
            Traction = traction;
            Turning = turning;
        }
    }

    private PlayerUpgradeValues _playerUpgradeValues;
    public PlayerUpgradeValues PlayerUpgradesMults => _playerUpgradeValues;

    private PlayerUpgradeValues _botUpgradeValues;
    public PlayerUpgradeValues BotUpgradeMults
    {
        get
        {
            var botMults = _botUpgradeValues;
            var playerMults = _playerUpgradeValues;

            return new PlayerUpgradeValues(botMults.Speed + playerMults.Speed / 2,
                botMults.Traction + playerMults.Traction / 2,
                botMults.Turning + playerMults.Turning / 2);
        }
    }

    public string HighScoreDataJSON
    {
        get
        {
            var data = new int[]
            {
                HighestRaceCount,
                HighestScorePerRace,
                HighestPlayerPoints
            };
            return Json.Stringify(data);
        }
    }


    // Score Stats
    public int HighestRaceCount = 0;
    public int HighestScorePerRace = 0;
    public int HighestPlayerPoints = 0;

    private int _raceCount = 1;
    private int _playerScore = 8192;
    private int _playerPoints = 0;

    public int RaceCount
    {
        get => _raceCount;
        set
        {
            _raceCount = value;
            if (_raceCount > HighestRaceCount)
            {
                HighestRaceCount = _raceCount;
            }
        }
    }

    public int PlayerScore
    {
        get => _playerScore;
        set
        {
            _playerScore = value;
            if (_playerScore > HighestScorePerRace)
            {
                HighestScorePerRace = _playerScore;
            }
        }
    }

    public int PlayerPoints
    {
        get => _playerPoints;
        set
        {
            _playerPoints = value;
            if (_playerPoints > HighestPlayerPoints)
            {
                HighestPlayerPoints = _playerPoints;
            }
        }
    }

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

        if (!FileAccess.FileExists("user://highscore.save"))
        {
            GD.PrintErr("No highscore save file found!");
            return; // Error! We don't have a save to load.
        }

        using var saveFile = FileAccess.Open("user://highscore.save", FileAccess.ModeFlags.Read);
        GD.Print(saveFile.GetAsText());
        var json = new Json();
        var error = json.Parse(saveFile.GetAsText());

        if (error != Error.Ok)
        {
            GD.PrintErr("Error parsing highscore save file!");
            GD.PrintErr(error);
            return;
        }

        GD.Print(json.Data);

        var highScores = (Godot.Collections.Array)json.Data;

        GD.Print(highScores);

        HighestRaceCount = (int)highScores[0];
        HighestScorePerRace = (int)highScores[1];
        HighestPlayerPoints = (int)highScores[2];
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
        Input.MouseMode = state is State.Racing or State.Tutorial ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
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
        Input.StopJoyVibration(0);
        GD.Print("Starting Race (Switching Scene)");
        GetTree().ChangeSceneToFile("res://scenes/game.tscn");
    }

    enum PlayerOrBot { PLAYER, BOT }

    private void AddUpgrade(PlayerOrBot who, PlayerUpgrade upgrade)
    {
        var whoUpgrades = who == PlayerOrBot.PLAYER ? PlayerUpgrades : BotUpgrades;
        var whoMults = who == PlayerOrBot.PLAYER ? _playerUpgradeValues : _botUpgradeValues;

        whoUpgrades.Add(upgrade);

        switch (upgrade.type)
        {
            case PlayerUpgrade.Type.SPEED:
                whoMults.Speed += upgrade.multiplier;
                break;
            case PlayerUpgrade.Type.TRACTION:
                whoMults.Traction += upgrade.multiplier;
                break;
            case PlayerUpgrade.Type.TURNING:
                whoMults.Turning += upgrade.multiplier;
                break;
        }

        if (who == PlayerOrBot.PLAYER)
        {
            PlayerUpgrades = whoUpgrades;
            _playerUpgradeValues = whoMults;
        }
        else
        {
            BotUpgrades = whoUpgrades;
            _botUpgradeValues = whoMults;
        }
    }

    public void AddPlayerUpgrade(PlayerUpgrade upgrade)
    {
        AddUpgrade(PlayerOrBot.PLAYER, upgrade);
    }

    public void AddBotUpgrade(PlayerUpgrade upgrade)
    {
        AddUpgrade(PlayerOrBot.BOT, upgrade);
    }

    public void ResetAllUpgrades()
    {
        PlayerUpgrades.Clear();
        BotUpgrades.Clear();

        _playerUpgradeValues = new PlayerUpgradeValues();
        _botUpgradeValues = new PlayerUpgradeValues();
    }
}

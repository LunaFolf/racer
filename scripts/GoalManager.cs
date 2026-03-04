using Godot;

public partial class GoalManager : Node2D
{

    [Export] public PackedScene TrackStartScene;

    [Export] public PackedScene TrackStraightScene;
    [Export] public PackedScene TrackCornerCwScene;
    [Export] public PackedScene TrackCornerCCwScene;

    private int _goalCounter = 1;

    struct TrackTile
    {
        public Track.TrackType Type;
        public Track.TrackDir Entrance;
        public Track.TrackDir Exit;
        public Track.TrackRotation Rotation;
        public PackedScene TrackScene;

        public TrackTile(Track.TrackType type)
        {
            Type = type;
            Rotation = Track.TrackRotation.Deg0;

            switch (type)
            {
                case Track.TrackType.CornerCw:
                    Entrance = Track.TrackDir.Top;
                    Exit = Track.TrackDir.Left;
                    break;
                case Track.TrackType.CornerCCw:
                    Entrance = Track.TrackDir.Bottom;
                    Exit = Track.TrackDir.Left;
                    break;
                case Track.TrackType.Start:
                case Track.TrackType.Straight:
                default:
                    Entrance = Track.TrackDir.Bottom;
                    Exit = Track.TrackDir.Top;
                    break;
            }
        }

        public Track.TrackDir GetExitDir()
        {
            if (Rotation == Track.TrackRotation.Deg0) return Exit;
            return Track.RotateDirection(Exit, Rotation);
        }

        public Track.TrackDir GetEntranceDir()
        {
            if (Rotation == Track.TrackRotation.Deg0) return Entrance;
            return Track.RotateDirection(Entrance, Rotation);
        }
    }

    private TrackTile[] _baseTrackTiles = [
        new (Track.TrackType.Straight),
        new (Track.TrackType.CornerCw),
        new (Track.TrackType.CornerCCw)
    ];

    private TrackTile[] _tracks = new TrackTile[10];

    public PackedScene GetTrackScene(Track.TrackType type)
    {
        switch (type)
        {
            case Track.TrackType.Straight:
                return TrackStraightScene;
            case Track.TrackType.CornerCw:
                return TrackCornerCwScene;
            case Track.TrackType.CornerCCw:
                return TrackCornerCCwScene;
            default:
                return null;
        }
    }

    public override void _Ready()
    {
        _tracks[0] = new TrackTile(Track.TrackType.Start);
        _tracks[0].TrackScene = TrackStartScene;

        // _tracks[1] = new TrackTile(Track.TrackType.Straight);
        // _tracks[1].TrackScene = TrackStraightScene;
        //
        // // The following tracks are for debugging and in the future will be generated using GenerateTrack
        // _tracks[2] = new TrackTile(Track.TrackType.CornerCw);
        // _tracks[2].Rotation = Track.TrackRotation.Deg180;
        // _tracks[2].TrackScene = TrackCornerCwScene;
        //
        // _tracks[3] = new TrackTile(Track.TrackType.CornerCw);
        // _tracks[3].Rotation = Track.TrackRotation.Deg270;
        // _tracks[3].TrackScene = TrackCornerCwScene;
        //
        // for (int i = 4; i < 6; i++)
        // {
        //     _tracks[i] = new TrackTile(Track.TrackType.Straight);
        //     _tracks[i].Rotation = Track.TrackRotation.Deg180;
        //     _tracks[i].TrackScene = TrackStraightScene;
        // }
        //
        // _tracks[6] = new TrackTile(Track.TrackType.CornerCw);
        // _tracks[6].TrackScene = TrackCornerCwScene;
        //
        // _tracks[7] = new TrackTile(Track.TrackType.CornerCw);
        // _tracks[7].Rotation = Track.TrackRotation.Deg90;
        // _tracks[7].TrackScene = TrackCornerCwScene;

        GD.Print("Track 1: " + _tracks[1].GetExitDir());

        GenerateTrack();
        SpawnTrack();

        // Inform Racers of the number of goals
        foreach (var racer in GetTree().GetRoot().GetNode("Game/Racers").GetChildren())
        {
            racer.Set("NumberOfGoals", _goalCounter - 1);
        }
    }

    public void GenerateTrack()
    {
        TrackTile lastTile = new TrackTile();
        for (int i = 1; i < _tracks.Length; i++)
        {
            _tracks[i] = _baseTrackTiles[GD.Randi() % _baseTrackTiles.Length];
            _tracks[i].TrackScene = GetTrackScene(_tracks[i].Type);

            if (lastTile.TrackScene != null)
            {
                while (lastTile.GetExitDir() != _tracks[i].GetEntranceDir())
                {
                    _tracks[i].Rotation += 1;
                }
            }

            lastTile = _tracks[i];
        }
    }

    public void SpawnTrack()
    {
        GD.Print("SpawnTrack");

        TrackTile lastTile = new TrackTile();

        float spawnX = 0, spawnY = 0;
        foreach (var tile in _tracks)
        {
            if (tile.TrackScene == null) continue;

            if (lastTile.TrackScene != null)
            {
                var lastTileExit = lastTile.GetExitDir();

                switch (lastTileExit)
                {
                    case Track.TrackDir.Top:
                        spawnY -= 500;
                        break;
                    case Track.TrackDir.Right:
                        spawnX += 500;
                        break;
                    case Track.TrackDir.Bottom:
                        spawnY += 500;
                        break;
                    case Track.TrackDir.Left:
                        spawnX -= 500;
                        break;
                }
            }

            Node2D scene = tile.TrackScene.Instantiate<Node2D>();
            AddChild(scene);

            scene.GlobalPosition = new Vector2(spawnX, spawnY);

            if (tile.Rotation != Track.TrackRotation.Deg0)
            {
                scene.GlobalRotationDegrees = (int)tile.Rotation * 90;
            }

            // TODO: Move this to OUTSIDE the for loop, otherwise bad performance :3
            foreach (var goal in scene.FindChildren("Goal"))
            {
                goal.Reparent(this);
                goal.Set("GoalNumber", _goalCounter);
                goal.Name = "Goal" + _goalCounter;
            }

            lastTile = tile;
            _goalCounter++;
        }
    }

}

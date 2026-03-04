using Godot;

public partial class GoalManager : Node2D
{

    [Export] public PackedScene StartingTrackScene;

    private int _goalCounter = 1;

    struct TrackTile
    {
        public Track.TrackType Type;
        public Track.TrackDir Entrance;
        public Track.TrackDir Exit;
        public PackedScene TrackScene;

        public TrackTile(Track.TrackType type)
        {
            Type = type;

            switch (type)
            {
                case Track.TrackType.CornerCw:
                    Entrance = Track.TrackDir.Top;
                    Exit = Track.TrackDir.Right;
                    break;
                case Track.TrackType.CornerCCw:
                    Entrance = Track.TrackDir.Top;
                    Exit = Track.TrackDir.Left;
                    break;
                case Track.TrackType.Straight:
                default:
                    Entrance = Track.TrackDir.Left;
                    Exit = Track.TrackDir.Right;
                    break;
            }
        }
    }

    private TrackTile[] _baseTrackTiles = [
        new (Track.TrackType.Straight),
        new (Track.TrackType.CornerCw),
        new (Track.TrackType.CornerCCw)
    ];

    private TrackTile[,] _tracks = new TrackTile[9, 9];

    public override void _Ready()
    {
        _tracks[4, 4] = new TrackTile(Track.TrackType.Straight);
        _tracks[4, 4].TrackScene = StartingTrackScene;

        GenerateTrack();

        SpawnTrack();
    }

    public void GenerateTrack()
    {
        
    }

    public void SpawnTrack()
    {
        GD.Print("SpawnTrack");
        foreach (var tile in _tracks)
        {
            if (tile.TrackScene == null) continue;

            var scene = tile.TrackScene.Instantiate();
            AddChild(scene);

            foreach (var goal in scene.FindChildren("Goal"))
            {
                goal.Reparent(this);
                goal.Set("goalNumber", _goalCounter);
                goal.Name = "Goal" + _goalCounter;
                _goalCounter++;
            }
        }
    }

}

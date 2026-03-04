using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GoalManager : Node2D
{

    [Export] public PackedScene TrackStartScene;

    [Export] public PackedScene TrackStraightScene;
    [Export] public PackedScene TrackCornerCwScene;
    [Export] public PackedScene TrackCornerCCwScene;

    private int _goalCounter = 1;
    private static int _maxRandomTracks = 20;

    struct TrackTile
    {
        public Track.TrackType Type;
        public Track.TrackDir Entrance;
        public Track.TrackDir Exit;
        public Track.TrackRotation Rotation;
        public PackedScene TrackScene;

        public int x = 0, y = 0;

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

    private TrackTile[] _tracks = new TrackTile[100];

    // TODO: Optimise :3
    public bool IsTrackTileOccupied(int x, int y)
    {
        foreach (TrackTile tile in _tracks) {
            if (tile.x == x && tile.y == y) return true;
        }
        return false;
    }

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

        GD.Print("Track 1: " + _tracks[1].GetExitDir());

        GenerateRandomTrack();
        var returnPath = PathfindToStart();
        GD.Print(returnPath);
        SpawnTrack();

        // Inform Racers of the number of goals
        foreach (var racer in GetTree().GetRoot().GetNode("Game/Racers").GetChildren())
        {
            racer.Set("NumberOfGoals", _goalCounter - 1);
        }
    }

    public void GenerateRandomTrack()
    {
        int localX = 0, localY = 1;
        TrackTile lastTile = _tracks[0];
        for (int i = 1; i < _maxRandomTracks; i++)
        {
            GD.Print("Generating Track Piece ", i);
            bool newTrackExitBlocked = false;

            int trackGenerationAttempt = 0;
            do
            {
                trackGenerationAttempt++;
                if (newTrackExitBlocked) GD.Print("Track blocked, regenerating...");
                if (trackGenerationAttempt >= 12)
                {
                    GD.Print("Attempted to generate track piece 12 times with no success, breaking and restarting...");
                    break;
                }
                _tracks[i] = _baseTrackTiles[GD.Randi() % _baseTrackTiles.Length];
                _tracks[i].x = localX;
                _tracks[i].y = localY;
                _tracks[i].TrackScene = GetTrackScene(_tracks[i].Type);

                if (lastTile.TrackScene != null)
                {
                    GD.Print(i, " Rotating Tile to fit ", Track.Opposite(lastTile.GetExitDir()), _tracks[i].GetEntranceDir());
                    int rotationCounter = 0;
                    while (rotationCounter < 4 && Track.Opposite(lastTile.GetExitDir()) != _tracks[i].GetEntranceDir())
                    {
                        GD.Print(i, " Rotating Tile to fit ", Track.Opposite(lastTile.GetExitDir()), _tracks[i].GetEntranceDir());
                        _tracks[i].Rotation += 1;
                        rotationCounter++;
                        if (rotationCounter >= 4)
                        {
                            GD.Print("Unable to rotate track into valid piece, forcing new track...");
                            newTrackExitBlocked = true;
                        }
                    }
                }

                if (newTrackExitBlocked) GD.Print("Track blocked, regenerating...");

                switch (_tracks[i].GetExitDir())
                {
                    case Track.TrackDir.Top:
                        newTrackExitBlocked = IsTrackTileOccupied(localX, localY + 1);
                        if (!newTrackExitBlocked) localY++;
                        break;
                    case Track.TrackDir.Bottom:
                        newTrackExitBlocked = IsTrackTileOccupied(localX, localY - 1);
                        if (!newTrackExitBlocked) localY--;
                        break;
                    case Track.TrackDir.Left:
                        newTrackExitBlocked = IsTrackTileOccupied(localX - 1, localY);
                        if (!newTrackExitBlocked) localX--;
                        break;
                    case Track.TrackDir.Right:
                        newTrackExitBlocked = IsTrackTileOccupied(localX + 1, localY);
                        if (!newTrackExitBlocked) localX++;
                        break;
                }
            } while (newTrackExitBlocked);

            lastTile = _tracks[i];

            if (trackGenerationAttempt >= 12) i = 1;
        }
    }

    static int Heuristic(Vector2I current, Vector2I goal)
    {
        return Mathf.Abs(current.X - goal.X) + Mathf.Abs(current.Y - goal.Y);
    }

    public List<Vector2I> PathfindToStart()
    {
        var lastTrack = _tracks[_maxRandomTracks - 1];
        var firstTrack = _tracks[0];
        GD.Print("Last Track " + lastTrack.x + " " + lastTrack.y);
        GD.Print("First Track " + firstTrack.x + " " + firstTrack.y);

        int startX = lastTrack.x, startY = lastTrack.y;
        int goalX = firstTrack.x, goalY = firstTrack.y - 1;

        switch (lastTrack.GetExitDir())
        {
            case Track.TrackDir.Top: startY++; break;
            case Track.TrackDir.Bottom: startY--; break;
            case Track.TrackDir.Left: startX--; break;
            case Track.TrackDir.Right: startX++; break;
        }

        GD.Print("Need to Pathfind from " + startX + ", " + startY + " to " + goalX + ", " + goalY);

        var start = new Vector2I(startX, startY);
        var goal = new Vector2I(goalX, goalY);

        var openList = new List<Vector2I> { start };
        var closedList = new HashSet<Vector2I>();

        var gScore = new Dictionary<Vector2I, int> { [start] = 0 };
        var hScore = new Dictionary<Vector2I, int> { [start] = Heuristic(start, goal) };
        var parentMap = new Dictionary<Vector2I, Vector2I>();

        while (openList.Count > 0)
        {
            var current = openList.OrderBy(track => gScore[track] + hScore[track]).First();
            if (current == goal)
            {
                // Return path reconstruction
                return ReconstructPath(parentMap, current);
            }

            openList.Remove(current);
            closedList.Remove(current);

            foreach (var neighbour in GetNeighbours(current))
            {
                if (closedList.Contains(current) || IsTrackTileOccupied(neighbour.X, neighbour.Y)) continue;

                int tentativeGScore = gScore[current] + Heuristic(neighbour, current);

                if (!gScore.ContainsKey(neighbour) || tentativeGScore < gScore[neighbour])
                {
                    gScore[neighbour] = tentativeGScore;
                    hScore[neighbour] = Heuristic(neighbour, goal);

                    parentMap[neighbour] = current;

                    if (!openList.Contains(neighbour)) openList.Add(neighbour);
                }
            }
        }

        return null;
    }

    private static List<Vector2I> ReconstructPath(Dictionary<Vector2I, Vector2I> parentMap, Vector2I current)
    {
        var path = new List<Vector2I> { current };
        while (parentMap.ContainsKey(current))
        {
            path.Add(parentMap[current]);
        }

        path.Reverse();
        return path;
    }

    // TODO: Vomit emoji
    private static List<Vector2I> GetNeighbours(Vector2I current)
    {
        var neighbours = new List<Vector2I>();
        neighbours.Add(new Vector2I(current.X, current.Y + 1));
        neighbours.Add(new Vector2I(current.X + 1, current.Y));
        neighbours.Add(new Vector2I(current.X, current.Y - 1));
        neighbours.Add(new Vector2I(current.X - 1, current.Y));

        return neighbours;
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

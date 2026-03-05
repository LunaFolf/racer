using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GoalManager : Node2D
{

    [Export] public PackedScene TrackStartScene;

    [Export] public PackedScene TrackStraightScene;
    [Export] public PackedScene TrackCornerCwScene;
    [Export] public PackedScene TrackCornerCCwScene;

    private static PackedScene _trackStraightScene;
    private static PackedScene _trackCornerCwScene;
    private static PackedScene _trackCornerCCwScene;

    private int _goalCounter = 1;
    private static int _maxRandomTracks = 25;

    public int GoalCounter => _goalCounter - 1;

    private List<Goal> _goals = new();
    public List<Goal> Goals => _goals;

    public float DistanceToGoal(Vector2 pos, int goalNumber)
    {
        var goal = Goals[goalNumber];
        return (pos - goal.Position).Length();
    }

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
            SetType(type);
        }

        public void SetType(Track.TrackType type)
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

    private TrackTile[] _tracks = new TrackTile[50];

    public bool IsTrackTileOccupied(int x, int y)
    {
        foreach (TrackTile tile in _tracks)
        {
            if (tile.TrackScene == null) continue;

            if (tile.x == x && tile.y == y)
                return true;
        }

        return false;
    }

    public static PackedScene GetTrackScene(Track.TrackType type)
    {
        switch (type)
        {
            case Track.TrackType.Straight:
                return _trackStraightScene;
            case Track.TrackType.CornerCw:
                return _trackCornerCwScene;
            case Track.TrackType.CornerCCw:
                return _trackCornerCCwScene;
            default:
                return null;
        }
    }

    public override void _Ready()
    {
        _trackStraightScene ??= TrackStraightScene;
        _trackCornerCwScene ??= TrackCornerCwScene;
        _trackCornerCCwScene ??= TrackCornerCCwScene;
    }

    public void StartGeneration()
    {
        _tracks[0] = new TrackTile(Track.TrackType.Start);
        _tracks[0].TrackScene = TrackStartScene;

        List<Vector2I> returnPath;

        do
        {
            GenerateRandomTrack();
            returnPath = PathfindToStart();

            if (returnPath == null) GD.PrintErr("Unable to generate a valid circuit, aborting...");
            else GD.Print("Pathfinding to start returned " + returnPath.Count + " points");
        } while (returnPath == null);

        GenerateReturnPathTrack(returnPath);

        SpawnTrack();
    }

    public void GenerateReturnPathTrack(List<Vector2I> path)
    {
        for (int i = 0; i < path.Count; i++) // Using for loop instead of foreach, so I can grab "next" track piece easily.
        {
            var current = path[i];

            TrackTile prev;
            if (i == 0)
                prev = _tracks[_maxRandomTracks - 1];
            else
                prev = _tracks[_maxRandomTracks + i - 1];

            var prevExitOpposite = Track.Opposite(prev.GetExitDir());
            var nextEntrance = GetTrackDir(current, new Vector2I(0, 0));

            var newTrackPiece = new TrackTile(Track.TrackType.Straight);

            if (i < path.Count - 1)
            {
                var next = path[i + 1];
                nextEntrance = GetTrackDir(current, next);
            }

            GD.Print(prevExitOpposite, " -> ", nextEntrance);
            newTrackPiece.SetType(GetExactTrackTile(prevExitOpposite, nextEntrance));
            newTrackPiece.TrackScene = GetTrackScene(newTrackPiece.Type);

            int rotationCounter = 0;
            while (rotationCounter < 4 && prevExitOpposite != newTrackPiece.GetEntranceDir())
            {
                GD.Print(i, " [RP] Rotating Tile to fit ", prevExitOpposite, newTrackPiece.GetEntranceDir());
                newTrackPiece.Rotation += 1;
                rotationCounter++;
                if (rotationCounter >= 4)
                {
                    GD.PrintErr("[RP] Unable to rotate track into valid piece, forcing new track...");
                    break;
                }
            }

            newTrackPiece.x = current.X;
            newTrackPiece.y = current.Y;

            _tracks[_maxRandomTracks + i] = newTrackPiece;
        }
    }

    public static Track.TrackType GetExactTrackTile(Track.TrackDir entrance, Track.TrackDir exit)
    {
        // CCW:
        // Bottom -> Left, Left -> Top, Top -> Right, Right -> Bottom
        if ((entrance == Track.TrackDir.Bottom && exit == Track.TrackDir.Left) ||
            (entrance == Track.TrackDir.Left && exit == Track.TrackDir.Top) ||
            (entrance == Track.TrackDir.Top && exit == Track.TrackDir.Right) ||
            (entrance == Track.TrackDir.Right && exit == Track.TrackDir.Bottom))
        {
            return Track.TrackType.CornerCCw;
        }

        // CW:
        // Top -> Left, Right -> Top, Bottom -> Right, Left -> Bottom
        if ((entrance == Track.TrackDir.Top && exit == Track.TrackDir.Left) ||
            (entrance == Track.TrackDir.Right && exit == Track.TrackDir.Top) ||
            (entrance == Track.TrackDir.Bottom && exit == Track.TrackDir.Right) ||
            (entrance == Track.TrackDir.Left && exit == Track.TrackDir.Bottom))
        {
            return Track.TrackType.CornerCw;
        }

        // Straight:
        // Bottom -> Top, Top -> Bottom, Left -> Right, Right -> Left
        if ((entrance == Track.TrackDir.Bottom && exit == Track.TrackDir.Top) ||
            (entrance == Track.TrackDir.Top && exit == Track.TrackDir.Bottom) ||
            (entrance == Track.TrackDir.Left && exit == Track.TrackDir.Right) ||
            (entrance == Track.TrackDir.Right && exit == Track.TrackDir.Left))
        {
            return Track.TrackType.Straight;
        }

        GD.PrintErr("[RP] Invalid track direction! Can't covert entrance: " + entrance + " exit: " + exit + " to a track type!");

        return Track.TrackType.Straight;
    }

    public Track.TrackDir GetTrackDir(Vector2I start, Vector2I end)
    {
        // Read the coords, check if we need a straight piece/corner/corner-cw
        // And if the piece needs to be rotated.
        // Coord will be in the form of (x, y), and could be moving from bottom to top, left to right, bottom to right, or any of the combos...

        int xDiff = end.X - start.X;
        int yDiff = end.Y - start.Y;

        GD.Print("xDiff: " + xDiff + ", yDiff: " + yDiff);

        if (xDiff < 0) return Track.TrackDir.Left;
        if (xDiff > 0) return Track.TrackDir.Right;
        if (yDiff > 0) return Track.TrackDir.Top;
        if (yDiff < 0) return Track.TrackDir.Bottom;

        throw new System.Exception("Invalid track direction! Can't covert xDiff: " + xDiff + " yDiff: " + yDiff + " to a direction!");
    }

    // Generate several random track pieces, that don't cause collisions with existing pieces,
    // so that the start of the circuit is more interesting.
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
                var randomType = _baseTrackTiles[GD.Randi() % _baseTrackTiles.Length].Type;
                _tracks[i] = new TrackTile(randomType);
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
                            GD.PrintErr("Unable to rotate track into valid piece, forcing new track...");
                            newTrackExitBlocked = true;
                        }
                    }
                }

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
            } while (trackGenerationAttempt <= 12 && newTrackExitBlocked);

            lastTile = _tracks[i];

            if (trackGenerationAttempt >= 12 || newTrackExitBlocked)
            {
                lastTile = _tracks[0];
                i = 0;
                localX = 0;
                localY = 1;
            };
        }
    }

    static int Heuristic(Vector2I current, Vector2I goal)
    {
        return Mathf.Abs(current.X - goal.X) + Mathf.Abs(current.Y - goal.Y);
    }

    // Pathfind from the end of the randomly generated track, back to the starting line,
    // so that we have a complete circuit for our race.
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

        int loopCount = 0;

        while (openList.Count > 0 && loopCount < 1000)
        {
            loopCount++;
            if (loopCount >= 1000)
            {
                GD.PrintErr("Got stuck in an infinite loop for pathfinding, breaking and restarting...");
                return null;
            }
            var current = openList.OrderBy(track => gScore[track] + hScore[track]).First();
            if (current.Equals(goal))
            {
                // Return path reconstruction
                return ReconstructPath(parentMap, current);
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach (var neighbour in GetNeighbours(current))
            {
                if (closedList.Contains(neighbour) || IsTrackTileOccupied(neighbour.X, neighbour.Y)) continue;

                int tentativeGScore = gScore[current] + 1;

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
            current = parentMap[current];
            path.Add(current);
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

    // Generate the track based on the given pieces, rotation, etc., in _tracks.
    public void SpawnTrack()
    {
        GD.Print("SpawnTrack");

        TrackTile lastTile = new TrackTile();

        foreach (var tile in _tracks)
        {
            if (tile.TrackScene == null) continue;

            Node2D scene = tile.TrackScene.Instantiate<Node2D>();
            AddChild(scene);

            scene.GlobalPosition = new Vector2(tile.x * 500, -tile.y * 500);

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
                _goals.Add((Goal)goal);
            }

            lastTile = tile;
            _goalCounter++;
        }
    }

}

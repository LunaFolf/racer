using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

public partial class Minimap : Node2D
{
    [Export] public Vector2[] TrackPoints = [];
    [Export] public Line2D TrackLine;
    [Export] public Sprite2D PlayerDot;
    [Export] public float RotationSpeed = 2f;
    private Player _player;
    private List<Sprite2D> RacerDots;
    private RacerManager RaceManager;
    private Vector2 _startingPoint;
    private float _scale;

    private float _targetMapSize = 192;

    public override void _Ready()
    {
        UpdateLine();
    }

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    public void SetRaceManager(RacerManager RaceManager)
    {
        this.RaceManager = RaceManager;
        SetRacers();
    }

    public void SetRacers()
    {
        RacerDots = new();

        foreach (var racer in RaceManager.Racers)
        {
            var color = racer.Modulate;
            color.A = .75f;

            var sprite = new Sprite2D();
            sprite.Name = racer.Name;
            sprite.Modulate = color;
            sprite.Texture = new CanvasTexture();
            sprite.RegionEnabled = true;
            sprite.RegionRect = new Rect2(0, 0, 5, 5);
            TrackLine.AddChild(sprite);
            RacerDots.Add(sprite);
        }
    }

    public void SetMap(Vector2[] points)
    {
        TrackPoints = points;
        UpdateLine();
    }

    public override void _Process(double delta)
    {
        

        var trackLength = 500;
        var playerMovementScale = trackLength / _scale;

        if (_player != null)
        {
            PlayerDot.Position = _startingPoint + (_player.Position / playerMovementScale);
            RotationDegrees = Mathf.Lerp(RotationDegrees, -_player.RotationDegrees, (float)delta * RotationSpeed);
        }

        if (RaceManager.Racers != null)
        {
            foreach (var racer in RaceManager.Racers)
            {
                var index = racer.RacerNumber - 1;
                RacerDots[index].Position = _startingPoint + (racer.Position / playerMovementScale);
            }
        }

        
    }

    private void UpdateLine()
    {
        TrackLine.ClearPoints();

        float smallX = float.MaxValue, smallY = float.MaxValue;
        float bigX = float.MinValue, bigY = float.MinValue;

        foreach (var point in TrackPoints)
        {
            if (point.X < smallX) smallX = point.X;
            if (point.Y < smallY) smallY = point.Y;

            if (point.X > bigX) bigX = point.X;
            if (point.Y > bigY) bigY = point.Y;
        }

        var pointWidth = bigX - smallX;
        var pointHeight = bigY - smallY;

        var xMult = _targetMapSize / pointWidth;
        var yMult = _targetMapSize / pointHeight;
        _scale = Mathf.Min(xMult, yMult);
        var scaledWidth = pointWidth * _scale;
        var scaledHeight = pointHeight * _scale;

        var offsetX = (_targetMapSize - scaledWidth) / 2f;
        var offsetY = (_targetMapSize - scaledHeight) / 2f;

        var first = true;
        foreach (var point in TrackPoints)
        {
            var normalized = new Vector2(point.X - smallX, -(point.Y - smallY));
            var scaled = normalized * _scale;
            var centered = scaled + new Vector2(offsetX, offsetY);

            TrackLine.AddPoint(centered);

            if (first)
            {
                first = false;
                _startingPoint = centered;
            }
        }
    }
}

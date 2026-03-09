using Godot;
using System;
using Godot.Collections;

public partial class Minimap : Node2D
{
    [Export] public Vector2[] TrackPoints = [];
    [Export] public Line2D TrackLine;

    private float _targetMapSize = 192;

    public override void _Ready()
    {
        UpdateLine();
    }

    public void SetMap(Vector2[] points)
    {
        TrackPoints = points;
        UpdateLine();
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
        var scale = Mathf.Min(xMult, yMult);
        var scaledWidth = pointWidth * scale;
        var scaledHeight = pointHeight * scale;

        var offsetX = (_targetMapSize - scaledWidth) / 2f;
        var offsetY = (_targetMapSize - scaledHeight) / 2f;

        foreach (var point in TrackPoints)
        {
            var normalized = new Vector2(point.X - smallX, -(point.Y - smallY));
            var scaled = normalized * scale;
            var centered = scaled + new Vector2(offsetX, offsetY);

            TrackLine.AddPoint(centered);
        }
    }
}

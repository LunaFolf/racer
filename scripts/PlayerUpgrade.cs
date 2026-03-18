using Godot.Collections;
using System;

public class PlayerUpgrade
{
    public enum Type
    {
        SPEED,
        TRACTION,
        TURNING,
        BOOST
    }

    private Dictionary<Type, String[]> _upgradeNames = new() {
        [Type.SPEED] = ["Two Hamster Wheels", "V67 Engine", "Racing Stripes"],
        [Type.TRACTION] = ["Glue Tyres", "MAGNETS!!", "Artificial Gravity"],
        [Type.TURNING] = ["PIVOT!", "361 Degrees", "A.I Ball-joints"],
        [Type.BOOST] = ["Yeet Fuel", "Literal Rocket Fuel", "Monster Energy™"]
    };

    public readonly Type type;
    public readonly float multiplier = 0f;
    public readonly string name;

    public PlayerUpgrade(Type _type, float _multiplier)
    {
        type = _type;
        multiplier = _multiplier;

        var possibleNames = _upgradeNames[type];
        name = possibleNames[possibleNames.Length - 1];
    }
}
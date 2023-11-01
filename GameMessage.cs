using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Application;

public enum ActionTypes
{
    Rotate,
    LookAt,
    Shoot
}

public enum MeteorType
{
    Large,
    Medium,
    Small
}

public record Vector(double X, double Y);

public record Projectile(string Id, Vector Position, Vector Velocity, int Size);

public record Meteor(string Id, Vector Position, Vector Velocity, int Size, MeteorType MeteorType)
    : Projectile(Id, Position, Velocity, Size);

[JsonDerivedType(typeof(ActionRotate))]
[JsonDerivedType(typeof(ActionLookAt))]
[JsonDerivedType(typeof(ActionShoot))]
public abstract record Action(ActionTypes Type);

public record ActionRotate(double Angle) : Action(ActionTypes.Rotate);

public record ActionLookAt(Vector Target) : Action(ActionTypes.LookAt);

public record ActionShoot() : Action(ActionTypes.Shoot);

public record MeteorInfos(
    int Score,
    int Size,
    double ApproximateSpeed,
    ExplodesInto[] ExplodesInto
);

public record ExplodesInto(MeteorType MeteorType, double ApproximateAngle);

public record World(int Width, int Height);

public record Rocket(int Speed, int Size);

public record Constants(World World, Rocket Rockets, Dictionary<MeteorType, MeteorInfos> MeteorInfos);

public record Cannon(Vector Position, double Orientation, int Cooldown);

public record GameMessage(
    Constants Constants,
    Cannon Cannon,
    Meteor[] Meteors,
    Projectile[] Rockets,
    string[] LastTickErrors,
    int Score,
    int Tick
);

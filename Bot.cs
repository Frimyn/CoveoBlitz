using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;

namespace Application;

public class Bot
{
    public const int CannonWaitTime = 10;
    public const string NAME = "MyBot C#";
    private Dictionary<string,MeteorSnapshot> NotedMeteors = new();
    private List<Action[]> actions = new();
    /// <summary>
    /// This method should be use to initialize some variables you will need throughout the game.
    /// </summary>
    public Bot()
    {
        Console.WriteLine("Initializing your super mega bot!");
    }

    /// <summary>
    /// Here is where the magic happens, for now the moves are random. I bet you can do better ;)
    /// </summary>
    public Action[] GetNextMove(GameMessage gameMessage)
    {

        try
        {

            foreach(Meteor meteor in gameMessage.Meteors.Where<Meteor>(x=> x.MeteorType == MeteorType.Large && !NotedMeteors.ContainsKey(x.Id)))
            {
                //Il y a une nouvelle astéroide qu'on doit noter.
                //Noted meteors pourrait être enlevé si on veut
                NotedMeteors.Add(meteor.Id, new MeteorSnapshot(meteor, gameMessage.Tick));
            }

            foreach(string id in NotedMeteors.Keys.Where(x=> !gameMessage.Meteors.Any(y=> y.Id == x)))
            {
                //une météorite qu'on a noté n'est plus sur la carte.
                NotedMeteors.Remove(id);
            }

            //Lorsque le cannon est en pause, il n'y a pas d'action pertinentes a prendre.
            if (gameMessage.Cannon.Cooldown > 0) return new Action[] {};


            //S'il n'a pas d'actions stockés et qu'il y a des grosses astéroides sur la carte.
            //L'idée serait d'enchainer des actions pour s'occuper de l'astéroide avec le plus de potentiel.
            //Une grosse astéroide prend au max 9 actions pour tirer complètement (90)
            if (!actions.Any() && NotedMeteors.Any())
            {

                var highestId = NotedMeteors.Keys.OrderByDescending(pair => int.Parse(pair)).First();
                actions.Add(SetupShotsForLargeMeteor(NotedMeteors[highestId], gameMessage));
            }
            if (actions.Any())
            {
                var nextAction = actions.First();
                actions.Remove(nextAction);
                return nextAction;
            }
            
            return new Action[] {};

        }catch(Exception e)
        {
            //Permettre de voir dans les logs lorsqu'on fait une erreur de prog.
            Console.WriteLine(e);
            throw;
        }


    }

    /// <summary>
    /// Renvoie les coordonées que le cannon devra viser pour détruire la météorite voulu
    /// </summary>
    /// <param name="target"></param>
    /// <param name="gameMessage"></param>
    /// <param name="additionalWaitTime">
    ///     Est utilisé s'il y a déjà des shots en attente. CooldownCannon * shots déjà en attente.
    /// </param>
    /// <returns>
    ///     L'endroit où le cannon devra viser pour atteindre la météorite.
    /// </returns>
    private Action[] SetupShotsForLargeMeteor(MeteorSnapshot target, GameMessage gameMessage, int additionalWaitTime = 0)
    {
        Vector targetVelocity = target.meteor.Velocity;

        //Si on enlève noted meteor nous n'avons pas de savoir le timespent since. Il serait seulement gameMessage.Cannon.Cooldown + additionalWaitTime;
        double timeSpentSinceSnapshot = gameMessage.Tick - target.timeTick + gameMessage.Cannon.Cooldown + additionalWaitTime;
        Vector targetMovementSinceSnapshot = new Vector(targetVelocity.X * timeSpentSinceSnapshot, targetVelocity.Y * timeSpentSinceSnapshot);

        Vector currentTargetPosition = AddVector(target.meteor.Position, targetMovementSinceSnapshot);

        Vector relativePosition = new Vector(currentTargetPosition.X - gameMessage.Cannon.Position.X, currentTargetPosition.Y - gameMessage.Cannon.Position.Y);

        double distanceToTarget = Math.Sqrt(relativePosition.X * relativePosition.X + relativePosition.Y * relativePosition.Y);
        double timeToReachTarget = distanceToTarget / gameMessage.Constants.Rockets.Speed;

        double futureX = currentTargetPosition.X + targetVelocity.X * timeToReachTarget;
        double futureY = currentTargetPosition.Y + targetVelocity.Y * timeToReachTarget;

        Vector aimSpot =  new Vector(futureX, futureY);

        Console.WriteLine($"Shooting meteor ID : {target.meteor.Id}");
        Console.WriteLine($"Current Target Position : {currentTargetPosition}");
        Console.WriteLine($"Aim Spot : {aimSpot}");
        Console.WriteLine($"Time to reach target : {timeToReachTarget}");
        Console.WriteLine($"target velocity : {targetVelocity}");

        //Setup shots for Medium Meteors and Small meteors



        return new Action[] { new ActionLookAt(aimSpot), new ActionShoot() };

    }

    public static Double Dot(Vector a, Vector b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    public static Vector AddVector(Vector a, Vector b)
    {
        return new Vector(a.X + b.X, a.Y + b.Y);
    }

    public static Vector SubstractVector(Vector a, Vector b)
    {
        return new Vector(a.X - b.X, a.Y - b.Y);
    }

}

public class MeteorSnapshot
{
    public double timeTick;
    public Meteor meteor;
    public MeteorSnapshot(Meteor meteor, double timeTick)
    {
        this.meteor = meteor;
        this.timeTick = timeTick;
    }
}

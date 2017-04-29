using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : Actor
{
    public override ActorType GetActorType()
    {
        return ActorType.Obstacle;
    }
}

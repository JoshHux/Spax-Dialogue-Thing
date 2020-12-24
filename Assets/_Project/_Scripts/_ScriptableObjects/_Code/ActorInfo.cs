using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ActorInfo", menuName = "DialogueStuff/DialogueActorInfo", order = 1)]
public class ActorInfo : ScriptableObject
{
    public string charName;
    public Sprite[] emotes;
}

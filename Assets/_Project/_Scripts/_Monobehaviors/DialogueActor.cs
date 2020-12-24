using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DialogueActor : MonoBehaviour
{
    //Scriptable Object that holds all the character's relevant information
    public ActorInfo info;

    //renderer on object, don't name it "renderer" because of an inherited member
    public SpriteRenderer spriteRenderer;

    //formally really important, now only really handles scaling
    private Sequence actorTweens;

    //'Global' queue for animations. First one ( .Peek() ) is playing, others are waiting in queue
    //main way to add tweens to the actor
    public Queue<Sequence> TweenQueue = new Queue<Sequence>();

    //set to true when you want the tweens to forcefully be completed
    private bool forceComplete;

    // Start is called before the first frame update
    void Start()
    {
        //gets the attached SpriteRenderer component
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        //instantiating the tween for movements and scaling stuff
        actorTweens = DOTween.Sequence();

        //no forcing, yet
        forceComplete = false;
    }

    //changes the displayed emote by getting the sprite from an array of index i and setting it to the sprite renderer's rendering sprite
    public void ChangeEmote(int i)
    {
        spriteRenderer.sprite = info.emotes[i];
    }

    //will append a scale tween to highlight the actor
    public void ScaleActor(bool speaking)
    {
        //this comment is retroactively added: I had no idea that DOLocalMove existed, and I'm so angry that I didn't know because it would've saved me so much grief
        //appends the actor's movement to the tween
        if (speaking)
        {
            //make larger
            actorTweens
                .Join(transform.DOScale(new Vector3(100f, 100f, 0f), 0.05f));
        }
        else
        {
            //not speaking, make smaller
            actorTweens
                .Join(transform.DOScale(new Vector3(90f, 90f, 0f), 0.05f));
        }
    }

    //wrapper that will call a method to append a move and attach the regular OnComplete callback
    public void MoveActor(float localPos, float duration, bool snap)
    {
        //appends the move tween to the queue
        Sequence seq = AppendMoveTween(localPos, duration, snap);

        //Set callback to a regular OnComplete callback
        seq.OnComplete (OnComplete);
    }

    //wrapper that will automatically move the actor offscreen, based off of where the actor currently is
    //and attach a KillActor callback
    public void MoveOffScreen()
    {
        //store the off screen position the actor should go
        float offScreenPos = 1500f;

        //if character is on left half, then exit off the left side
        if (transform.localPosition.x < 0)
        {
            //off the screen on the left side
            offScreenPos = -1500f;
        }

        //appends a tween to move off screen
        Sequence seq = AppendMoveTween(offScreenPos, 0.05f, false);

        //appends the callback to kill the actor on completion
        seq.OnComplete (KillActor);
    }

    //call to add a tween to move in the queue
    //returns the created move tween
    private Sequence AppendMoveTween(float localPos, float duration, bool snap)
    {
        //this comment is retroactively added: I had no idea that DOLocalMove existed, and I'm so angry that I didn't know because it would've saved me so much grief
        //appends the actor's movement to the tween
        //Create paused sequence
        Sequence seq = DG.Tweening.DOTween.Sequence();

        //will remain paused for as long as it isn't its turn to play
        seq.Pause();

        //Append everything you want
        seq.Append(transform.DOLocalMoveX(localPos, duration, snap));

        //Add to queue
        TweenQueue.Enqueue (seq);

        //Check if this animation is the only item in the queue (waiting for nothing else)
        if (TweenQueue.Count == 1)
        {
            //plays if this is the only tween in the queue
            TweenQueue.Peek().Play();
        }

        //return the queue we created
        return seq;
    }

    //call to force all queued tweens and scaling to automatically finish
    public void ForceCompleteTweens()
    {
        //want to start forcing tweens to finish
        forceComplete = true;

        //if scaling tween is active
        if (actorTweens.IsActive())
        {
            //forces scaling tweens to finish
            actorTweens.Complete();
        }

        //if there's animations in queue left
        if (TweenQueue.Count > 0)
        {
            //hold the sequence for easier reference
            Sequence seq = TweenQueue.Peek();

            //safety measure, don't wanna force an inactive tween
            if (seq.IsActive())
            {
                //Debug.Log("forcing :: " + TweenQueue.Count);
                //complete next
                seq.Complete();
            }
            else
            {
                //coner case precaution, never likely reached
                forceComplete = false;
            }
        }
        else
        {
            //no animations left, turning of force completion just in case
            forceComplete = false;
        }
    }

    //callback
    //call to destroy this actor
    private void KillActor()
    {
        //Debug.Log("Kill processing");
        //if the scaling tween is still going on, just kill it
        //just in case we destroy the transform before the tween ends
        if (actorTweens.IsActive())
        {
            actorTweens.Kill();
        }

        //destroy this gameobject
        Destroy (gameObject);
    }

    //Callback
    //dequeues tween and plays next one
    //always attach to queued tween
    private void OnComplete()
    {
        //Debug.Log("completed");
        //remove animation that was completed
        TweenQueue.Dequeue();

        //if there's animations in queue left
        if (TweenQueue.Count > 0)
        {
            //hold the sequence for easier reference
            Sequence seq = TweenQueue.Peek();

            //play next
            seq.Play();

            //if we want all tweens to automatically be completed
            //the isActive check is just a safety measure
            if (forceComplete)
            {
                //safety measure, don't wanna force an inactive tween
                if (seq.IsActive())
                {
                    //Debug.Log("forcing :: " + TweenQueue.Count);
                    //forces a tween's completion
                    seq.Complete();
                }
                else
                {
                    //coner case precaution, never likely reached
                    forceComplete = false;
                }
            }
        }
        else
        {
            //no animations left, turning of force completion just in case
            forceComplete = false;
        }
    }
}

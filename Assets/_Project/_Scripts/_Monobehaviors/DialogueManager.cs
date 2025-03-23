using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Menu;

public class DialogueManager : MonoBehaviour
{
    //text object to display dialogue
    public TMP_Animated text;

    //text object to display names
    public TMP_Animated nameText;

    //text object to display Choices
    public TMP_Animated choiceText;

    //object that displays the dialogue choices
    public GameObject choiceContainer;
    //object that indicates what choice is selectec
    public GameObject choiceIndicator;

    //parents the active dialogue actors
    public GameObject actorContainer;

    //enable when dialogue line is done
    public GameObject endedIndicator;

    //parent transform of dialogue
    public Transform dialogueParent;

    //story that will be produced from the JSON file
    private Story story;

    //JSON file output by compiling the ink file
    public TextAsset textStuff;

    //holds the currently active actors moving around the screan
    private List<DialogueActor> actors;

    //are we doing dialogue right now?
    //public bool inDialogue;

    //are we making a choice right now?
    public bool makingChoice;

    private Choice m_selectedChoice;

    // Start is called before the first frame update
    void Start()
    {
        //adds a listener delegate
        //Called when emote of an actor changes
        text.emoteEvent += EmoteListener;

        //Called when emote of an actor changes
        text.speakerEvent += SpeakerListener;

        //Called when actor is supposed to move
        text.moveEvent += MoveListener;

        //Called when actor is supposed to face left or right
        text.faceEvent += FaceListener;

        //called when an actor is supposed to be instantiated
        text.enterEvent += EnterListener;

        //called when an actor is supposed to be instantiated
        text.exitEvent += ExitListener;

        //Called when dialogue finishes displaying
        text.onDialogueFinish += EndListener;

        //Called when dialogue starts displaying
        text.onDialogueStart += StartListener;

        //starts of not in a dialogue scene
        //inDialogue = false;

        //we're not expecting to make a decision yet
        makingChoice = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (EnumHelper.IsMostOuterLayer(MenuLevel.DIALOGUE, MenuManager.Instance.Level))
        {
            //plceholder to move index up or down depending on the selected item
            if (this.makingChoice)
            {

                //this if statement is here so that we dont' need to run the following code every update while making a choice
                //probably unecessarty since I doubt that this code generates that much work for it to be a concern, but it's nearly midnight and this is for peace of mind
                if (Keyboard.current[Key.S].wasPressedThisFrame || Keyboard.current[Key.W].wasPressedThisFrame)
                {
                    //just some index shenanigans to make a scrolling select
                    List<Choice> choices = this.story.currentChoices;

                    int curIndex = this.m_selectedChoice.index;
                    if (Keyboard.current[Key.S].wasPressedThisFrame)
                    {
                        curIndex += 1;
                    }
                    else if (Keyboard.current[Key.W].wasPressedThisFrame)
                    {
                        curIndex -= 1;
                    }

                    curIndex = (curIndex + choices.Count) % choices.Count;

                    this.m_selectedChoice = choices[curIndex];

                    //set the position of the selection thing
                    this.choiceIndicator.transform.localPosition = new Vector3(this.choiceIndicator.transform.localPosition.x, 125 - this.m_selectedChoice.index * 68, this.choiceIndicator.transform.localPosition.z);
                }
            }

            //placeholder function sdvance dialogue for testing purposes
            if (Keyboard.current[Key.Space].wasPressedThisFrame)
            {
                //if the text is in the middle of displaying something
                if (text.isTyping)
                {
                    //skip the text
                    SkipLine();
                }
                else
                {
                    //load the next line
                    LoadNextStoryLine();
                }
            } //placeholder, starts dialogue if none is going on

        }
        //we only want to overwrite the control if we're in the gameplay level
        else if (Keyboard.current[Key.Enter].wasPressedThisFrame && !EnumHelper.IsMostOuterLayer(MenuLevel.DIALOGUE, MenuManager.Instance.Level))
        {
            Debug.Log("starting dialogue");
            StartDialogue();
        }
    }

    //call when you want to skip the currently displaying line
    private void SkipLine()
    {
        //tell the text display to skip displaying text
        text.SkipText();

        //calling this to complete any ongoing tweens
        CompleteActorTweens();
    }

    //set TextAsset before calling to start dialogue
    private void StartDialogue()
    {
        //moves dialogue panel into place
        dialogueParent.DOLocalMoveY(-364, 0.05f);

        //starting dialogue
        //inDialogue = true;

        //setting up the story object from its respective JSON
        story = new Story(textStuff.text);

        //initializes list
        actors = new List<DialogueActor>();

        //initalizes story and sets it to the first needed line
        //calling the next line to actually start the dialogue
        LoadNextStoryLine();

        MenuManager.Instance.EnterDialogue();
    }

    //call to end the dialogue
    private void EndDialogue()
    {
        //holding array length
        int length = actors.Count;
        for (int i = 0; i < length; i++)
        {
            //holds the DialogueActor object we want to use, easier access
            DialogueActor da = actors[0];

            //removes actor from the array at index 0, since removing an element will reduce the number of remaining elements, 0 index will always have an element
            actors.RemoveAt(0);

            //destroys the actor object and moves it offscreen
            da.MoveOffScreen();
        }

        //moves dialogue panel into place
        dialogueParent.DOLocalMoveY(-900, 0.1f);

        //no longer in the middle of dialogue
        //inDialogue = false;

        MenuManager.Instance.ExitDialogue();
    }

    //call this method to read the next line of dialogue
    private void LoadNextStoryLine()
    {
        //turns off the indicator to be invisible
        endedIndicator.SetActive(false);

        if (this.makingChoice)
        {
            this.makingChoice = false;
            this.choiceContainer.SetActive(false);

            this.story.ChooseChoiceIndex(this.m_selectedChoice.index);
        }

        //if there is another line to display
        if (story.canContinue)
        {
            //get ned
            string line = story.Continue();

            text.ReadText(line);

        }
        else
        //no more lines to display, end dialogue
        {
            EndDialogue();
        }
    }

    //call to display the current choices of the story
    private void LoadChoices()
    {
        //Debug.Log("entered choice");

        this.makingChoice = true;
        this.choiceContainer.SetActive(true);

        this.choiceText.text = "";

        List<Choice> choices = this.story.currentChoices;

        for (int i = 0; i < choices.Count; i++)
        {
            this.choiceText.text += choices[i].text + "\n";
        }

        //by default, the choice should be the first
        this.m_selectedChoice = choices[0];

    }

    //callback
    //for when actor is supposed to change emote
    private void EmoteListener(object sender, TextIntEventArgs e)
    {
        //finds the sigular actor we want to manipulate
        DialogueActor da = FindActor(e.name);

        //makes sure that the found actor isn't null
        if (da != null)
        {
            //changes the emote to the wnated one if the names match
            da.ChangeEmote(e.value);
        }
    }

    //callback
    //for when a speaker is supposed to be highlighted
    private void SpeakerListener(object sender, TextStringEventArgs e)
    {
        //if no character name is given
        if (e.name == "")
        {
            //display the name in the text, it'll become obvious why if you look at the test dialogue
            nameText.text = "";
        }

        //we don't use FindActor here, since we want to do something to all the actors, regardless of whether or not it's the assigned speaker
        //holding array length
        int length = actors.Count;
        for (int i = 0; i < length; i++)
        {
            //holds the DialogueActor object we want to use, faster access
            DialogueActor da = actors[i];

            //holds the sprite renderer component for faster access, need it regardless of whether or not it's a speaker
            SpriteRenderer renderer = da.spriteRenderer;

            //checks if the names match
            if (da.info.charName == e.name)
            {
                //scales the speaker to highlight
                //da.gameObject.transform.localScale =new Vector3(100f, 100f, 0f);
                da.ScaleActor(true);

                //basically makes there is no filter on the speaker's sprite
                renderer.color = Color.white;

                //don't set it to 1 or the speaker will appear above the textbox
                renderer.sortingLayerName = "UI/Speaker";

                //if no display name is given
                if (e.display == "")
                {
                    //display the name in the scriptable object
                    nameText.text = da.info.charName;
                }
                else
                {
                    //display the display name
                    nameText.text = e.display;
                }
            }
            else
            {
                //scales non-speaker down
                //da.gameObject.transform.localScale = new Vector3(90f, 90f, 0f);
                da.ScaleActor(false);

                //puts grey filter over sprite to un-highlight the nonspeaker
                renderer.color = Color.grey;

                //pushes the nonspeaker back
                renderer.sortingLayerName = "UI/NonSpeaker";
            }
        }
    }

    //callback
    //for when you want an actor to move
    private void MoveListener(object sender, TextXYBoolEventArgs e)
    {
        //finds the sigular actor we want to manipulate
        DialogueActor da = FindActor(e.name);

        //makes sure that the found actor isn't null
        if (da != null)
        {
            //move only the character along the x-axis
            //x is postion, y is duration, condition is snapping
            da.MoveActor(e.x, e.y, e.condition);
        }
    }

    //callback
    //for when you want an actor to face left or right (default right)
    private void FaceListener(object sender, TextIntEventArgs e)
    {
        //finds the sigular actor we want to manipulate
        DialogueActor da = FindActor(e.name);

        //makes sure that the found actor isn't null
        if (da != null)
        {
            //holds the sprite renderer component for faster access
            SpriteRenderer renderer = da.spriteRenderer;

            //request wants actor to face in negative x
            if (e.value < 0)
            {
                //the sprites, by default, face left, so set the flip to false to face left
                renderer.flipX = false;
            }
            else
            {
                //set to true so the sprite flips to face right
                renderer.flipX = true;
            }
        }
    }

    //callback
    //for when you want an actor to enter
    private void EnterListener(object sender, TextIntEventArgs e)
    {
        //hold gameobject that is being instantiated
        GameObject go =
            Instantiate((GameObject)Resources.Load(e.name)) as GameObject;

        //gets the dialogue actor component
        DialogueActor da = go.GetComponent<DialogueActor>();

        //set the parent to the parent container
        go.transform.SetParent(actorContainer.transform);

        //request wants actor to be off screen in the negative x direction
        if (e.value < 0)
        {
            //move only the character along the x-axis
            go.transform.localPosition = new Vector3(-1500f, 0f, 0f);
        }
        else
        {
            //move only the character along the x-axis
            go.transform.localPosition = new Vector3(1500f, 0f, 0f);
        }

        //assuming, it doesn't enter speaking, scales non-speaker down
        go.transform.localScale = new Vector3(90f, 90f, 0f);

        //holds the sprite renderer component for faster access
        SpriteRenderer renderer = da.spriteRenderer;

        //puts grey filter over sprite to un-highlight the nonspeaker
        renderer.color = Color.grey;

        //add object to the List
        actors.Add(da);
    }

    //callback
    //for when actor is supposed to exit
    private void ExitListener(object sender, TextIntEventArgs e)
    {
        //finds the sigular actor we want to manipulate
        DialogueActor da = FindActor(e.name);

        //makes sure that the found actor isn't null
        if (da != null)
        {
            //get the game object of actor for faster access
            GameObject go = da.gameObject;

            //removes the actor from the list, doens't destroy them
            actors.Remove(da);

            //destroys the actor object
            da.MoveOffScreen();
        }
    }

    //callback
    //for when text is starting to display, called before any tags are read
    private void StartListener(object sender)
    {
        //calling this to prepare the actors for any upcoming tween tags
        CompleteActorTweens();
    }

    //callback
    //for when the dialogue finishes displaying
    private void EndListener(object sender)
    {
        //turns on the indicator so that it's visible
        endedIndicator.SetActive(true);

        //this is when to display choices (if any)
        if (story.currentChoices.Count > 0)
        {
            this.LoadChoices();
        }
    }

    //Utility, call to force the tweens on the actors to complete
    private void CompleteActorTweens()
    {
        //quick loop
        //holding array length
        int length = actors.Count;
        for (int i = 0; i < length; i++)
        {
            //holds the DialogueActor object we want to use, easier access
            DialogueActor da = actors[i];

            //forces actor's tweens to all complete
            da.ForceCompleteTweens();
        }
    }

    //utility, for finding a singular actor, returns null if no actor is found
    private DialogueActor FindActor(string name)
    {
        //holding array length
        int length = actors.Count;
        for (int i = 0; i < length; i++)
        {
            //holds the DialogueActor object we want to use, faster access
            DialogueActor da = actors[i];

            //checks if the names match
            if (da.info.charName == name)
            {
                //returns the found DialogueActor
                return da;
            }
        }

        //returns nothing, no actor found
        return null;
    }
}
//374

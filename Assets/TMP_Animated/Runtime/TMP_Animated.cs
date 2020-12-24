using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextIntEventArgs
{
    public TextIntEventArgs(string nm,int val) { name = nm;value = val; }
    //Name identifier of the character
    public string name { get; } // readonly
    //int value being passed
    public int value { get; } // readonly
}

public class TextStringEventArgs
{
    public TextStringEventArgs(string nm,string dspl) { name = nm;display = dspl; }
    public string name { get; } // readonly
    //WHat the name should be displayed as
    public string display { get; } // readonly
}

public class TextfloatEventArgs
{
    public TextfloatEventArgs(string nm,float pos) { name = nm;position = pos; }
    //Name identifier of the character
    public string name { get; } // readonly
    //position on the x-axis the actor moves to
    public float position { get; } // readonly
}


public class TextXYEventArgs
{
    public TextXYEventArgs(string nm,float xVal,float yVal) { name = nm;x = xVal;y = yVal; }
    //Name identifier of the character
    public string name { get; } // readonly
    //one of two x and y values
    public float x { get; } // readonly
    //one of two x and y values
    public float y { get; } // readonly
}

public class TextXYBoolEventArgs:TextXYEventArgs
{
    public TextXYBoolEventArgs(string nm,float xVal,float yVal, bool cond):base(nm,xVal,yVal) { condition=cond; }
    //Name identifier of the character
    public bool condition { get; } // readonly
}



namespace TMPro
{

    //[System.Serializable] public class TextRevealEvent : UnityEvent<char> { }

    //declaring the delegate handlers
    //for passing any int calue with name from the text
    public delegate void TextIntEventHandler(object sender, TextIntEventArgs e);

    //for changing speaker name
    public delegate void TextStringEventHandler(object sender, TextStringEventArgs e);

    //for moving the specified Actor
    public delegate void TextFloatEventHandler(object sender, TextfloatEventArgs e);

    //for making a general call to something, without parameters
    public delegate void TextEventHandler(object sender);
//for moving the specified Actor
    public delegate void TextXYBoolEventHandler(object sender, TextXYBoolEventArgs e);

    public class TMP_Animated : TextMeshProUGUI
    {
                [SerializeField] private float speed = 50;

        
        //Declares event, add listener to this to be triggered when the event is triggered
        //Call when character emote changes
        public event TextIntEventHandler emoteEvent;
        //Call when speaker should change
        public event TextStringEventHandler speakerEvent;
        //Call when character should move
        public event TextXYBoolEventHandler moveEvent;
        //Call when character should face a direction
        public event TextIntEventHandler faceEvent;
        //Call when character should enter
        public event TextIntEventHandler enterEvent;
        //Call when character should exit
        public event TextIntEventHandler exitEvent;
//when dialogue is first displayed
        public event TextEventHandler onDialogueStart;
//when dialogue is displaying
        public event TextEventHandler onTextReveal;

        //call when dialogue is finished displaying
        public event TextEventHandler onDialogueFinish;

        //bool to tell if we want to skip or not
        public bool skip=false;
        //bool to tell if the text asset is typing or not 
        public bool isTyping=false;
        //call to skip the text displaying process
        public void SkipText(){
            //Debug.Log("skipping");
            skip=true;
        }

        public void ReadText(string newText)
        {
            isTyping=true;
            text = string.Empty;
            // split the whole text into parts based off the <> tags 
            // even numbers in the array are text, odd numbers are tags
            string[] subTexts = newText.Split('<', '>');

            // textmeshpro still needs to parse its built-in tags, so we only include noncustom tags
            string displayText = "";
            for (int i = 0; i < subTexts.Length; i++)
            {
                if (i % 2 == 0)
                    displayText += subTexts[i];
                else if (!isCustomTag(subTexts[i].Replace(" ", "")))
                    displayText += $"<{subTexts[i]}>";
            }

            // check to see if a tag is our own
            //IMPORTANT NOTE: make sure the tag here is match the ones being parsed later, otherwise, the tags will appear in the dialogue text
            bool isCustomTag(string tag)
            {
                return tag.StartsWith("spd=") || tag.StartsWith("stop=") || tag.StartsWith("emote=") || tag.StartsWith("move") || tag.StartsWith("spk") || tag.StartsWith("face") || tag.StartsWith("enter") || tag.StartsWith("exit");
            }

            // send that string to textmeshpro and hide all of it, then start reading
            text = displayText;
            maxVisibleCharacters = 0;
            StartCoroutine(Read());

            IEnumerator Read()
            {
                //invokes when dialogue is being started to display
                onDialogueStart?.Invoke(this);
                //setting skip to false, just in case some chenanigans happen
                skip=false;

                int subCounter = 0;
                int visibleCounter = 0;
                while (subCounter < subTexts.Length)
                {
                    // if 
                    if (subCounter % 2 == 1)
                    {
                        yield return StartCoroutine("EvaluateTag",subTexts[subCounter].Replace(" ", ""));
                    }else{
                        while (visibleCounter < subTexts[subCounter].Length)
                        {
                            //relic from the old system, looks like it's invode whenever a new character is displayed
                            //you can use it, if you want, but I'm not, at the moment
                            onTextReveal?.Invoke(subTexts[subCounter][visibleCounter]);
                            visibleCounter++;
                            maxVisibleCharacters++;
                            if(!skip){
                               
                                yield return new WaitForSeconds(1f / speed);
                            }
                        }
                        visibleCounter = 0;
                    }
                    subCounter++;
                }
                yield return null;

                
                //text is done displaying, stop skipping
                skip=false;
                isTyping=false;
                //call when you want it to finish
                onDialogueFinish?.Invoke(this);
            }
        }

        public IEnumerator EvaluateTag(string tag)
                {
                    if (tag.Length > 0)
                    {
                        //IMPORTANT NOTE: make sure the tags being parse here match the ones being looked for in isCustomTag method from earlier
                        if (!skip&&tag.StartsWith("spd="))
                        {
                            speed = float.Parse(tag.Split('=')[1]);
                        }
                        //we add the skip condition, or the stop tag will stop despite wanting to skip
                        else if (tag.StartsWith("stop="))
                        {
                            yield return StartCoroutine("Test", float.Parse(tag.Split('=')[1]));
                        }
                        else if (tag.StartsWith("emote="))
                        {
                            //holding the array of strings to easily reference
                            string[] param=tag.Split('=')[1].Split(',');
                            // Raise the event in a thread-safe manner using the ?. operator.
                            emoteEvent?.Invoke(this, new TextIntEventArgs(param[0],Int32.Parse(param[1])));
                        }
                        else if (tag.StartsWith("move="))
                        {
                            //holding the array of strings to easily reference
                            string[] param=tag.Split('=')[1].Split(',');
                            //passes in the moving character's name and new position to move to
                            //if we want to skip, the we should snap all proceeding tweens
                            moveEvent?.Invoke(this, new TextXYBoolEventArgs(param[0],float.Parse(param[1], CultureInfo.InvariantCulture),float.Parse(param[2], CultureInfo.InvariantCulture),skip));
                        }
                        //this is to change the speaker, the first item is who should be speaking, and the second is the display name
                        else if (tag.StartsWith("spk="))
                        {
                            //holding the array of strings to easily reference
                            string[] param=tag.Split('=')[1].Split(',');

                            TextStringEventArgs args;
                            //if there is a display name (different from character name)
                            if(param.Length>1){
                                //get the displayname and pass it in
                                args=new TextStringEventArgs(param[0],param[1]);
                            }else{
                                //if no display name, pass an empty string
                                args=new TextStringEventArgs(param[0],"");    
                            }
                            // Raise the event in a thread-safe manner using the ?. operator.
                            speakerEvent?.Invoke(this, args);
                        }
                        else if (tag.StartsWith("face="))
                        {
                            //holding the array of strings to easily reference
                            string[] param=tag.Split('=')[1].Split(',');
                            // Raise the event in a thread-safe manner using the ?. operator.
                            faceEvent?.Invoke(this, new TextIntEventArgs(param[0],Int32.Parse(param[1])));
                        }
                        else if (tag.StartsWith("enter="))
                        {
                            //holding the array of strings to easily reference
                            string[] param=tag.Split('=')[1].Split(',');
                            // Raise the event in a thread-safe manner using the ?. operator.
                            enterEvent?.Invoke(this, new TextIntEventArgs(param[0],Int32.Parse(param[1])));
                        }
                        else if (tag.StartsWith("exit="))
                        {
                            //holding the array of strings to easily reference
                            string[] param=tag.Split('=')[1].Split(',');
                            // Raise the event in a thread-safe manner using the ?. operator.
                            exitEvent?.Invoke(this, new TextIntEventArgs(param[0],Int32.Parse(param[1])));
                        }
                    }
                    yield return null;
                }

        public IEnumerator Test(float s){
            float timePassed = 0f;
                            //float s=float.Parse(tag.Split('=')[1]);
            while(timePassed < s)                   // ride the bus for the given time
            {
                if (skip)                // break from coroutine when accident occures
                {
                    yield break;
                 }
                yield return new WaitForEndOfFrame();
                timePassed += Time.deltaTime;
            }
        }
    }
}
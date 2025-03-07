<enter=Lana,-1><face=Lana,1><enter=Signis,1><face=Signis,-1><move=Lana,-575,0.1><move=Signis,575,0.1><spk=Signis>Yo! Lana!<stop=0.7><emote=Signis,1> You think we have free will?

-> choice
== choice ==
+ [Yeah] -> Yes
+ [Nah] -> No
+ [Wha?] -> What

== Yes ==
<spk=Lana>Well, yes<stop=0.2><emote=Lana,3>, why ask?
-> END
== No ==
<spk=Lana><spd=1>...<spd=50><emote=Lana,3> No.<emote=Signis,2>
-> END
== What ==
<emote=Lana,2>I'm sorry,<stop=0.2> what?
<spk=Signis><emote=Signis,0><emote=Lana,0>Oh!<stop=0.7><emote=Signis,1> You think we have free will?
->choice
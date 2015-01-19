This file will outline how I want to develop this program and what it should do.

##Activation##

Instead of using a prefix (like `!!` or `<<`), you talk to the chat user directly in the following ways:

1. @reply the user

        @Chatbot, alive?
        I'm not dead yet!
2. Reply to any message said by the chatbot


## Commands ##

**help** - shows some help information about what the software is

    This is a chat bot for the SO Close Vote Reviewers chat room, developed by [gunr2171](http://stackoverflow.com/users/1043380/gunr2171). For more information see the [github page](https://github.com/gunr2171/SOCVR-Chatbot).

**alive** - command for checking if the chatbox is alive without too much other nonsence. One of the following phrases will be picked at random:

    I'm alive and kicking!
    Still here you guys!
    I'm not dead yet!

**status** - chatbox replies with info about how it's running

    SOCVR ChatBot version 1.x.x, running for 3 hours and 2 minutes.

**stats** - shows the stats on the close vote queue stats page. For example:

    10,101  
    need review
    
    1,508  
    reviews today

**current tag** - shows the top tag from the data export query

    The current tag is [tag:xml]

**next x tags** - shows the next [x] tags in the data exporter query. Should limit it to some reasonable number, like 10.

    The next 4 tags are [tag:xml], [tag:apple], [tag:java], and [tag:validation]

**start event** - a combination of the "stats" and "next x tags" commands (shows next 3).

**starting** - tells the bot that you are starting a new review session

**commands** - lists out all the commands avalible for people to use:

    **Commands For Use**
    
    **Everyone**
    (list names of commands)
    
    **Privilaged**
    (list names of commands)
    
    **Owner**
    power-down
    ...
    

## Recording ##

The chatbot will also record particular lines by registered memebers of the chat room

1. When they pass/fail an audit

        passed javascript audit
2. When they run out of a filter (chat bot can also announce that a certain percentage of people have finshed a tag)

        There are no items for you to review, matching the filter "[antivirus]"
        
3. When they run out of reviews or close votes for the day (chat bot can also announce when a certain number of people have completed the event)

        Thank you for reviewing 40 close votes today; come back in 3 hours to continue reviewing.
        You have no more close votes today; come back in 11 hours.

---

Other ideas to add in when I get time:

 - Add a register command, like "I'm starting" or similar. This will record the start date time for that user
 - When the user either runs out of close votes or review items, take note of that number and the date time of the message.
 - Add a "last session stats" command so people can see some simple stats on their last session
 - For the last, this would read like: for your session ending 4 minutes ago, you reviewed 40 items in 24 minutes, averaging 1 review every 30 seconds. Overall, your average session time is 26 minutes and 17 seconds.

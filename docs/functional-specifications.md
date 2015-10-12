# Chatbot for the SO Close Vote Reviews Chat Room Functional Specifications

This document describes the functionality of the chatbot that resides in the [SO Close Vote Reviewers chat room](http://chat.stackoverflow.com/rooms/41570/so-close-vote-reviewers).

## Background

Grab a chair and sit down. Let me tell you a tale of how things used to be.

Back when I (gunr2171) joined SOCVR, the main members were rene and TGMCians. That's just about it. Some people popped in from time to time, but for the most part the only people consistanly helping on the CV queue were those two. It took me some time to get used to the queue but once I was accimated I joined in.

The best way we had to keep track of what we were working on was manually. We would need to ping each other (usualy rene) to say when were done with a tag, or other such status updates.

Audits were a different story. We wrote "passed c# audit" not to keep track of it, but to just tell the other person that we were still here and doing our part. It was a "hey look, I'm still going strong."

All of the commands in version 1 are formatted to the way manual messages were made. It's not the cleanest, but it works.

## What does this program plan to solve?

Instead of 3 regulars we've got about 10+, depending on the day. I developed the bot primarily as a way of keeping track of what our chat room has accomplished so we know what to work on next.

So the question this bot is designed to solve is:

> Me and the other chat members are in the CV queue right now, what should we be working on to maximize the number of review items that get completed?

## History of v1

Version 1 was a really good stab. It got everything we really needed to have the bot do. Of course, there are a lot of things we wish it _could_ do. Version 2 is mostly improvement to the current system.

Areas of Version 1 that we have problems with:

* You needed to type chat commands for anything to happen.
* It only knew about thing you told it about.

## Goals for v2

What are the primary goals for this version of the software? The bullet point items should be no more than (around) 5, very broad in scope (because they will be explained in detail later), and give a "big picture" for the direction of the software

* Runs as a Linux Docker image - running the bot from Jenkins is not what Jenkins is designed for, so the software will be built to run as a service. Jenkins will still be used for testing the software and deploying it to test/production environments.

* Minimum interactions needed from chat to operate - bot will work in the background to gather information, without needing it from chat.

* Store review sessions by UTC day, not by individual sessions - in v1 a person can have multiple sessions per day, which doesn't make a lot of sense and can be confusing. In v2, the chatbot will just record all reviews done within a UTC day and do computations from there.

> Consideration, have a command that prints out a link and date of all recorded reviews for the user in the same UTC day.

## V2 Member Workflow

Here's how a chat member should expect to use the bot in v2:

1. a UTC day starts
2. the user does a single review
3. the bot pings the user to say "I see you have started reviewing. Good luck!"
4. the user may stop and start (human phrasing) multiple times, the bot will not care. This includes taking a hour between reviews.
5. if the user passes an audit it will be posted to the chat room.
6. as soon as the user completes 40 review items in a single UTC day, the bot will ping the user saying
  > You've completed 40 CV review items today, thanks! The time between your first and last review today was X minutes, averaging to a review every Y minutes.

7. the user then waits for the next UTC day to start.

## V2 Command List

Command for this version will focus on stats. Some commands will stick around incase manual intervention is needed.

### Everyone
Command that everyone can use

#### Alive

### Registered Users

#### Who
(all toy commands should only be usable by registered users)

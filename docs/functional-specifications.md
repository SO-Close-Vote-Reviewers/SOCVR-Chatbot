# Chatbot for the SO Close Vote Reviews Chat Room Functional Specifications

This document describes the functionality of the chatbot that resides in the [SO Close Vote Reviewers chat room](http://chat.stackoverflow.com/rooms/41570/so-close-vote-reviewers).

## Background

Grab a chair and sit down. Let me tell you a tale of how things used to be.

Back when I (gunr2171) joined SOCVR, the main members were rene and TGMCians. That's just about it. Some people popped in from time to time, but for the most part the only people consistently helping on the CV queue were those two. It took me some time to get used to the queue but once I was acclimated I joined in.

The best way we had to keep track of what we were working on was manually. We would need to ping each other (usualy rene) to say when were done with a tag, or other such status updates. With only 3 members, this wasn't a problem.

Audits were a different story. We wrote "passed c# audit" not to keep track of it, but to just tell the other person that we were still here and doing our part. It was a "hey look, I'm still going strong."

All of the commands in version 1 are formatted to the way manual messages were made. It's not the cleanest, but it works.

## What does this program plan to solve?

Instead of 3 regulars we've got about 10 or more, depending on the day. I developed the bot primarily as a way of keeping track of what our chat room has accomplished so we know what to work on next.

The question this bot is designed to solve is:

> Me and the other chat members are in the CV queue right now, what should we work on to maximize the number of review items that get completed?

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

General rule for commands:
* "Everyone" commands are primarily to test if the bot is running
* Only registered users can run toy commands
* Moderators have access to all commands

The following is a summary table of all commands. Extended details are in the next sections.

<!-- use https://ozh.github.io/ascii-tables/ to create this -->

| Permission Level | Command              | Description                                                                                                  |
|------------------|----------------------|---------------------------------------------------|
| Everyone         | Alive                | Tests if the bot is running and listening to chat                                                            |
| Everyone         | Commands             | Shows the list of commands to control the bot                                                                |
| Everyone         | Help                 | Prints information about the bot                                                                             |
| Everyone         | Running Commands     | Displays a list of commands that the bot is currently executing                                              |
| Everyone         | Status               | The bot will display how long it has been running for and what version is running                            |
| Toy              | Who                  | A toy command for blaming a chat room user.                                                                  |
| Toy              | Fox                  | A toy command for posting the meme fox gif                                                                   |
| Toy              | Panic                | A toy command for posting a gif of something panicing                                                        |
| Toy              | When                 | A toy command for getting a random date                                                                      |
| Reviewer         | Audit stats          | Shows the user how many of each tag they have passed audits for                                              |
| Reviewer         | Current Tag          | Get the tag that has the most amount of manageable close queue items from the SEDE query.                    |
| Reviewer         | Next [#] tags        | Displays the first X tags from the SEDE query to focus on.                                                   |
| Reviewer         | Refresh tags         | Forces a refresh of the tags obtained from the SEDE query.                                                   |
| Reviewer         | Queue stats          | Shows the stats at the top of the /review/close/stats page.                                                  |
| Reviewer         | Current Review Count | Shows the number of review items the bot thinks the user has completed this day                              |
| Bot Owner        | Start event          |                                                                                                              |
| Bot Owner        | Ping reviewers       | Sends a message which includes an @reply to all users in the reviewers group that have done reviews recently |
| Bot Owner        | Stop bot             | The bot will leave the chat room and quit the running application.                                           |

### Reviews Today

Format: `reviews today`

This command is used for showing what the bot knows of the user's completed review items. Here is an example table:

+---------+------------+--------+-------------------------+
| Item Id | Action     | Audit  | Completed At            |
+---------+------------+--------+-------------------------+
| 12345   | Closed     |        | 2015-01-02 03:23:11 UTC |
| 23456   | Edit       | Passed | 2015-01-02 03:23:11 UTC |
| 34567   | Leave Open | Failed | 2015-01-02 03:23:11 UTC |
+---------+------------+--------+-------------------------+

The `Item Id` is the number in the URL for that review item. If the review item is not an audit then the Audit cell will be blank. Order this table by `Completed At` ascending.

## Permission system

The new permission system will be made of 4 groups:

* Public
* Reviewers
* Toy Users
* Bot Owners

These groups are independent of each other. A person can be in multiple groups at once. Members of each group (besides Public) can add users to that group.

Restrictions on joining a group:
* In order to join the Reviewers group you must have at least 3000 reputation.
* In order to join the Bot Owners group you must be in the Reviewers and the Toy Users group

Restrictions on approving or rejecting a request:
* In order to approve or reject a request for the Reviewers or Toy Users group you must be in that group for at least 1 week.
* In order to approve or reject a request for the Reviewers group you must have done at least 100 reviews in the last 7 days, including the current UTC day.

### Permission Request

In version 1, only bot owners could add users to the track list. This was fine most of the time, but there are some drawbacks:
* If there are no bot owners present then the user has to come back later or make a ping.
* Even if the bot owner is around they might not see the request.

Either way, it's easy for a request to go unseen. This new permission system will allow normal members to handle requests (in the same spirit as community moderation on Stack Overflow). More eyes means quicker request handling.

Note: a user may only have one request per permission group active at a time (active means it is waiting to be approved or rejected).

#### Asking for permission

A user has 2 methods to request permission to a group.
1. Attempt to run a command they don't have access to, then respond "yes" to the prompt.
2. Run the `request permission for <group name>` command.

**First method**

If a user does not have the correct permissions to run a command the bot will respond with

> Sorry, you are not in the <name of group> permission group. Do you want to request access? (reply with "yes")

A permission request will be inserted into the request queue if either
* The user replies "yes" (or limited variations) to the bot's message
* The user's first message proceeding the bot's message, posted less than 1 minute after the bot's message, is "yes" (or limited variations)

If the user tries to run a command where they do not have permission to do so, and they have an active request for that permission, the bot will respond with

> Sorry, you are not in the <name of group> permission group. There is already a request to get you this permission, please be patent.

If the user tries to run a command where they do not have permission to do so, the latest request for that permission has been denied, and that denial was within the last 48 hours, then the bot will ignore the message. Once 48 hours has ellapsed, the bot will allow them to ask for permission again. Note that there should not be a need to increase the amount of time a reject will encure. There is a already a "kick" and "ban" system in chat which which room owners should use if it gets to this point.

**Second method**



#### Viewing Requests

Any user non-public user can run the `View requests` command to see the full list. This is an example output:

| Request # | Display Name | User Id | Requesting | Requested at            |
|-----------|--------------|---------|------------|-------------------------|
| 1         | gunr2171     | 12345   | Reviewer   | 2015-01-01 00:00:00 UTC |
| 2         | rene         | 23456   | Bot Owner  | 2015-01-01 00:00:00 UTC |
| 3         | sam          | 34567   | Toy User   | 2015-01-01 00:00:00 UTC |

Non-public non-bot-owners must run the command to see this request list. The bot will send a general message (not a reply to any user) if
 * A Bot Owner has posted 3 messages within 5 minutes
 * There is one or more request in the system
 * This message has not been displayed in the last 6 hours.

A group member has 2 method to approve or reject a request.
1. Run `approve request <#>` or `reject request <#>`.
2. The user manually adds the user to the group (this will approve the request at the same time).

## Docker

Docker is a "virtual machine" system for linux. We should use the following methods when using docker.
* Environment variables before config file - If a setting can be found as an environment variable, use it. If it can't, try and find it in the configuration file. If it still can't be found, use a built-in default.
* Database in its own container, which is linked.
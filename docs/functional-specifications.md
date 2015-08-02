# Chat bot for the SO Close Vote Reviews Chat Room

This document describes the functionality of the chatbot that will be reside in the SO Close Vote Reviewers chat room.

## Background
How did this all start?

What we we doing (in the chatroom in general) beforehand? What was the purpose of the chatroom?

How were we keeping track of data?

## What does this program plan to solve?

Highlight the troubles that the old system had, and talk (in broad terms) of the way it will be fixed or improved.

## History of v1

Talk about what v1 could do, and it's short comings.

## Goals for v2

What are the primary goals for this version of the software? Should be no more than ~5, be very broad, and gives a "big picture" for the direction of the software. More details on these will be explained later.

* Runs as a Linux service - running the bot from Jenkins is not what Jenkins is designed for, so the software will be built to run as a service. Jenkins will still be used for testing the software and deploying it to test / production environments.

* Minimum interactions needed from chat to operate - bot will work in the background to gather information, without needing it from chat.

* Store review sessions by UTC day, not by indivual sessions - in v1 a person can have multiple sessions per day, which doesn't make a lot of sense and can be confusing. In v2, the chatbot will just record all reviews done within a UTC day and do computations from there.

> Consideration, have a command that prints out the link and date of all recorded reviews for the user in the same UTC day.

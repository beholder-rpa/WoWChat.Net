WoWChat.Net -- README
=====================

WoWChat.Net is a library that facilitates creating a chat bot for old versions of World of Warcraft.

WoWChat.Net is a clean rewrite of the excellent scala-based [WoWChat](https://github.com/fjaros/wowchat) project in dotNet Core 6.x with the focus on making a library usable for creating chat bots that can utilize not only Discord, but other chat and messaging platforms.

**It does NOT support WoW Classic or Retail servers.**

Currently supported versions are:
  * Wrath of the Lich King (Tested with Ascension)

Features:
* Clientless (Does not need the WoW Client to be open or even installed to run - this means you can run a chatbot on something as small as a Raspberry PI or a $5/mo Digital Ocean droplet)
* Runs within a dotNet Core program, and therefore works on Windows, Mac, and Linux.

## How it works
At its core, WoWChat.Net interacts with WoW private servers utilizing its binary TCP Socket protocol. [Dotnetty](https://github.com/Azure/DotNetty) is utilized as a networking framework. It is designed to be a library that can be used in any .Net Core based application such as creating/publishing a bot for Discord, via Twilios API, an MQTT or AMPQ-based Message broker, or even a chat section on a website via Signalr/WebSockets.

The main WoWChat class implements an IObservable<WoWChatEvent> interface, allowing chat messages, and other events, to be consumed via any IObserver<WoWChatEvent> implementation.

The included CLI sample project contains a command line interface that can be used to run the bot.

##### DO NOT, under any circumstances, use this bot on an account with existing characters!
Even though this bot does not do anything malicious, some servers may not like a bot connecting, and GMs may ban the account!

*** This bot currently successfully connects to realm and game servers and publishes messages, however consider it to be a work in progress as other packet types are implemented***
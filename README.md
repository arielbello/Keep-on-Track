Keep on Track!
====================

This is a 2D runner game made to implement the GamEXP model.
It was made with Unity3d free version, using C# and some third party libraries (listed below).

Whats GamEXP?
--------------------
You might have this question. Basically, it's a way to generate statistics based on which you may balance your game mechanics.
This model relies on players to create new configurations to the game as well as rate them.
The data is processed and collected by a server. You can find an example code of such a server here: http://github.com/arielbello.

So, to implement the GamEXP you'll need:
- Players to test, tweak and evaluate
- A server to handle the data
- A game to experiment with

Basic Setup
--------------------
To quickly setup the GamEXP with this game and my server example, you need to first setup the server (can be done locally).
Then you should edit the Constants.cs, typing your server url and the game name you've set up for this experiment on the server.

Last but not least, take your time reading and understanding the code (it's not good, I guarantee it).
If you need some support, mail me: arielfbello@gmail.com


Credits
--------------------
First, I'd like to thank Matt Schoen for the great JSONObject library I use here. Link (Unity asset store): https://www.assetstore.unity3d.com/en/#!/content/710

Second, Thanks to JNA Mobile, who put together the really nice GUI I try to use in this game. Link (Unity asset store): https://www.assetstore.unity3d.com/en/#!/content/7987

As for my code, it's using the beer license as follows

	###THE BEER-WARE LICENSE (Revision 42):
	<arielfbello@gmail.com> wrote this file. As long as you retain this notice you
	can do whatever you want with this stuff. If we meet some day, and you think
	this stuff is worth it, you can buy me a beer in return
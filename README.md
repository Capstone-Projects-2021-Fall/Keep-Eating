
# Keep Eating! :joy:
Keep Eating is a mulitplayer game project for Temple CIS4398 capstone course. In this game, you can get to your heart's content, or you can ruin your opponent's good time by 
picking up the bombs and explode them to death :smiling_imp:. Just like Megumin!

## Revision History
| Revision | Author | Revision Date| Comments
|----------|------|----------|---------|
| 1.0 | Brendan Lisowski, Darnell Faison, Huifeng Liang, Ji Park, Ju-Hung Chen, Minyi Li, Omran Farighi | Sept. 2,2021 | Initiated |
|1.01|Darnell Faison|Sept. 3, 2021| Detail added for lobbies and player interaction. |
|1.02|Ju-Hung Chen|Sept. 9 2021| Add game discription outline |
|1.03|Brendan Lisowski, Ju-Hung Chen, Omran Farighi| Sept. 14, 2021| Update system block diagram, update user stories|
|1.04|Ji Park, Omran Farighi| Sept. 14, 2021| Update user stories|
|1.05| Omran Farighi | Nov. 15 2021 | Updated project overview and linked QA testing document | 
|1.06|Ji Park|Dec. 5 2021| Last minute Readme changes

## Overview
Keep Eating is an online multiplayer game, powered by the Unity engine and Photon Unity Networking (PUN) framework. Unity scripts are written in C# and are used to create the game logic. The game state, player manager, and handling of assets are all handled via C# scripts. PUN is a Unity package that allows for matchmaking by placing players into rooms. Within these rooms, the game is synced over the network allowing for a low latency multiplayer gameplay to be played on either MAC or PC. 

The matchmaking, which is handled by PUN, is done by creating lobbies. Either a player can join a lobby or create their own. Players within a lobby can choose which team they want to be a part of, Eaters or Enforcers.   The Eaters team objective is to win the match by reaching their score goal. They gain points by eating "food" that spawns all over the map. The Enforcers team objective is to prevent the Eaters from scoring points by shooting them with weapons. If an Eater's health is depleted by the attacks from the Enforcers, they respawn on the map with a new full bar of health. The Enforcers win by preventing the Eaters from reaching the winning score as time runs out. 


## QA Testing
[Use the following document for QA testing](https://github.com/Capstone-Projects-2021-Fall/Keep-Eating/blob/Updated-ReadMe-for-pre-release/Keep%20Eating%20Acceptance%20QA%20Testing%20doc.xlsx)

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

## Project Abstract
This document proposes an online video game in which players compete against each other in order to eat more food than their opponents. The game takes place inside an arena where food items spawn in from the sides. Players eat by colliding with the food, earning points. The player with the most points when a timer runs out is the winner. Players can create their own lobbies to play against each other over the internet.

## High Level Requirement
The project, by its completion, must be able to do the following:

-   Function as a complete game with all of its elements present.
    
-   Allow players to play against each other over the internet
    
-   Allow players to create their own lobbies
    
-   Allow for players to customize their avatars
    
-   Develop or implement assets that thematically fit the game
    
-   Have both Windows and Android versions

## Conceptual Design
This game will be built using the Unity game engine, which comes with its own set of scripts assets and multiplayer integration. Additionally, Unity games can be easily ported to both PC and mobile devices.

Players are able to create their own lobbies, with up to four players to play five-minute matches. At the start of each game, all players in the lobby will be sent to one of a many arenas. The second the game starts, food will periodically spawn into the arena and the players must compete to eat the food first.

Each food item has different points values to encourage interactivity. In addition to simply racing to eat more food, players can also attack each other by picking up weapons, such as hammers and bombs. Hitting a player with a melee weapon knocks them back; bombs inflict knockback and make the opponent lose points.Which player has the highest score wins the game. On top of that, players can unlock new styles for their characters.

## Background
This project can be described as *Agar.io* meets *Among Us*. This is a lobby-based game in which players must compete against each other by racing, fighting and outplaying each other to get points. Players only have five minutes to beat their opponents, leading to fast-paced gameplay. Different items have different points values, encouraging emergent gameplay opportunities. Additionally, the game will spawn bombs which take away points when touched. These bombs can be picked up and used as weapons by the players, adding to the game’s complexity. The game is wrapped in an 8-bit theme and players are able to customize their avatars to add personalization.

## Required Resources
In order to complete this project, all team members need:

-   A Unity Student Account
    
-   Access to the Unity Infrastructure
    
-   An understanding of C#
    
-   AWS Cloud Computing Service

## User Story 
### Alice has heard about Keep Eating from her friends and she wants to play with her friends:
- Alice has arrived at the main menu. She wants to start a game, so she clicks on the “Create Lobby” button
- Upon clicking on the button, a request to create the server is sent to Photon. The Photon infrastructure creates the server, connects Alice to that server, assigns her as the   master client, and places her avatar into a unique lobby room. Each room created by Photon has its own unique lobby code. From this point forward, anyone can join this lobby     using the join lobby button
- Once Alice has been placed into her lobby, she wants to have other people join. At any time, anyone who clicks on the Join Lobby button could join her lobby. But Alice was       really looking forward to only playing with her friends. So Alice, as the master client, clicks on a button to make the lobby private
- At this point, the only way to join is to enter a lobby code on the “Join Lobby” menu. Alice uses Discord to share the code to her friends and all of her friends are able to     join the lobby
- Alice selects the settings for her game once all of her friends join, including playing with bots on the BigGameMap, with a max of six players. When everyone is ready, Alice     presses a button to start the game
- All players load into the mtch and a game of Keep Eating begins
### Bob gets a text from Alice to play with her and her friends
- Bob wants to join Alice’s game of Keep Eating. He enters the lobby code he got from Discord on the main and hits the Join Lobby button
- The request to join the lobby is sent to Photon. Photon checks the number of players in the lobby and realizes that there are already six players, which is the limit Alice set   previously. The server prevents Bob from joining the lobby
- Alice realizes that Bob can’t join. While she could increase the max number of players, Alice decides instead to stop playing for the day and quits
- The server updates the number of players in the lobby and randomly assigns a new master (Carol)
- Bob tries to join the lobby again and this time he succeeds.
- Carol finally starts a new match




## System Block Diagram
![Systam Block Diagram](/images/systemblockdiagram.png)


------------------------------------------------------------------------------------------------------------------------------------------------------------------------------



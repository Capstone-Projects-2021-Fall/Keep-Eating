
# Keep Eating! :joy:

## Revision History
| Revision | Author | Revision Date| Comments
|----------|------|----------|---------|
| 1.0 | Brendan Lisowski, Darnell Faison, Huifeng Liang, Ji Park, Ju-Hung Chen, Minyi Li, Omran Farighi | Sept. 2,2021 | Initiated |
|1.01|Darnell Faison|Sept. 3, 2021| Detail added for lobbies and player interaction. |

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
### Unlucky Ji Park: 
Ji was living in the greater Philadelphia area when the region was hit by a strong storm knocking out the power for his neighborhood. With no power, all he has left is his phone that he charges in his car every few hours. Bored with nothing to do while he waits for his power to come back on, Ji keeps himself entertained by playing Keep Eating to help pass the time.

### Darnell and Friends: 
Darnell is having a birthday party with three of his closest friends. To make sure everyone is having fun at the party, Darnell has his friends download Keep Eating. With everyone on the app, Darnell and his friends have endless fun playing the game and staying entertained throughout the party.

### Brendan: 
Brendan has been playing Keep Eating for a month now and has been able to unlock several skins and accessories for his avatar. A friend of his just started playing the game but did not have any unlockables due to the lack of experience and achievement points. Brendan and his friend work together to help his friend gain experience and achievements so he can make his avatar unique and personal.

## System Block Diagram


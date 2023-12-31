# Details: Beamable Chat Vanilla Test

This sample project demonstrates specific [Beamable](https://beamable.com/) features and services including [Beamable Chat](https://docs.beamable.com/docs/chat-feature).

### Variations Of Examples

There are several sample projects related to Chat.
1. [Chat_GPW_Sample_Project](https://github.com/beamable/Chat_GPW_Sample_Project) - This is a great place to start. It focuses on vanilla chat.
1. [Chat_GPW_2_With_MicroStorage_Sample_Project](https://github.com/beamable/Chat_GPW_2_With_MicroStorage_Sample_Project) - This is just like #1 above, plus it includes Beamable Microservices and Beamable MicroStorage.


-----

**Project Configuration**
* `Unity Target` - Standalone MAC/PC
* `Unity Version` - Use this [Version](./client/ProjectSettings/ProjectVersion.txt) or above

**Project Structure**
* `README.md` - This README file
* `client/` - Open this folder in the Unity Editor
* `client/Assets/` - Core files of the project
* `client/Assets/Scenes/` - **Open a scene** in the Unity Editor to play the game!
* `client/Assets/3rdParty/` - Dependency asset files for the project
* `client/Packages/` - Dependency package files for the project

**Beamable SDK**
* **Included**: This project includes the Beamable SDK for Unity
* **Version**: The latest public release as of each GIT commit

# Overview: Chat Vanilla Test

## What is "Chat Vanilla Test"?
This project is a demonstration representing the core functions of the chat service. It shows the possibility of adding more than one local player.
The project includes: 
  - Two Players
  - Your Rooms Selection
  - Create/Join/Leave Chat Rooms

>Important Note:
The project is configured for more than one local players which is using the BeamContext.ForPlayer(string playerCode).Instance method instead of the
Default Context.
If you want to use the Default context please remove the symbol definition in Edit/Project Settings/Player/Other Settings/Script Compilation and
remove the BEAMABLE_ENABLE_BEAM_CONTEXT_DEFAULT_OVERRIDE.

## What is Beamable?
Beamable is the low-code option for rapidly adding social, 
commerce, and content management features to your live game. 
Learn how to do that with Beamable's online product documentation.
<br>[docs.beamable.com](https://docs.beamable.com/)

## What is Beamable's "Chat" Feature?
The purpose of this feature is to allow players to communicate in-game.
<br>[Beamable Chat](https://docs.beamable.com/docs/chat-feature)

## Got feedback?
Let us know what you think or ask any questions you might have.
<br>[Contact Us](https://docs.beamable.com/discuss)

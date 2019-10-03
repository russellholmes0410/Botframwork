# EchoBot

Bot Framework v4 echo bot sample for Teams.

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to create a simple bot that accepts input from the user and echoes it back.

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 2.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- Microsoft Teams is installed and you have an account

## To try this sample

### Ngrok
- Download and install [ngrok](https://ngrok.com/download)
- In terminal navigate to where ngrok is installed and run: ```ngrok http -host-header=rewrite 3978```
- Copy/paste the https web address into notepad as you will need it later

### Microsoft Teams
- Launch Microsoft Teams. In the search bar at the top of Teams search for and select ```App Studio```.
- Click the ```Manifest editor``` tab near the top of the screen.
- Click the ```Create a new app``` button on the left hand side.
- Under the ```Details``` section fill in the following fields 
  - In the Short name field enter ```EchoBot```
  - Click the ```Generate``` button under App ID 
  - Package Name
  - Version 
  - Short description
  - Long description
  - Developer name
  - Website 
  - Privacy statement web address
  - Terms of use web address
- Under the ```Capabilities``` tab on the left hand side click the ```Bots``` tab
- Click the ```Set up``` button
- Under the ```New bot``` tab Fill in the following fields
  - Put ```EchoBot``` into the Name field
  - Under ```Scope``` check all 3 boxes ```Personal```, ```Team```, ```Group Chat```
  - Click the ```Create bot``` button
- Copy the Bot ID (string under ```EchoBot```) and paste it into notepad as you will need it later
- Click the ```Generate new password``` button (copy/paste) the password into notepad as you will need it later)
- Under Messaging endpoint paste the https ngrok url and add ```/api/messages``` to the end
  - EX: ```https://ca7f8a7e.ngrok.io/api/messages```
- Press Enter to save the address

### Bot Setup
- Clone the repository

    ```bash
    git clone https://github.com/Microsoft/botbuilder-samples.git
    ```

- In Visual Studio navigate to the ```52.teams-echo-bot``` folder and open the ```appsettings.json``` file
- Put the  you saved earlier from Teams in the ```MicrosoftAppId``` field
- Put the password into the ```MicrosoftAppPassword``` field
- Save

- Run the bot from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/csharp_dotnetcore/52.teams-echo-bot` folder
  - Select `EchoBot.csproj` file
  - Press `F5` to run the project

### Back to Teams
- Back in Teams click ```Test and distribute``` on the left hand side under ```Finish``` section
- Click the ```Install``` button
- Click the ```Add``` button
- In the compose message area at the bottom of the screen send the message ```hi``` 
- You should receive a response from the bot

### Installing the bot in a Team
- Search for App Studio
- Go to the Manifest editor 
- Click the ```EchoBot``` card
- Click ```Test and distribute```
- Click ```Install```
- Click the down arrow to the right of the ```Add``` button 
- Click ```Add to Team```
- Search for and select your team
- Click the ```Set up a bot``` button
- **Note:** You need to type: ```@EchoBot [message]``` to message your bot
 - Send your bot one of the following strings: ```show members```, ```show channels```, or ```show details``` to get the members of your team, the channels in your team, or metadata about your team respectively. 
  - **EX:** ```@EchoBot show members```

### Installing the bot in a group chat
- Search for App Studio
- Go to the Manifest editor 
- Click the ```EchoBot``` card
- Click ```Test and distribute```
- Click ```Install```
- Click the down arrow to the right of the ```Add``` button 
- Click ```Add to Chat```
- Search for and select your group chat
- **Note:** The group chat has to have at least 1 message for the group chat to be searchable
- Click the ```Set up a bot``` button
- **Note:** You need to type: ```@EchoBot [message]``` to have the bot receive your message
 - Send your bot one of the following strings: ```show members``` to get the members in the group chat. 
  - **EX:** ```@EchoBot show members```

### Installing the bot in a personal chat
- Search for App Studio
- Go to the Manifest editor 
- Click the ```EchoBot``` card
- Click ```Test and distribute```
- Click ```Install```
- Click the down arrow to the right of the ```Add``` button 
- Click ```Add```
- Search for and select your team
- Click the ```Set up a bot``` button
- **Note:** You need to type: ```@EchoBot [message]``` to message your bot
 - Send your bot one of the following strings: ```show members``` to get the members in the group chat. 
  - **EX:** ```@EchoBot show members```

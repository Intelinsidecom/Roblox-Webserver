# Roblox Webserver

## Have Questions? Need Help? Join the official Freebloxia Server: 
https://discord.gg/9ypw3ytK4t

Roblox Webserver is a project designed to work nicely with all Roblox Clients (Hosting, Joining and more) and to be as close to how roblox looked during 2016/2017 Era, be Documented on All Apis, how everything works.

## This Repo is consisted of:
1. Completed And Incomplete Core Logic
2. Additional Logic
3. Supported clients
4. How to Set it up for Myself

## Completed And Incomplete Core Logic:

- [X] Users, Database Stuff, Signup/Login.
- [X] RCC Arbiter that can send lua scripts to RCC binary (JSON support not added).
- [X] Assets.
- [X] Game Server Hosting using Arbiter.
- [X] Game Server Joining with Clients.
- [ ] Base Support for All Clients (Android, Windows, IOS, MacOS, Xbox, UWP and maybe custom 2016 Ports)

## Additional Logic:

- [ ] Additional Features, Apis like DataStore or Badges and so on.
- [ ] Friends, Economy (For buying items, gears or doing purchases in games).
- [ ] All Pages (Games, Home, Develop, Avatar Customizer, Catalog and so on).
- [ ] Make Pages work on Mobiles, Okd Webviews and be optimised.
- [ ] All Adittional Client Apis (Android, IOS, UWP, Xbox (idk about Xbox).
- [ ] Studio Support, Editing, Uploading, Team Create and So On.

## Supported Clients:
- Windows
- UWP

Documentation will be made in github Wiki Form.

# How to Set it up for myself:
1. Download NET. SDK 8.1 on Windows
2. Download Latest PostgresSQL on Windows
3. Change the AppSettings.json files to match your needs.
4. In each project, open cmd window and run "dotnet run" (What each project does is in wiki)
5. Compile Control Panel project (Windows Only) and from database section, update database with schemas. (Migrate button)
6. Check manually if everything works by setting up fully using the documentation from wiki.
7. Profit

Note: If you dont have an domain but want to test it out, change the hosts file to redirect freblx.xyz or whatever url you want like and then make change client urls accordingly and also set the localhost mode to true in website project.

# Project is not affiliated with Roblox in any way
Roblox assets from Pre 2021 era are only used for the core aesthetic of the project, project is mean for self hosting. in production its recommended to remake or change the JS, CSS, image assets to avoid any legal action from Roblox Corporation.

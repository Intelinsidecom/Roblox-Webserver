# Roblox Webserver

## Have Questions? Need Help? Join the official Freebloxia Server: 
https://discord.gg/8gWQtsUE9

Webserver designed to work nicely with all Roblox Clients (Hosting, Joining and more) and to be as close to how roblox looked during 2016/2017 Era, be Documented on All Apis, how everything works.

## This Repo is consisted of:
1. Completed And Incomplete Core Logic
2. Additional Logic
3. Supported clients
4. How to Set it up for Myself

## Completed And Incomplete Core Logic:

- [X] Users, Database Stuff, Signup/Login.
- [X] RCC Arbiter that can send lua scripts to RCC binary (JSON support not added).
- [ ] Assets (Items and so on).
- [ ] Game Server Hosting using Arbiter.
- [ ] Game Server Joining with Clients.
- [ ] Base Support for All Clients (Android, Windows, IOS, MacOS, Xbox, UWP and maybe custom 2016 Ports)

## Additional Logic:

- [ ] Additional Features, Apis like DataStore or Badges and so on.
- [ ] Friends, Economy (For buying items, gears or doing purchases in games).
- [ ] All Pages (Games, Home, Develop, Avatar Customizer, Catalog and so on).
- [ ] Make Pages work on Mobiles, Okd Webviews and be optimised.
- [ ] All Adittional Client Apis (Android, IOS, UWP, Xbox (idk bout Xbox).
- [ ] Studio Support, Editing, Uploading, Team Create and So On.

## Supported Clients:
 None.

Documentation will be made in gitub Wiki Form.

P.S. UWP 2016 gets to login, is able to login, sign out and loads some Apis for Info Like Balance, User Info, device/initialize not fixed as it still isnt analyzed as to what it expects for response.

# How to Set it up for myself:
1. Download NET. SDK 8.1 on Windows
2. Change the AppSettings.json files to match your needs, expectations on hardware.
3. Dotnet run in cmd in each directory where the project is
4. Profit

Note: If you dont have an domain but want to test it out, change the hosts file to redirect freblx.xyz or whatever url you want like roblox.com to localhost so the clients think its legit, on external Devices, umm, no idea rn.

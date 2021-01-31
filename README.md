# Proudly brought to you by @Kinue72 and @itsmehoaq
## Project VCLBot (aka MoguMogu Bot) is purpose-made for Vietnam Community League only.

Bot is open-source, which means you are on your own if you want to use this for your tournament.

- Credits: [Skybot](https://github.com/Blade12629/Skybot/) - Irc

### Basic Bot Usage
0. Prefix: default prefix is `*`

1. User commands:
   - `*verify`: You know this.
   - `*upcoming`: View upcoming match.
   
2. Referee commands: 
   - `*csay <channel_id> <message>`
   - `*postresult <channel_id> <match_id>`: ~~In case auto result posting is bugged.~~ Working now :D
   
3. Host commands (Or users with Administrator permission):
   - `*prefix`: View current prefix
   - `*config`: Including these parameters: `Enable Tournament Mode`, `Referee Role ID`, `Host Role ID`, `Spreadsheet ID`, `Verified Role Name`, `Auto Result Posting`, `Result Channel ID`, `Auto Reminder`, `Reminder Channel ID`, `Time Offset (UTC)`
     - `*config prefix <new_prefix>`
     - `*config enable_tour <true/false>`: Allowing commands below to work.
     - `*config ref_role_id <role_id>`
     - `*config host_role_id <role_id>`
     - `*config sheets_id <spreadsheet_id>`: Link to Referee/Staff Sheet. Keep in mind there is a template for this, which will be public when we are done with the stuff.
     - `*config verify_role_name <role_name>` _why role name you may ask?_ _(kinue: idk bro)_
     - `*config auto_result <true/false>`: Match result will be automatically posted when a match is finish & its ref sheet contains enough data.
     - `*config result_channel_id <channel_id>`
     - `*config auto_reminder <true/false>`: Bot will mention ref and players 30 minutes before the match.
     - `*config reminder_channel_id <channel_id>`
     - `*config time_offset <offset_value>`: UTC time offset (_E.g: UTC+7 -> time_offset = 7_)
     
4. Dev commands (_Only the creators are allowed to use these commands_):
   - `*changeactivity <new_activity>`
   - `*changeusername <new_username>` (Limited to ~~4 times per day~~ 2 times per hour)
   - `*changeavatar <link to image>`
   - `*merge`: Merge existing verified users

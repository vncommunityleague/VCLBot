# Proudly brought to you by @Kinue72 and @itsmehoaq
## Project VCLBot (aka MoguMogu Bot) is purpose-made for Vietnam Community League only.

Bot is open-source, which means you are on your own if you want to use this for your tournament.

- Credits: [Skybot](https://github.com/Blade12629/Skybot/)

### Basic Bot Usage
0. Prefix: default prefix is `*`
1. User commands:
   - `*verify`: You know this.
2. Referee commands: 
   - `*postresult <channel_id> <match_id>`: In case auto result posting is bugged.
3. Host commands (Or users with Administrator permission):
   - `*prefix <new_prefix>`
   - `*config`: Including these parameters: `Enable Tournament Mode`, `Referee Role ID`, `Host Role ID`, `Spreadsheet ID`, `Verified Role Name`, `Auto Result Posting`, `Result Channel ID`, `Auto Reminder`, `Reminder Channel ID`, `Time Offset (UTC)`
     - `*config enable_tour <true/false>`
     - `*config ref_role_id <role_id>`
     - `*config host_role_id <role_id>`
     - `*config sheets_id <spreadsheet_id>`
     - `*config verify_role_name <role_name>`
     - `*config auto_result <true/false>`
     - `*config result_channel_id <channel_id>`
     - `*config auto_reminder <true/false>`
     - `*config reminder_channel_id <channel_id>`
     - `*config time_offset <offset_value>`
4. Dev commands (_Only the creators are allowed to use these commands_):
   - `*changeactivity <new_activity>`
   - `*changeusername <new_username>` (Limited to 4 times per day)
   - `*changeavatar <link to image>`

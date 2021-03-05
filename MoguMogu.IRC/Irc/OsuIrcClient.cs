using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace MoguMogu.IRC.Irc
{
    //https://github.com/Blade12629/Skybot/tree/master/SkyBot.Networking
    public class OsuIrcClient : IDisposable
    {
        private readonly IrcClient _irc;
        private string _lastNick;
        private string _lastPass;
        
        private Timer _pingTimer;
        private DateTime _lastPong = DateTime.Now;


        public OsuIrcClient(string host = "irc.ppy.sh", int port = 6667)
        {
            _irc = new IrcClient(host, port, "Mogu Mogu", "Okayuuu", false);
            _irc.OnMessageRecieved += OnRawIrcMessageReceived;
        }

        public bool IsDisposed { get; private set; }
        public string CurrentUser { get; private set; }
        public bool IsConnected => _irc?.IsConnected ?? false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event EventHandler<IrcJoinEventArgs> OnUserJoined;
        public event EventHandler<IrcQuitEventArgs> OnUserQuit;
        public event EventHandler<IrcModeEventArgs> OnUserMode;
        public event EventHandler<IrcPartEventArgs> OnUserParted;
        public event EventHandler<IrcPrivateMessageEventArgs> OnPrivateMessageReceived;
        public event EventHandler<IrcPrivateMessageEventArgs> OnPrivateBanchoMessageReceived;
        public event EventHandler<IrcChannelMessageEventArgs> OnChannelMessageReceived;
        public event EventHandler<IrcWelcomeMessageEventArgs> OnWelcomeMessageReceived;
        public event EventHandler<IrcChannelTopicEventArgs> OnChannelTopicReceived;
        public event EventHandler<IrcMotdEventArgs> OnMotdReceived;
        public event EventHandler OnBeforeReconnect;
        public event EventHandler OnAfterReconnect;
        public event EventHandler<IrcUserNotFoundEventArgs> OnUserNotFound;

        ~OsuIrcClient()
        {
            Dispose(false);
        }
        private void Reconnect()
        {
            OnBeforeReconnect?.Invoke(this, new EventArgs());

            while (!ReconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                Task.Delay(500).ConfigureAwait(false).GetAwaiter().GetResult();

            while (!IsConnected)
                Task.Delay(250).ConfigureAwait(false).GetAwaiter().GetResult();
            
            OnWelcomeMessageReceived += OnReconnected;

            try
            {
                LoginAsync(_lastNick, _lastPass).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                OnWelcomeMessageReceived -= OnReconnected;
                throw;
            }

            void OnReconnected(object sender, EventArgs e)
            {
                try
                {
                    OnAfterReconnect?.Invoke(this, new EventArgs());
                }
                catch (Exception)
                {
                    OnWelcomeMessageReceived -= OnReconnected;
                    throw;
                }

                OnWelcomeMessageReceived -= OnReconnected;
            }
        }


        public async Task ConnectAsync() {
            await Task.Run(() => _irc.Connect()).ConfigureAwait(false);
            _irc.StartReadingAsync();
            if (_pingTimer == null)
            {
                _pingTimer = new Timer(TimeSpan.FromMinutes(2).TotalMilliseconds)
                {
                    AutoReset = true
                };
                _pingTimer.Elapsed += (sender, args) =>
                {
                    if (IsConnected)
                    {
                        SendCommandAsync("PING", "cho.ppy.sh irc.ppy.sh").ConfigureAwait(false);
                        var diff = Math.Floor((DateTime.Now - _lastPong).TotalMinutes);
                        if (diff > 5)
                        {
                            //reconnect
                            Reconnect();
                            Console.WriteLine($"Last pong: {diff}m ago, reconnecting.....");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Disconnected from server, reconnecting....");
                        ConnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        LoginAsync(_lastNick, _lastPass).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                };
                _pingTimer.Start();
            }
        }

        public async Task DisconnectAsync(bool stopTimer = true)
        {
            await Task.Run(() =>
            {
                _irc.StopReading();
                _irc.Disconnect();
            }).ConfigureAwait(false);
        }

        public async Task<bool> ReconnectAsync()
        {
            try
            {
                await DisconnectAsync(false).ConfigureAwait(false);
                Task.Delay(500).Wait();
                await ConnectAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task LoginAsync(string nick, string pass)
        {
            _lastNick = nick;
            _lastPass = pass;

            await SendCommandAsync("PASS", pass).ConfigureAwait(false);
            await SendCommandAsync("NICK", nick).ConfigureAwait(false);
        }

        public async Task SendCommandAsync(string command, string parameters)
        {
            await WriteAsync($"{command} {parameters}").ConfigureAwait(false);
        }

        public async Task JoinChannelAsync(string channel)
        {
            if (channel[0] != '#')
                channel = "#" + channel;

            await SendCommandAsync("JOIN", channel);
        }

        public async Task PartChannelAsync(string channel)
        {
            if (channel[0] != '#')
                channel = "#" + channel;

            await SendCommandAsync("PART", channel);
        }

        public async Task SendMessageAsync(string destination, string message)
        {
            await SendCommandAsync("PRIVMSG", $"{destination} {message}").ConfigureAwait(false);
        }

        protected virtual async Task WriteAsync(string message)
        {
            await _irc.WriteAsync(message).ConfigureAwait(false);
        }

        private void OnPong()
        {
            _lastPong = DateTime.Now;
        }

        private void OnRawIrcMessageReceived(object sender, string e)
        {
            var msgSplit = e.Split(' ').ToList();

            switch (msgSplit[0].ToLower(CultureInfo.CurrentCulture))
            {
                case "ping":
                    OnPing(msgSplit[1]);
                    return;
            }

            switch (msgSplit[1].ToLower(CultureInfo.CurrentCulture))
            {
                case "join":
                    OnJoinMessage(msgSplit);
                    return;

                case "quit":
                    OnQuitMessage(msgSplit);
                    return;

                case "mode":
                    OnModeMessage(msgSplit, e);
                    return;

                case "privmsg":
                    OnMessage(msgSplit, e);
                    return;

                case "353": //User List (names)
                    OnUserListReceived(msgSplit);
                    return;

                case "366": //User List End
                    OnUserListEndReceived();
                    return;

                case "part":
                    OnUserPart(msgSplit);
                    return;
                case "401":
                    OnNotFound(msgSplit[2]);
                    break;

                case "001": //Welcome Message
                    OnWelcomeMessage(e.Remove(0, msgSplit[0].Length + msgSplit[1].Length + msgSplit[2].Length + 3)
                        .TrimStart(':'));
                    return;

                case "332": //Channel topic
                    OnChannelTopic(e.Remove(0, msgSplit[0].Length + msgSplit[1].Length + msgSplit[2].Length + 3)
                        .TrimStart(':'));
                    return;

                case "333": //???

                    return;

                case "375": //Motd begin

                    return;

                case "372": //Motd
                    OnMotd(e.Remove(0, msgSplit[0].Length + msgSplit[1].Length + msgSplit[2].Length + 3)
                        .TrimStart(':'));
                    return;

                case "376": //Motd end

                    return;

                case "pong":
                    OnPong();
                    return;
                case "301": //user busy
                    return;

                case "464": //Bad authentication token (ERR_PASSWDMISMATCH) (should be the same for osu?)
                    var reInitTimer = false;
                    if (_pingTimer?.Enabled ?? !true)
                    {
                        _pingTimer.Stop();
                        reInitTimer = true;
                    }

                    Reconnect();

                    if (reInitTimer)
                        _pingTimer.Start();
                    Console.WriteLine("Invalid User/Password (IRC)");
                    return;
            }

            Console.WriteLine("Unkown command: " + e);
        }

        private void OnJoinMessage(IReadOnlyList<string> msgSplit)
        {
            var userAndServer = ExtractUserAndServer(msgSplit[0]);
            var parameter = msgSplit[2].TrimStart(':');

            OnUserJoined?.Invoke(this, new IrcJoinEventArgs(userAndServer.Item1, userAndServer.Item2, parameter));
        }

        private void OnQuitMessage(IReadOnlyList<string> msgSplit)
        {
            var userAndServer = ExtractUserAndServer(msgSplit[0]);
            var parameter = msgSplit[2].TrimStart(':');

            string channel = null;

            if (IsChannel(parameter))
                channel = parameter;

            OnUserQuit?.Invoke(this, new IrcQuitEventArgs(userAndServer.Item1, userAndServer.Item2, channel));
        }

        private void OnModeMessage(IReadOnlyList<string> msgSplit, string line)
        {
            var userAndServer = ExtractUserAndServer(msgSplit[0]);
            var parameters = line.Remove(0, msgSplit[0].Length + msgSplit[1].Length + 2);

            OnUserMode?.Invoke(this, new IrcModeEventArgs(userAndServer.Item1, userAndServer.Item2, parameters));
        }

        private void OnMessage(IReadOnlyList<string> msgSplit, string line)
        {
            var (sender, server) = ExtractUserAndServer(msgSplit[0]);

            var isChannel = IsChannel(msgSplit[2]);
            var msg = line.Remove(0, msgSplit[0].Length + msgSplit[1].Length + msgSplit[2].Length + 3)
                .TrimStart(':');

            if (isChannel)
                OnChannelMessage(sender, server, msgSplit[2], msg);
            else
                OnPrivateMessage(sender, server, msgSplit[2], msg);
        }

        private void OnPrivateMessage(string sender, string server, string destUser, string message)
        {
            if (sender.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase))
                OnPrivateBanchoMessage(new IrcPrivateMessageEventArgs(sender, server, destUser, message));
            else
                OnPrivateMessageReceived?.Invoke(this,
                    new IrcPrivateMessageEventArgs(sender, server, destUser, message));
        }

        private void OnChannelMessage(string sender, string server, string destChannel, string message)
        {
            OnChannelMessageReceived?.Invoke(this,
                new IrcChannelMessageEventArgs(sender, server, destChannel, message));
        }

        private void OnUserListReceived(List<string> msgSplit)
        {
            //for (int i = 0; i < 6; i++)
            //    msgSplit.RemoveAt(i);

            //msgSplit.RemoveAt(msgSplit.Count - 1);
        }

        private void OnUserListEndReceived()
        {
        }

        private void OnUserPart(List<string> msgSplit)
        {
            var (sender, server) = ExtractUserAndServer(msgSplit[0]);
            var channel = msgSplit[2].TrimStart(':');

            OnUserParted?.Invoke(this, new IrcPartEventArgs(sender, server, channel));
        }

        private void OnPrivateBanchoMessage(IrcPrivateMessageEventArgs message)
        {
            OnPrivateBanchoMessageReceived?.Invoke(this, message);
        }

        private bool IsChannel(string channel)
        {
            return channel[0] == '#';
        }

        private (string, string) ExtractUserAndServer(string msg)
        {
            msg = msg.TrimStart(':').TrimEnd(' ');

            var split = msg.Split('!');

            return (split[0], split[1]);
        }

        private void ExtractUserAndServer(string msg, out string sender, out string server)
        {
            var (item1, item2) = ExtractUserAndServer(msg);
            sender = item1;
            server = item2;
        }

        private void OnWelcomeMessage(string msg)
        {
            OnWelcomeMessageReceived?.Invoke(this, new IrcWelcomeMessageEventArgs(msg));
        }
        private void OnNotFound(string user)
        {
            OnUserNotFound?.Invoke(this, new IrcUserNotFoundEventArgs(user));
        }

        private void OnChannelTopic(string msg)
        {
            OnChannelTopicReceived?.Invoke(this, new IrcChannelTopicEventArgs(msg));
        }

        private void OnMotd(string msg)
        {
            OnMotdReceived?.Invoke(this, new IrcMotdEventArgs(msg));
        }

        private void OnPing(string str)
        {
            SendCommandAsync("PONG", str).ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (disposing)
            {
                _irc?.Dispose();
                _pingTimer?.Dispose();
            }
        }
    }
}
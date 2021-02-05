using System;

namespace MoguMogu.IRC.Irc
{
    public class IrcWelcomeMessageEventArgs : EventArgs
    {
        public IrcWelcomeMessageEventArgs(string welcomeMessage)
        {
            WelcomeMessage = welcomeMessage;
        }

        public string WelcomeMessage { get; }
    }
    public class IrcUserNotFoundEventArgs : EventArgs
    {
        public IrcUserNotFoundEventArgs(string user)
        {
            User = user;
        }

        public string User { get; }
    }

    public class IrcChannelTopicEventArgs : EventArgs
    {
        public IrcChannelTopicEventArgs(string topic)
        {
            Topic = topic;
        }

        public string Topic { get; }
    }

    public class IrcMotdEventArgs : EventArgs
    {
        public IrcMotdEventArgs(string motd)
        {
            Motd = motd;
        }

        public string Motd { get; }
    }

    public class IrcPartEventArgs : IrcQuitEventArgs
    {
        public IrcPartEventArgs(string sender, string server, string channel) : base(sender, server, channel)
        {
        }
    }

    public class IrcPrivateMessageEventArgs : EventArgs
    {
        public IrcPrivateMessageEventArgs(string sender, string server, string destination, string message)
        {
            Sender = sender;
            Server = server;
            Destination = destination;
            Message = message;
        }

        public string Sender { get; }
        public string Server { get; }
        public string Destination { get; }
        public string Message { get; }
    }

    public class IrcChannelMessageEventArgs : IrcPrivateMessageEventArgs
    {
        public IrcChannelMessageEventArgs(string sender, string server, string destination, string message) : base(
            sender, server, destination, message)
        {
        }
    }

    public class IrcModeEventArgs : EventArgs
    {
        public IrcModeEventArgs(string sender, string server, string parameters)
        {
            Sender = sender;
            Server = server;
            Parameters = parameters;
        }

        public string Sender { get; }
        public string Server { get; }
        public string Parameters { get; }
    }

    public class IrcJoinEventArgs : EventArgs
    {
        public IrcJoinEventArgs(string sender, string server, string channel)
        {
            Sender = sender;
            Server = server;
            Channel = channel;
        }

        public string Sender { get; }
        public string Server { get; }
        public string Channel { get; }
    }

    public class IrcQuitEventArgs : IrcJoinEventArgs
    {
        public IrcQuitEventArgs(string sender, string server, string channel) : base(sender, server, channel)
        {
        }
    }
}
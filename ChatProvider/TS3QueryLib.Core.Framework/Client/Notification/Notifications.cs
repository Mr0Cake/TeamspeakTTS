using System;
using System.Collections.Generic;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Client.Notification.EventArgs;
using TS3QueryLib.Core.Common.Notification;

namespace TS3QueryLib.Core.Client.Notification
{
    /// <summary>
    /// This class handles the notifications sent by the teamspeak-client and raises type safe events for each notification
    /// </summary>
    public class Notifications : NotificationsBase
    {
        #region Events

        public event EventHandler<TalkStatusEventArgs> TalkStatusChanged;
        public event EventHandler<TalkStatusEventArgsBase> ChannelTalkStatusChanged;
        public event EventHandler<TalkStatusEventArgsBase> WisperTalkStatusChanged;
        public event EventHandler<TS3QueryLib.Core.Server.Notification.EventArgs.ClientMovedEventArgs> ClientMoved;
        public event EventHandler<TS3QueryLib.Core.Server.Notification.EventArgs.MessageReceivedEventArgs> MessageReceived;

        #endregion

        #region Constructor

        internal Notifications(QueryRunner queryRunner) : base(queryRunner)
        {

        }

        #endregion

        #region Non Public Methods

        protected override Dictionary<string, Action<CommandParameterGroupList>> GetNotificationHandlers()
        {
            return new  Dictionary<string, Action<CommandParameterGroupList>>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "notifytalkstatuschange", HandleTalkStatusChange },
                { "notifyclientmoved", HandleClientMoved },
                { "notifyclientleftview", HandleClientMoved },
                { "notifycliententerview", HandleClientMoved },
                { "notifytextmessage", HandleTextMessage },

        
            };
        }

        private void HandleTextMessage(CommandParameterGroupList parameterGroupList)
        {
            TS3QueryLib.Core.Server.Notification.EventArgs.MessageReceivedEventArgs eventArgs = new TS3QueryLib.Core.Server.Notification.EventArgs.MessageReceivedEventArgs(parameterGroupList);
            OnTextMessage(eventArgs);
        }

        private void OnTextMessage(TS3QueryLib.Core.Server.Notification.EventArgs.MessageReceivedEventArgs args)
        {
            if (MessageReceived != null)
                MessageReceived(this, args);
        }


        private void HandleClientMoved(CommandParameterGroupList parameterGroupList)
        {
            TS3QueryLib.Core.Server.Notification.EventArgs.ClientMovedEventArgs eventArgs = new TS3QueryLib.Core.Server.Notification.EventArgs.ClientMovedEventArgs(parameterGroupList);

            OnClientMoved(eventArgs);

        }

        protected void OnClientMoved(TS3QueryLib.Core.Server.Notification.EventArgs.ClientMovedEventArgs args)
        {
            if (ClientMoved != null)
                ClientMoved(this, args);
        }


        private void HandleTalkStatusChange(CommandParameterGroupList parameterGroupList)
        {
            TalkStatusEventArgs eventArgs = new TalkStatusEventArgs(parameterGroupList);

            OnTalkStatusChanged(eventArgs);

            if (eventArgs.IsWisper)
                OnWisperTalkStatusChanged(eventArgs);
            else
                OnChannelTalkStatusChanged(eventArgs);
        }

        

        

        protected void OnTalkStatusChanged(TalkStatusEventArgs args)
        {
            if (TalkStatusChanged != null)
                TalkStatusChanged(this, args);
        }

        protected void OnChannelTalkStatusChanged(TalkStatusEventArgsBase args)
        {
            if (ChannelTalkStatusChanged != null)
                ChannelTalkStatusChanged(this, args);
        }

        protected void OnWisperTalkStatusChanged(TalkStatusEventArgsBase args)
        {
            if (WisperTalkStatusChanged != null)
                WisperTalkStatusChanged(this, args);
        }

        #endregion
    }
}
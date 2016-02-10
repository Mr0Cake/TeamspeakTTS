using System;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Common.Entities;

namespace TS3QueryLib.Core.Client.Entities
{
    public class ChannelListEntry
    {
        #region Properties

        #region Always returned Properties

        public bool IsSubscribed { get; protected set; }
        public uint ChannelId { get; protected set; }
        public uint ParentChannelId { get; protected set; }
        public uint Order { get; protected set; }
        public string Name { get; protected set; }
        public uint? TotalClients { get; protected set; }

        #endregion

        #endregion

        #region Public Methods

        protected void FillFrom(CommandParameterGroup currrentParameterGroup, CommandParameterGroup firstParameterGroup)
        {
            if (currrentParameterGroup == null)
                throw new ArgumentNullException("currentParameterGroup");

            IsSubscribed = currrentParameterGroup.GetParameterValue<byte>("channel_flag_are_subscribed").ToBool();
            ChannelId = currrentParameterGroup.GetParameterValue<uint>("cid");
            ParentChannelId = currrentParameterGroup.GetParameterValue<uint>("pid");
            Order = currrentParameterGroup.GetParameterValue<uint>("channel_order");
            Name = currrentParameterGroup.GetParameterValue("channel_name");
            TotalClients = currrentParameterGroup.GetParameterValue<uint?>("total_clients");

        }

        public static ChannelListEntry Parse(CommandParameterGroup currrentParameterGroup, CommandParameterGroup firstParameterGroup)
        {
            if (currrentParameterGroup == null)
                throw new ArgumentNullException("currrentParameterGroup");

            return new ChannelListEntry{
            IsSubscribed = currrentParameterGroup.GetParameterValue<byte>("channel_flag_are_subscribed").ToBool(),
            ChannelId = currrentParameterGroup.GetParameterValue<uint>("cid"),
            ParentChannelId = currrentParameterGroup.GetParameterValue<uint>("pid"),
            Order = currrentParameterGroup.GetParameterValue<uint>("channel_order"),
            Name = currrentParameterGroup.GetParameterValue("channel_name"),
            TotalClients = currrentParameterGroup.GetParameterValue<uint?>("total_clients"),
            };
        }

        #endregion
    }
}
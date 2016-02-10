using System;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Common.Entities;
using System.Collections.Generic;
using TS3QueryLib.Core.Common;

namespace TS3QueryLib.Core.Client.Entities
{
    public class ClientListEntry : IDump
    {
        #region Properties

        public uint ClientId { get; set; }
        public uint ChannelId { get; set; }
        public uint ClientDatabaseId { get; set; }
        public string Nickname { get; set; }
        public ushort ClientType { get; protected set; }
        public bool Talking { get; set; }
        

        #endregion

        #region Constructor

        private ClientListEntry()
        {
            
        }

        #endregion

        #region Public Methods

        public static ClientListEntry Parse(CommandParameterGroup currentParameterGroup, CommandParameterGroup firstParameterGroup)
        {
            if (currentParameterGroup == null)
                throw new ArgumentNullException("currentParameterGroup");

            return new ClientListEntry
            {
                ClientId = currentParameterGroup.GetParameterValue<uint>("clid"),
                ChannelId = currentParameterGroup.GetParameterValue<uint>("cid"),
                ClientDatabaseId = currentParameterGroup.GetParameterValue<uint>("client_database_id"),
                Nickname = currentParameterGroup.GetParameterValue("client_nickname"),
                ClientType = currentParameterGroup.GetParameterValue<ushort>("client_type"),
                
            };
        }

        #endregion
    }
}


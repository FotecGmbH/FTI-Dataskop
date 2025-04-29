// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BDA.Service.AppConnectivity.DataConnector;
using Biss.Dc.Core;
using Biss.Dc.Core.DcChat;
using Biss.Dc.Server.DcChat;
using Biss.Log.Producer;
using Database;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace ConnectivityHost.DataConnector.Chat
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse DcChat. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class DcChat : DcChatServerBase
    {
        // ReSharper disable once NotAccessedField.Local
        private ServerRemoteCalls _src;

        /// <summary>
        /// DcChat
        /// </summary>
        /// <param name="src"></param>
        public DcChat(ServerRemoteCalls src)
        {
            _src = src;
        }

        /// <inheritdoc />
        public override async Task<IDcChatUser> GetUser(long userId)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            //var user = db.TblUsers.AsNoTracking()
            //    .Where(w => w.Id == userId)
            //    .Select(s => (IDcChatUser) new ExDcChatUser()
            //                               {
            //                                   FullName = $"{s.FirstName} {s.LastName}",
            //                                   ImageLink = s.UserImageLink,
            //                                   Id = s.Id,
            //                                   DataVersion = s.DataVersion
            //                               }).First();
            return null!;
        }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <inheritdoc />
        public override async Task<(List<IDcChatUser>, List<long>)> GetChatUsers(long userId, List<DcSyncElement> currentUsers, DcEnumChatSyncMode chatSyncMode)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Logging.Log.LogWarning($"[{nameof(DcChat)}]({nameof(GetChatUsers)}): Chat noch nicht fertig!");
            return (new List<IDcChatUser>(), new List<long>());
        }


        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<(List<IDcChat> chats, List<long> removeChats)> GetChats(long userId, List<DcSyncElement> currentChats, DcEnumChatSyncMode chatSyncMode)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Logging.Log.LogWarning($"[{nameof(DcChat)}]({nameof(GetChats)}): Chat noch nicht fertig!");
            return (new List<IDcChat>(), new List<long>());
        }

        /// <summary>Chat-Einträge</summary>
        /// <param name="userId"></param>
        /// <param name="currentChatEntries"></param>
        /// <param name="chatSyncMode"></param>
        /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<(Dictionary<long, List<IDcChatEntry>> chatEntries, List<long> removeChatEntries)> GetChatEntries(long userId, Dictionary<long, List<DcSyncElement>> currentChatEntries, DcEnumChatSyncMode chatSyncMode)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Logging.Log.LogWarning($"[{nameof(DcChat)}]({nameof(GetChatEntries)}): Chat noch nicht fertig!");
            return (new Dictionary<long, List<IDcChatEntry>>(), new List<long>());
        }

        /// <inheritdoc />
        public override Task<DcChatData> Post(long deviceId, long userId, string msg, long? chatId = null, long? otherUserId = null, string chatName = "", string addData = "")
        {
            return Task.FromResult(new DcChatData());
        }
    }
}
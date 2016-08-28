// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using log4net;
using Thought.vCards;
using CalDavSynchronizer.ThoughtvCardWorkaround;

namespace CalDavSynchronizer.Implementation.Contacts
{
  public class CardDavRepository : IEntityRepository<vCard, WebResourceName, string, int>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ICardDavDataAccess _cardDavDataAccess;
    private readonly vCardImprovedWriter _vCardImprovedWriter;
  
    public CardDavRepository (ICardDavDataAccess cardDavDataAccess)
    {
      _cardDavDataAccess = cardDavDataAccess;
      _vCardImprovedWriter = new vCardImprovedWriter();
    }

    public Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetVersions (IEnumerable<IdWithAwarenessLevel<WebResourceName>> idsOfEntitiesToQuery, int context)
    {
      return _cardDavDataAccess.GetVersions (idsOfEntitiesToQuery.Select (i => i.Id));
    }

    public async Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetAllVersions (IEnumerable<WebResourceName> idsOfknownEntities, int context)
    {
      using (AutomaticStopwatch.StartInfo (s_logger, "CardDavRepository.GetVersions"))
      {
        return await _cardDavDataAccess.GetAllVersions();
      }
    }

    public async Task<IReadOnlyList<EntityWithId<WebResourceName, vCard>>> Get (ICollection<WebResourceName> ids, ILoadEntityLogger logger, int context)
    {
      if (ids.Count == 0)
        return new EntityWithId<WebResourceName, vCard>[] { };

      using (AutomaticStopwatch.StartInfo (s_logger, string.Format ("CardDavRepository.Get ({0} entitie(s))", ids.Count)))
      {
        var entities = await _cardDavDataAccess.GetEntities (ids);
        return ParallelDeserialize (entities, logger);
      }
    }

    public Task VerifyUnknownEntities (Dictionary<WebResourceName, string> unknownEntites, int context)
    {
      return Task.FromResult (0);
    }

    public void Cleanup (IReadOnlyDictionary<WebResourceName, vCard> entities)
    {
      // nothing to do
    }

    private IReadOnlyList<EntityWithId<WebResourceName, vCard>> ParallelDeserialize (IReadOnlyList<EntityWithId<WebResourceName, string>> serializedEntities, ILoadEntityLogger logger)
    {
      var result = new List<EntityWithId<WebResourceName, vCard>>();

      Parallel.ForEach (
          serializedEntities,
          () => Tuple.Create (new vCardStandardReader(), new List<Tuple<WebResourceName, vCard>>()),
          (serialized, loopState, threadLocal) =>
          {
            vCard vcard;

            if (TryDeserialize (serialized.Entity, out vcard, serialized.Id, threadLocal.Item1, logger))
              threadLocal.Item2.Add (Tuple.Create (serialized.Id, vcard));
            return threadLocal;
          },
          threadLocal =>
          {
            lock (result)
            {
              foreach (var card in threadLocal.Item2)
                result.Add (EntityWithId.Create (card.Item1, card.Item2));
            }
          });

      return result;
    }


    public async Task<bool> TryDelete (WebResourceName entityId, string version, int context)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        return await _cardDavDataAccess.TryDeleteEntity (entityId, version);
      }
    }

    public async Task<EntityVersion<WebResourceName, string>> TryUpdate (
        WebResourceName entityId,
        string entityVersion,
        vCard entityToUpdate,
        Func<vCard, Task<vCard>> entityModifier, 
        int context)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        vCard newVcard = new vCard();
        newVcard.UniqueId = (!string.IsNullOrEmpty (entityToUpdate.UniqueId)) ? entityToUpdate.UniqueId : Guid.NewGuid().ToString();
        newVcard = await entityModifier (newVcard);

        return await _cardDavDataAccess.TryUpdateEntity (entityId, entityVersion, Serialize (newVcard));
      }
    }

    public async Task<EntityVersion<WebResourceName, string>> Create (Func<vCard, Task<vCard>> entityInitializer, int context)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        vCard newVcard = new vCard();
        newVcard.UniqueId = Guid.NewGuid().ToString();
        var initializedVcard = await entityInitializer (newVcard);
        return await _cardDavDataAccess.CreateEntity (Serialize (initializedVcard), newVcard.UniqueId);
      }
    }


    private string Serialize (vCard vcard)
    {
      string newvCardString;

      using (var writer = new StringWriter())
      {
        _vCardImprovedWriter.Write (vcard, writer);
        writer.Flush();
        newvCardString = writer.GetStringBuilder().ToString();

        return newvCardString;
      }
    }

    private static bool TryDeserialize (
      string vcardData, 
      out vCard vcard, 
      WebResourceName uriOfAddressbookForLogging, 
      vCardStandardReader deserializer,
      ILoadEntityLogger logger)
    {
      vcard = null;
      string fixedVcardData = ContactDataPreprocessor.FixRevisionDate (vcardData);
      string fixedVcardData2 = ContactDataPreprocessor.FixUrlType (fixedVcardData);
      string fixedVcardData3 = ContactDataPreprocessor.FixPhoto(fixedVcardData2);

      try
      {
        vcard = Deserialize (fixedVcardData3, deserializer);
        return true;
      }
      catch (Exception x)
      {
        s_logger.Error (string.Format ("Could not deserialize vcardData of '{0}':\r\n{1}", uriOfAddressbookForLogging, fixedVcardData3), x);
        logger.LogSkipLoadBecauseOfError (uriOfAddressbookForLogging, x);
        return false;
      }
    }

    private static vCard Deserialize (string vcardData, vCardStandardReader serializer)
    {
      using (var reader = new StringReader (vcardData))
      {
        return serializer.Read (reader);
      }
    }
  }
}
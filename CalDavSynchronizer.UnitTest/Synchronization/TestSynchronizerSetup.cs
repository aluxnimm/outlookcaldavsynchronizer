// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using System.Collections.Generic;
using System.Linq;
using CalDavSynchronizer.Generic.EntityMapping;
using CalDavSynchronizer.Generic.EntityRelationManagement;
using CalDavSynchronizer.Generic.EntityRepositories;
using CalDavSynchronizer.Generic.InitialEntityMatching;
using CalDavSynchronizer.Generic.Synchronization;
using NUnit.Framework;

namespace CalDavSynchronizer.UnitTest.Synchronization
{
  internal class TestSynchronizerSetup : ISynchronizerContext<string, int, string, string, int, string>
  {

    public TestRepository _localRepository;
    public TestRepository _serverRepository;

    protected ISynchronizer _synchronizer;
    private readonly IEntityRelationDataFactory<string, int, string, int> _entityRelationDataFactory;
    private List<EntityRelationData> _entityRelationData = new List<EntityRelationData>();

    public TestSynchronizerSetup ()
    {

      _localRepository = new TestRepository ("l");
      _serverRepository = new TestRepository ("s");

      _entityRelationDataFactory = new EntityRelationDataFactory();
    }

    public void AssertServerCount (int i)
    {
      Assert.AreEqual (i, _serverRepository.Count);
    }

    public void AssertLocalCount (int i)
    {
      Assert.AreEqual (i, _localRepository.Count);
    }

    public void AssertServer (string id, int version, string content)
    {
      Assert.AreEqual (Tuple.Create (version, content), _serverRepository[id]);
    }

    public void AssertLocal (string id, int version, string content)
    {
      Assert.AreEqual (Tuple.Create (version, content), _localRepository[id]);
    }

    public void AssertLocal (int version, string content)
    {
      CollectionAssert.Contains (_localRepository.EntityVersionAndContentById.Values, Tuple.Create (version, content));
    }

    public void AssertServer (int version, string content)
    {
      CollectionAssert.Contains (_serverRepository.EntityVersionAndContentById.Values, Tuple.Create (version, content));
    }


    public void InitializeWithTwoEvents ()
    {
      var v1 = _localRepository.Create (v => "Item 1");
      var v2 = _localRepository.Create (v => "Item 2");

      var v3 = _serverRepository.Create (v => "Item 1");
      var v4 = _serverRepository.Create (v => "Item 2");

      _entityRelationData.Add (new EntityRelationData (
          v1.Id,
          v1.Version,
          v3.Id,
          v3.Version
        ));

      _entityRelationData.Add (new EntityRelationData (
          v2.Id,
          v2.Version,
          v4.Id,
          v4.Version
        ));
    }

    public IEntityMapper<string, string> EntityMapper
    {
      get { return new Mapper(); }
    }

    public IEntityRepository<string, string, int> AtypeRepository
    {
      get { return _localRepository; }
    }

    public IEntityRepository<string, string, int> BtypeRepository
    {
      get { return _serverRepository; }
    }


    public DateTime From
    {
      get { return DateTime.Now; }
    }

    public DateTime To
    {
      get { return DateTime.Now; }
    }

    public IInitialEntityMatcher<string, string, int, string, string, int> InitialEntityMatcher {
      get { throw new NotImplementedException(); }
    }

    public IEntityRelationDataFactory<string, int, string, int> EntityRelationDataFactory
    {
      get { return _entityRelationDataFactory; }
    }
    

    public IEnumerable<IEntityRelationData<string, int, string, int>> LoadEntityRelationData ()
    {
      return _entityRelationData.ToArray();
    }

    public void SaveEntityRelationData (List<IEntityRelationData<string, int, string, int>> data)
    {
      _entityRelationData = data.Cast<EntityRelationData>().ToList();
    }
  }
}
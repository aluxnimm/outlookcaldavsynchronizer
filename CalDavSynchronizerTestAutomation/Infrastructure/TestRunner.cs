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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  internal class TestRunner
  {
    private ITestDisplay _testDisplay;

    public TestRunner (ITestDisplay testDisplay)
    {
      if (testDisplay == null)
        throw new ArgumentNullException ("testDisplay");

      _testDisplay = testDisplay;
    }

    public void Run (Assembly testAssembly, bool excludeManual)
    {
      var testFixtures = testAssembly
          .GetTypes()
          .Where (t => t.GetCustomAttributes (typeof (TestFixtureAttribute), true).Any());


      var allTests =
          (from testFixture in testFixtures
            from test in testFixture.GetMethods().Where (m =>
                m.GetCustomAttributes (typeof (TestAttribute), true).Any()
                && !(excludeManual && m.GetCustomAttributes (typeof (ContainsManualAssertAttribute), true).Any()))
            select new
                   {
                       Fixture = testFixture,
                       Test = test
                   }).ToArray();

      Array.ForEach (allTests, t => _testDisplay.SetRunPending (t.Test));

      foreach (var fixture in allTests.GroupBy (t => t.Fixture))
        Run (fixture.Key, fixture.Select (f => f.Test));
    }


    private void Run (Type testFixture, IEnumerable<MethodInfo> tests)
    {
      var testFixtureInstance = testFixture.GetConstructor (Type.EmptyTypes).Invoke (new object[] { });
      foreach (var test in tests)
        Run (testFixtureInstance, test);
    }

    private void Run (object testFixture, MethodInfo test)
    {
      try
      {
        test.Invoke (testFixture, new object[] { });
        _testDisplay.SetPassed (test);
      }
      catch (TargetInvocationException x)
      {
        _testDisplay.SetFailed (test, x.InnerException);
      }
    }
  }
}
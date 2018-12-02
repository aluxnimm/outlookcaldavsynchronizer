using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Config;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests
{
    [SetUpFixture]
    public class SetupFixture
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            XmlConfigurator.Configure();
        }
    }
}

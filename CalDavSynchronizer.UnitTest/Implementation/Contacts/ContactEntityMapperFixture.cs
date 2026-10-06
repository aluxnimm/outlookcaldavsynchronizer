using System;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using GenSync.Logging;
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;
using Rhino.Mocks;
using Thought.vCards;

namespace CalDavSynchronizer.UnitTest.Implementation.Contacts
{
    [TestFixture]
    public class ContactEntityMapperFixture
    {
        [TestCase(0, "", "", "")]
        [TestCase(1, "email0@example.invalid", "", "")]
        [TestCase(3, "email0@example.invalid", "email1@example.invalid", "email2@example.invalid")]
        public async Task Map2To1_EmailsWithinOutlookLimit_DoesNotWarn(int count, string first, string second, string third)
        {
            var source = CreateContact(count);
            var logger = CreateLogger();

            var target = await MapContact(source, false, logger);

            Assert.That(logger.GetReport().Warnings, Is.Empty);
            AssertEmails(target, first, second, third);
        }

        [TestCase(4)]
        [TestCase(6)]
        public async Task Map2To1_TooManyEmails_ReportsOneActionableItemWarning(int count)
        {
            var source = CreateContact(count);
            var logger = CreateLogger();
            logger.SetBId("/contact.vcf");

            var target = await MapContact(source, false, logger);

            var report = logger.GetReport();
            Assert.That(report.BId, Is.EqualTo("/contact.vcf"));
            Assert.That(report.Warnings, Has.Length.EqualTo(1));
            Assert.That(report.Warnings[0], Does.Contain(count + " email addresses")
                .And.Contain("Outlook").And.Contain("3 email addresses")
                .And.Contain("not synchronized").And.Contain("Review").And.Contain("source contact"));
            Assert.That(report.Errors, Is.Empty);
            AssertEmails(target, "email0@example.invalid", "email1@example.invalid", "email2@example.invalid");
            Assert.That(source.EmailAddresses.Count, Is.EqualTo(count));
        }

        [TestCase(3, false, "email1@example.invalid", "email2@example.invalid")]
        [TestCase(3, true, "email2@example.invalid", "email1@example.invalid")]
        [TestCase(4, false, "email1@example.invalid", "email2@example.invalid")]
        [TestCase(4, true, "email2@example.invalid", "email1@example.invalid")]
        [TestCase(6, false, "email1@example.invalid", "email2@example.invalid")]
        [TestCase(6, true, "email2@example.invalid", "email1@example.invalid")]
        public async Task Map2To1_Emails_PreservesHomeWorkSelection(int count, bool workFirst, string first, string second)
        {
            var source = CreateContact(count);
            source.EmailAddresses[1].ItemType = ItemType.HOME;
            source.EmailAddresses[2].ItemType = ItemType.WORK;
            var logger = CreateLogger();

            var target = await MapContact(source, workFirst, logger);

            AssertEmails(target, first, second, "email0@example.invalid");
            Assert.That(logger.GetReport().Warnings, Has.Length.EqualTo(count > 3 ? 1 : 0));
        }

        private static vCard CreateContact(int count)
        {
            var source = new vCard();
            for (int i = 0; i < count; i++)
                source.EmailAddresses.Add(new vCardEmailAddress("email" + i + "@example.invalid"));
            return source;
        }

        private static EntitySynchronizationLogger<string, IContactItemWrapper, string, vCard> CreateLogger()
        {
            return new EntitySynchronizationLogger<string, IContactItemWrapper, string, vCard>(
                SynchronizationOperation.UpdateInA,
                MockRepository.GenerateStub<IEntityLogMessageFactory<IContactItemWrapper, vCard>>());
        }

        private static async Task<ContactItem> MapContact(vCard source, bool workFirst, IEntitySynchronizationLogger logger)
        {
            // RhinoMocks cannot proxy the TypeIdentifier attribute of an embedded interop type.
            var contactType = Type.GetType(typeof(ContactItem).FullName +
                ", Microsoft.Office.Interop.Outlook, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", true);
            var contact = (ContactItem) MockRepository.GenerateStub(contactType);
            contact.Email1Address = contact.Email2Address = contact.Email3Address = "old@example.invalid";
            contact.Email1DisplayName = contact.Email2DisplayName = contact.Email3DisplayName = "Old";
            var target = MockRepository.GenerateStub<IContactItemWrapper>();
            target.Stub(t => t.Inner).Return(contact);
            var mapper = new ContactEntityMapper(
                new ContactMappingConfiguration {MapOutlookEmail1ToWork = workFirst},
                () => { throw new InvalidOperationException("Network access is not expected."); });

            Assert.That(await mapper.Map2To1(source, target, logger, null), Is.SameAs(target));
            return contact;
        }

        private static void AssertEmails(ContactItem target, string first, string second, string third)
        {
            Assert.That(new[] {target.Email1Address, target.Email2Address, target.Email3Address},
                Is.EqualTo(new[] {first, second, third}));
            Assert.That(new[] {target.Email1DisplayName, target.Email2DisplayName, target.Email3DisplayName},
                Is.EqualTo(new[] {"", "", ""}));
        }
    }
}

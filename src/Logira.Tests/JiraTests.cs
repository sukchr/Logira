// ReSharper disable InconsistentNaming

using System;
using Logira.ServiceV2;
using NUnit.Framework;
using Shouldly;

namespace Logira.Tests
{
    [TestFixture]
    public class JiraTests
    {
        [Test]
        public void Create_issue_fails_when_service_is_not_configured()
        {
            Should.Throw<InvalidOperationException>(() => Jira.CreateIssue("foo", new RemoteIssue()));
        }

        [Test]
        public void Add_attachment_fails_when_service_is_not_configured()
        {
            Should.Throw<InvalidOperationException>(() => Jira.AddAttachments("foo", new RemoteIssue(), null));
        }
    }
}

// ReSharper restore InconsistentNaming
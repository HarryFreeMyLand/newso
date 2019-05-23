using System.Collections.Generic;
using FSO.SimAntics.NetPlay.EODs.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSO.Tests
{
    [TestClass]
    public class EODTests
    {
        [TestMethod]
        public void Newspaper()
        {
            var expected = "Test Event 1";
            var article = "This event should show up as the latest event. It has a description " +
                    "which is too long to fit within the preview button, so the user has to click to " +
                    "expand it into the upper view.";
            var news = new List<VMEODFNewspaperNews>()
            {
                new VMEODFNewspaperNews()
                {
                    Name = expected,
                },
            };

            Assert.AreNotEqual(news, news2);
        }
    }
}

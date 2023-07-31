using WikidataEditor.Extensions;

namespace WikidataEditorTests
{
    [TestClass]
    public class StringExtensionTests
    {
        [TestMethod]
        public void DateStringShouldParseToDateOnly()
        {
            var actual = "4 May 1919".ParseDateOnlyWikidata();
            var expected = new DateOnly(1919, 5, 4);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DateStringYearShouldParseToDateOnly()
        {
            var actual = "1919".ParseDateOnlyWikidata();
            var expected = new DateOnly(1919, 1, 1);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DateStringMonthYearShouldParseToDateOnly()
        {
            var actual = "May 1919".ParseDateOnlyWikidata();
            var expected = new DateOnly(1919, 5, 1);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void InvalidDateStringShouldParseToMinDateOnly()
        {
            var actual = "some invalid date".ParseDateOnlyWikidata();
            var expected = DateOnly.MinValue;
            Assert.AreEqual(expected, actual);
        }
    }
}
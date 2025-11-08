using System.Collections;

namespace Quizzer.Api.FunctionalTest.DataCollection;

internal class TagDataCollection : IEnumerable<object[]>
{
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { "Science", "Questions related to science topics." };
        yield return new object[] { "History", "Questions about historical events and figures." };
        yield return new object[] { "Geography", null! };
        yield return new object[] { "Mathematics", "Questions involving numbers and calculations." };
        yield return new object[] { "Literature", "Questions about books, authors, and literary terms." };
    }
}


internal class DuplicateTagDataCollection : IEnumerable<object[]>
{
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { "Science", "Questions related to science topics." };
    }
}

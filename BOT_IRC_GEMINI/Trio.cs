namespace BOT_IRC_GEMINI;

[Serializable]
public class Trio<T1, T2, T3>
{
    public T1 Item1 { get; set; }
    public T2 Item2 { get; set; }
    public T3 Item3 { get; set; }

    public Trio(T1 item1 = default, T2 item2 = default, T3 item3 = default)
    {
        Item1 = item1;
        Item2 = item2;
        Item3 = item3;
    }

    public Trio()
    {
    }

    #region ForEach Compatibility

    /*public IEnumerator GetEnumerator()
    {
        return (IEnumerator)this;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();*/

    #endregion
}
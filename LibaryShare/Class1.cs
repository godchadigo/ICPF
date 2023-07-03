namespace LibaryShare
{
    public class LSManager
    {
        public event EventHandler<EventArgs> TEvent;

        public static LSManager Instance = new LSManager();
        public void Test()
        {
            TEvent?.Invoke(this, null);
        }
    }
}
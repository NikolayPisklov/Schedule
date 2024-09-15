namespace Schedule.Command
{
    public class Messanger
    {
        public delegate void ViewChangedEventHandler(object newViewModel);

        public event ViewChangedEventHandler? ViewChanged;

        private static Messanger? instance;
        public static Messanger Instance => instance ?? (instance = new Messanger());

        public void Send(object newViewModel) 
        {
            ViewChanged?.Invoke(newViewModel);
        }
    }
}

namespace Schedule.Command
{
    public class Messanger
    {
        public delegate void ViewChangedEventHandler(object newViewModel);
        public delegate void SessionCreationEventHandler(object userSessionService);
        public delegate void ScheduleDoneHandler();

        public event ViewChangedEventHandler? ViewChanged;
        public event SessionCreationEventHandler? SessionCreation;
        public event ScheduleDoneHandler? ScheduleDone;

        private static Messanger? instance;
        public static Messanger Instance => instance ?? (instance = new Messanger());

        public void ViewChangedSend(object newViewModel) 
        {
            ViewChanged?.Invoke(newViewModel);
        }
        public void SessionCreationSend(object userSessionService) 
        {
            SessionCreation?.Invoke(userSessionService);
        }
        public void ScheduleDoneSend() 
        {
            ScheduleDone?.Invoke();
        }
    }
}

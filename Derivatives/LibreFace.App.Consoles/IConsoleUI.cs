namespace LibreFace.App.Consoles {

    internal interface IConsoleUI {

        void Initialize(IReadOnlyList<string> files);

        void SetState(int index, State state);

        void SetElapsed(int index, TimeSpan elapsed);

        void SetProcessed(int index, TimeSpan processed);
    }
}

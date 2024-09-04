namespace MappingAI
{
    public interface ICommand
    {
        bool Execute();
        void Undo();
        void Redo();
    }

    public interface ICommandWithResult<T> : ICommand
    {
        T Result { get; }
    }
}



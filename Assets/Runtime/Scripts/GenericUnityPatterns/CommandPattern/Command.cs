using System;

/// <summary>
/// Command pattern implementation loosely based on the following source material:
/// https://www.richard-banks.org/2008/09/generic-command-pattern-in-net-35.html
/// </summary>
namespace GenericUnityPatterns.CommandPattern
{
    public interface ICommand
    {
        void Execute();
    }

    public interface IReversibleCommand
    {
        void UndoExecute();
    }

    [Serializable]
    public class SerializableCommand<T> : ICommand
    {
        protected T target;
        protected Action<T> command;

        public SerializableCommand(T target, Action<T> command)
        {
            this.target = target;
            this.command = command;
        }

        public void Execute()
        {
            command(target);
        }
    }

    [Serializable]
    public class SerializableReversibleCommand<T> : SerializableCommand<T>, IReversibleCommand
    {
        private Action<T> undoCommand;

        public SerializableReversibleCommand(T target, Action<T> command, Action<T> undoCommand) : base(target, command)
        {
            this.undoCommand = undoCommand;
        }

        public void UndoExecute()
        {
            undoCommand(target);
        }
    }
}
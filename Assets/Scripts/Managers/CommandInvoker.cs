using UnityEngine;
using System.Collections.Generic;

public class CommandInvoker : MonoBehaviour
{
    private Stack<ICommand> undoStack = new Stack<ICommand>();
    private Stack<ICommand> redoStack = new Stack<ICommand>();

    private static CommandInvoker Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        undoStack.Push(command);
        redoStack.Clear(); // new action invalidates redo history
    }

    public void Undo()
    {
        if (undoStack.Count == 0)
            return;
        
        ICommand command = undoStack.Pop();
        command.Undo();
        redoStack.Push(command);
    }

    public void Redo()
    {
        if (redoStack.Count == 0)
            return;
        
        ICommand command = redoStack.Pop();
        command.Execute();
        undoStack.Push(command);
    }
}
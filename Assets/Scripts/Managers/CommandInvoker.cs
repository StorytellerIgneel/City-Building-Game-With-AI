using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Obsolute system component. Was previously part of the architecture for undo and redo actions, where each action executed through this handles the do and undo logic. Currently not in use, but can be reintroduced if  want to add undo/redo functionality for player actions in the future.
/// Given up due to the imbalanced cost between implementation and actual benefits in a small system like this project, but the pattern can be useful for larger projects with more complex interactions and a need for robust undo/redo functionality.
/// Currently the actions for placement are handled by demolish, while the upgrade, demolish, and placeroad and not undoable. 
/// </summary>
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
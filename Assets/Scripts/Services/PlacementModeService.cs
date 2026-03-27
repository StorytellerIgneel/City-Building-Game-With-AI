public class PlacementModeService
{
    public PlacementMode CurrentMode { get; private set; } = PlacementMode.None;

    public bool IsIdle => CurrentMode == PlacementMode.None;

    public bool TryEnterMode(PlacementMode mode)
    {
        // second cond to ensure if we are trying to enter the same mode, we allow it to proceed (useful for cases like road placement where we want to reset the state if the button is clicked again)
        if (CurrentMode != PlacementMode.None && CurrentMode != mode)
            return false;

        CurrentMode = mode;
        return true;
    }

    public void ExitMode(PlacementMode mode)
    {
        if (CurrentMode == mode)
        {
            CurrentMode = PlacementMode.None;
        }
    }

    public bool IsInMode(PlacementMode mode)
    {
        return CurrentMode == mode;
    }
}
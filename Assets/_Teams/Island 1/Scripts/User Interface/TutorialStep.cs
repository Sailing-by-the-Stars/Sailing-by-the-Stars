namespace Assets._Teams.Island_1.Scripts.User_Interface
{
    /// <summary>
    /// Specifies the steps in the game's tutorial sequence.
    /// </summary>
    /// <remarks>
    /// Use this enumeration to track or control the player's progress through the tutorial. The
    /// steps are ordered to guide new players through basic controls and interactions. Some steps may not display a
    /// popup but are included for completeness and future extensibility.
    /// </remarks>
    public enum TutorialStep
    {
        // Default value, should not be used
        None,

        // This will be the first step, so that players can get used to the controls before being asked to do something specific, will be shown directly after the player wakes up in the game
        LookAround,

        // This will be shown so that players can get used to moving before being asked to do something specific, will be shown directly after the look around tutorial
        Move,

        Sprint,

        // This will be shown when the player approaches the first item, the Astrolabe, so that they can get used to picking up items, will be shown directly after the move tutorial
        PickUpItem,

        // This will be to show how to use the item, so that players can get used to using items, will be shown directly after the pickup tutorial, and will be triggered when the player picks up the Astrolabe
        TiltAstrolabe,

        // This will show the press TAB to toggle the Astrolabe
        ToggleAstrolabe,

        // This will be shown after picking up the journal, it will tell the player to use Q and E to browse through the journal
        NavigateJournal,

        // This will show the press ???? to toggle the journal
        ToggleJournal,

        // This is the last step, most likely there won't be a popup, but it's here just in case 
        Completed,

        RotateAstrolabe,
    }
}

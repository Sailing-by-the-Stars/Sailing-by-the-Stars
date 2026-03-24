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

        // This will be the second step, so that players can get used to moving before being asked to do something specific, will be shown directly after the look around tutorial
        Move,
        
        // This will be the third step, most likely there won't be a popup, but it's here just in case
        GoToTown,

        // This will be the fourth step, most likely there won't be a popup, but it's here just in case 
        TalkToNPC,

        // This will be the fifth step, most likely there won't be a popup, but it's here just in case
        FindItem,

        // This will be the sixth step, this will be triggered when the player finds the item, so that they can get used to picking up items
        PickUpItem,

        // This will be the seventh step, this will be triggered when the player picks up the item, so that they can get used to using items
        UseItem,

        // This will be the eighth step, this will be triggered when the player uses the item, so that they can get used to dropping items
        DropItem,

        // This is the last step, most likely there won't be a popup, but it's here just in case 
        Completed
    }
}

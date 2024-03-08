namespace komikaan.Enums;

public enum JourneyExpectation
{
    /// <summary>
    /// Everything is as usual, your journey can be completed without even thinking about it
    /// </summary>
    Full,
    /// <summary>
    /// The journey can be completed but there are issues along the way
    /// </summary>
    Maybe,
    /// <summary>
    /// Currently you can't complete this journey normally
    /// </summary>
    Nope,
    /// <summary>
    /// We simply don't know if the journey can be completed
    /// </summary>
    Unknown
}

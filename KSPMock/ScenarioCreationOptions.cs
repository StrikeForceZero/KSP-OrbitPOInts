namespace KSPMock
{
    public enum ScenarioCreationOptions
    {
        None = 0,
        AddToNewSandboxGames = 2,
        AddToExistingSandboxGames = 4,
        AddToNewScienceSandboxGames = 8,
        AddToExistingScienceSandboxGames = 16, // 0x00000010
        AddToNewCareerGames = 32, // 0x00000020
        AddToNewGames = 42, // 0x0000002A
        AddToExistingCareerGames = 64, // 0x00000040
        AddToExistingGames = 84, // 0x00000054
        AddToAllGames = 126, // 0x0000007E
        RemoveFromSandboxGames = 128, // 0x00000080
        RemoveFromScienceSandboxGames = 256, // 0x00000100
        RemoveFromCareerGames = 512, // 0x00000200
        AddToNewMissionGames = 1024, // 0x00000400
        AddToExistingMissionGames = 2048, // 0x00000800
        AddToAllMissionGames = 3072, // 0x00000C00
        RemoveFromMissionGames = 4096, // 0x00001000
        RemoveFromAllGames = 4992, // 0x00001380
    }
}

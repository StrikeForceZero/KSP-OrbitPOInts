namespace KSPMock
{
    public interface IDiscoverable
    {
        // TODO: DiscoveryInfo not stubbed
        // DiscoveryInfo DiscoveryInfo { get; }

        string RevealName();

        string RevealDisplayName();

        double RevealSpeed();

        double RevealAltitude();

        string RevealSituationString();

        string RevealType();

        float RevealMass();
    }
}

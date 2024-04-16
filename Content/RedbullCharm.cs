namespace CupAPI.Content {
    public class RedbullCharm : IEquipInfo
    {
        public string DisplayName => "Red Bull";
        public string Subtext => "Wings";
        public string Description => "Grants you wings, allowing you to fall slower with quicker reaction time.";
        public string BundleName => "cupapi_content";
        public string[] GreyIcons => [];
        public string[] NormalIcons =>
        [
            "charm_redbull0",
            "charm_redbull1",
            "charm_redbull2"
        ];
    }
}
namespace WildlifeTracker.ViewModels.Reports
{
    public class EndangeredSpeciesVm : ReportSelectYearVm
    {
        public List<EndangeredRowVm> Items { get; set; } = new();
    }

    public class EndangeredRowVm
    {
        public string SpeciesName { get; set; } = "";
        public int InitialTotal { get; set; }
        public int CurrentTotal { get; set; }
    }
}

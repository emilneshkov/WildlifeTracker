namespace WildlifeTracker.ViewModels.Reports
{
    public class SettlementMatrixVm : ReportSelectYearVm
    {
        public List<string> SpeciesHeaders { get; set; } = new();
        public List<SettlementMatrixRowVm> Rows { get; set; } = new();
    }

    public class SettlementMatrixRowVm
    {
        public string SettlementName { get; set; } = "";
        public List<string> Cells { get; set; } = new();
    }
}

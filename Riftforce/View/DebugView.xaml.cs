using ReactiveUI;

namespace Riftforce
{
    public class DebugViewBase : ReactiveWindow<DebugViewModel> { }

    public partial class DebugView : DebugViewBase
    {
        public DebugView()
        {
            this.InitializeComponent();
            this.ViewModel = new DebugViewModel();

            this.OneWayBind(this.ViewModel, vm => vm.Lines, v => v.Lines.ItemsSource);
        }
    }
}

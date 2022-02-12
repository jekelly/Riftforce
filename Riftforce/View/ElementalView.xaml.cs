using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ReactiveUI;

namespace Riftforce
{

    public class ElementalViewModelBase : ReactiveUserControl<ElementalViewModel> { }
    /// <summary>
    /// Interaction logic for ElementalView.xaml
    /// </summary>
    public partial class ElementalView : ElementalViewModelBase
    {
        public ElementalView()
        {
            InitializeComponent();

            this.WhenActivated((disposal) =>
            {
                this.BindCommand(this.ViewModel, vm => vm.ActivateCommand, v => v.Button).DisposeWith(disposal);
            });
        }
    }
}

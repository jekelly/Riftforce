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
    public class LocationViewBase : ReactiveUserControl<LocationViewModel> { }

    public partial class LocationView : LocationViewBase
    {
        public LocationView()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.ElementalTwo, v => v.Enemy.ItemsSource).DisposeWith(disposable);
                this.OneWayBind(this.ViewModel, vm => vm.ElementalOne, v => v.MyElementals.ItemsSource).DisposeWith(disposable);
                this.BindCommand(this.ViewModel, vm => vm.Command, v => v.PlayButton);
            });
        }
    }
}

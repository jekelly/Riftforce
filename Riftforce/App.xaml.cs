using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;
using Splat;

namespace Riftforce
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Locator.CurrentMutable.Register(() => new LocationView(), typeof(IViewFor<LocationViewModel>));
            Locator.CurrentMutable.Register(() => new ElementalView(), typeof(IViewFor<ElementalViewModel>));
            Locator.CurrentMutable.Register(() => new DebugView(), typeof(IViewFor<DebugViewModel>));
        }
    }
}

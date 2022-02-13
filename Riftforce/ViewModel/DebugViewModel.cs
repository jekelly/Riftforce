using System;
using DynamicData;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

namespace Riftforce
{
    public static class Log
    {
        private static readonly Subject<string> output;

        public static IObservable<string> Output => output.AsObservable();

        static Log()
        {
            output = new Subject<string>();
        }

        public static void WriteLine(string s) => output.OnNext(s);
    }

    public class DebugViewModel : ReactiveObject, IActivatableViewModel
    {
        private readonly ReadOnlyObservableCollection<string> lines;
        public ReadOnlyObservableCollection<string> Lines => this.lines;

        public ViewModelActivator Activator { get; }

        public DebugViewModel()
        {
            this.Activator = new ViewModelActivator();
            var disposeMe = Log.Output.ToObservableChangeSet().Select(x => x).Bind(out this.lines).Subscribe();
            // TODO: this doesn't get called when window is closed, figure out why
            this.WhenActivated(disposables =>
            {
                disposeMe.DisposeWith(disposables);
            });
        }
    }
}
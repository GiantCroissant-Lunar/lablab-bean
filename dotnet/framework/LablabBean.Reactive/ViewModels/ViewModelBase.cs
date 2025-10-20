using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace LablabBean.Reactive.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
    [Reactive]
    public string Title { get; set; } = string.Empty;

    [Reactive]
    public bool IsBusy { get; set; }

    protected ViewModelBase()
    {
    }
}

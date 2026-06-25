using CommunityToolkit.Mvvm.ComponentModel;

namespace MutiManagerForMe.App.ViewModels;

public abstract class PageViewModel : ObservableObject
{
    public abstract string Title { get; }
    public virtual Task LoadAsync() => Task.CompletedTask;
}

public sealed record NavigationItem(string Label, string Glyph, PageViewModel Page);
public sealed record PriorityChoice(int Value, string Label);

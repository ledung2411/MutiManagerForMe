using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutiManagerForMe.App.Data;
using MutiManagerForMe.App.Models;
using MutiManagerForMe.App.Services;
using System.Collections.ObjectModel;

namespace MutiManagerForMe.App.ViewModels;

public partial class NotesViewModel(DatabaseService database, IUserDialogService dialogs) : PageViewModel
{
    public override string Title => "Ghi chú";
    public ObservableCollection<NoteEntry> Items { get; } = [];

    [ObservableProperty] private NoteEntry? selectedNote;
    [ObservableProperty] private string editorTitle = string.Empty;
    [ObservableProperty] private string editorContent = string.Empty;
    [ObservableProperty] private bool isPinned;
    [ObservableProperty] private string validationMessage = string.Empty;

    public override async Task LoadAsync()
    {
        var selectedId = SelectedNote?.Id;
        var notes = await database.GetNotesAsync();
        Items.Clear();
        foreach (var note in notes) Items.Add(note);

        if (selectedId is not null)
        {
            SelectedNote = Items.FirstOrDefault(n => n.Id == selectedId);
        }
    }

    partial void OnSelectedNoteChanged(NoteEntry? value)
    {
        if (value is null) return;
        EditorTitle = value.Title;
        EditorContent = value.Content;
        IsPinned = value.IsPinned;
        ValidationMessage = string.Empty;
    }

    [RelayCommand]
    private void New()
    {
        SelectedNote = null;
        EditorTitle = string.Empty;
        EditorContent = string.Empty;
        IsPinned = false;
        ValidationMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditorTitle))
        {
            ValidationMessage = "Nhập tiêu đề ghi chú.";
            return;
        }

        var id = await database.SaveNoteAsync(new NoteEntry
        {
            Id = SelectedNote?.Id ?? 0,
            Title = EditorTitle,
            Content = EditorContent,
            IsPinned = IsPinned
        });
        ValidationMessage = string.Empty;
        await LoadAsync();
        SelectedNote = Items.FirstOrDefault(n => n.Id == id);
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedNote is null || !dialogs.Confirm($"Xóa ghi chú “{SelectedNote.Title}”?")) return;
        await database.DeleteNoteAsync(SelectedNote.Id);
        New();
        await LoadAsync();
    }
}

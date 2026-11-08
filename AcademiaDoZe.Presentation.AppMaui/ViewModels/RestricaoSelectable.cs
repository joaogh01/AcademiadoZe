using AcademiaDoZe.Application.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

// Utilitário para seleção múltipla (CheckBoxes) com enum Flags
public class RestricaoSelectable : ObservableObject
{
    public AppMatriculaRestricoes Value { get; set; }

    public string Descricao => Value.ToString();

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
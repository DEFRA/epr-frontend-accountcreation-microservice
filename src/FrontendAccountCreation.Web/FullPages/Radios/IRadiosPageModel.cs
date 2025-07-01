using FrontendAccountCreation.Web.ErrorNext;
using Microsoft.AspNetCore.Html;

namespace FrontendAccountCreation.Web.FullPages.Radios;

public interface IRadiosPageModel
{
    IEnumerable<IRadio> Radios { get; }

    string? SelectedValue { get; set; }

    /// <summary>
    /// In some cases, you can choose to display radios ‘inline’ beside one another (horizontally).
    ///
    /// Only use inline radios when:
    ///
    /// * the question only has two options
    /// * both options are short
    ///
    /// Remember that on small screens such as mobile devices, the radios will still be ‘stacked’ on top of one another (vertically).
    /// </summary>
    bool AreRadiosInline => false;

    IErrorState Errors { get; }

    string? DescriptionPartial => null;

    string? Legend { get; }

    string? Hint => null;
    string? ButtonText => "Continue";
}
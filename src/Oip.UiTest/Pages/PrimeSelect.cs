namespace Oip.UiTest.Pages;

/// <summary>
/// A PrimeNG Select. Its options overlay is appended to the body.
/// </summary>
internal class PrimeSelect(IWebDriver driver, By host) : BasePage(driver)
{
    private static readonly By Label = By.CssSelector(".p-select-label");
    private static readonly By Filter = By.CssSelector("input.p-select-filter");
    private static readonly By Options = By.CssSelector("li[role='option']");

    /// <summary>
    /// Label of the selected option as shown in the select.
    /// </summary>
    public string SelectedText => Wait.UntilFindElement(host).FindElement(Label).Text.Trim();

    /// <summary>
    /// Picks the option with the given label. Options carry their label in <c>aria-label</c>, also when
    /// the select renders them with a custom template.
    /// </summary>
    /// <param name="text">Option label.</param>
    /// <param name="filter">Types the label into the filter first, for long or virtually scrolled lists.</param>
    public void Choose(string text, bool filter = false)
    {
        Wait.UntilClick(host);
        if (filter)
            Wait.UntilFindElement(Filter).SendKeys(text);

        var option = By.CssSelector($"li[role='option'][aria-label='{text}']");
        Wait.UntilClick(option);
        Wait.UntilDisappear(option);
    }

    /// <summary>
    /// Picks the option at the given position, for options whose labels depend on the interface language.
    /// </summary>
    /// <param name="index">Zero-based position of the option.</param>
    public void ChooseAt(int index)
    {
        Wait.UntilClick(host);
        // The overlay opens with an animation, during which a click can land on a neighbouring option.
        Wait.Until(d => d.FindElements(Options)[index].Click());
        Wait.UntilDisappear(Options);
    }
}

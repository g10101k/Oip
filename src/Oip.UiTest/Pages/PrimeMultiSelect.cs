namespace Oip.UiTest.Pages;

/// <summary>
/// A PrimeNG MultiSelect. Its options overlay is appended to the body and covers the elements below it,
/// so every change closes the overlay before returning.
/// </summary>
internal class PrimeMultiSelect(IWebDriver driver, By host) : BasePage(driver)
{
    /// <summary>
    /// Slightly longer than the 150 ms PrimeNG MultiSelect ignores container clicks after the previous one.
    /// </summary>
    private const int ClickGuardMilliseconds = 300;

    /// <summary>
    /// Selects the option, keeping the options already selected.
    /// </summary>
    /// <param name="text">Option label.</param>
    public void Select(string text) => Toggle(text, selected: true);

    /// <summary>
    /// Clears the option, keeping the other selected options.
    /// </summary>
    /// <param name="text">Option label.</param>
    public void Deselect(string text) => Toggle(text, selected: false);

    /// <summary>
    /// Opens the overlay and reports whether the option is selected.
    /// </summary>
    /// <param name="text">Option label.</param>
    public bool IsSelected(string text)
    {
        var isSelected = IsOptionSelected(Open(text));
        Close(text);
        return isSelected;
    }

    private void Toggle(string text, bool selected)
    {
        var option = Open(text);
        if (IsOptionSelected(option) != selected)
            option.Click();
        Close(text);
    }

    private IWebElement Open(string text)
    {
        Wait.UntilClick(host);
        return Wait.UntilFindElement(Option(text));
    }

    // A click inside a dialog does not count as an outside click, but a second click on the multiselect
    // toggles the overlay closed. The multiselect ignores container clicks for 150 ms after the previous one,
    // so let that pass first.
    private void Close(string text)
    {
        Thread.Sleep(ClickGuardMilliseconds);
        Driver.FindElement(host).Click();
        Wait.UntilDisappear(Option(text));
    }

    private static bool IsOptionSelected(IWebElement option) => option.GetAttribute("aria-selected") == "true";

    private static By Option(string text) =>
        By.XPath($"//li[@role='option'][.//span[normalize-space()='{text}']]");
}

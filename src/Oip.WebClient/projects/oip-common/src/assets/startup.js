// Applies the theme saved by LayoutService to the startup screen before the first paint.
(function () {
  try {
    var config = JSON.parse(localStorage.getItem('layoutConfig') || 'null');
    if (config && typeof config.darkTheme === 'boolean') {
      document.documentElement.setAttribute('data-startup-theme', config.darkTheme ? 'dark' : 'light');
    }
  } catch (e) {
    // Storage is unavailable or the value is broken: the system theme is used.
  }
})();

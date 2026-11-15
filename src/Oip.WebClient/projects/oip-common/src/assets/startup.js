// Applies the theme and language saved by LayoutService to the startup screen before the first paint.
(function () {
  // Languages the startup screen has texts for; the first one is the application default.
  var startupLanguages = ['ru', 'en'];
  var language = startupLanguages[0];

  try {
    var config = JSON.parse(localStorage.getItem('layoutConfig') || 'null');
    if (config && typeof config.darkTheme === 'boolean') {
      document.documentElement.setAttribute('data-startup-theme', config.darkTheme ? 'dark' : 'light');
    }
    if (config && typeof config.language === 'string') {
      language = startupLanguages.indexOf(config.language) >= 0 ? config.language : 'en';
    }
  } catch (e) {
    // Storage is unavailable or the value is broken: the system theme and the default language are used.
  }

  document.documentElement.lang = language;
  document.documentElement.setAttribute('data-startup-lang', language);
})();
